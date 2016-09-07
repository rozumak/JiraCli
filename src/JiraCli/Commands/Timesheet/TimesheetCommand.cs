using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JiraCli.Api;
using JiraCli.Configuration;
using JiraCli.ViewModels;

namespace JiraCli.Commands.Timesheet
{
    public partial class TimesheetCommand
    {
        public Period Period { get; set; }

        public string[] Users { get; set; }

        public string OutputFileName { get; set; }

        public void Run(JiraConfiguration settings, TextWriter output)
        {
            if (Period == null || Users == null)
                throw new InvalidOperationException(
                    $"{nameof(Period)} and {nameof(Users)} must be initialized before running command.");

            if (Users.Length == 0)
                throw new InvalidOperationException($"{nameof(Users)} must contain at least one user.");

            output.WriteLine("Downloading worklogs for period of {0}.", Period);

            var jiraClient = new RestApiServiceClient(new HttpClientHandler(), settings.BaseUrl,
                settings.Login,
                settings.Password);
            var views = DownloadTimesheetAsync(jiraClient, Users, Period).Result;

            output.WriteLine();

            using (var fileWriter = GetFileWriter(OutputFileName))
            {
                var compositOutput = new CompositTextWriter(fileWriter, output);
                //printing into output
                foreach (var view in views)
                {
                    compositOutput.WriteLine("Worklogs for user \"{0}\":", view.Author.Name);
                    view.Print(new PlainTextOutput(compositOutput));
                    compositOutput.WriteLine();
                }
            }
        }

        private async Task<IEnumerable<ProjectViewModel>> DownloadTimesheetAsync(RestApiServiceClient client,
            IEnumerable<string> users, Period period)
        {
            //NOTE: downloading timesheets for users not in single search request but simultaneously to increase speed 
            var downloadTasks = users.Select(user => DownloadTimesheetImplAsync(client, period, user)).ToList();
            return await Task.WhenAll(downloadTasks);
        }

        private async Task<ProjectViewModel> DownloadTimesheetImplAsync(RestApiServiceClient client,
            Period period,
            params string[] users)
        {
            var issuesRequest = BuildRequest(period, users);

            if (users.Length != 1)
                throw new NotImplementedException();

            //NOTE: our jira search request return all issues that match our request,
            //but this issues also may include worklogs that are not in requested period or for requested author
            //worklogs must be filtered here to match user request
            var issuesWithWorklogs = await client.GetIssuesAsync(issuesRequest);

            //mapping models to view models
            var authorViewModel = new AuthorViewModel(users[0]);
            var issuesViewModels = new List<IssueViewModel>();
            foreach (var issue in issuesWithWorklogs)
            {
                var issueViewModel = new IssueViewModel(issue);
                issueViewModel.Worklogs = issue.worklogs
                    .Where(w => authorViewModel.Name.Equals(w.authorName, StringComparison.InvariantCultureIgnoreCase))
                    .Where(w => w.started.Date >= period.StartDate && w.started.Date <= period.EndDate)
                    .Select(w =>
                    {
                        var worklogViewModel = new WorklogViewModel(w)
                        {
                            Issue = issueViewModel,
                            Author = authorViewModel
                        };
                        return worklogViewModel;
                    }).ToList();

                issuesViewModels.Add(issueViewModel);
            }

            var workDayViewModels = issuesViewModels.SelectMany(issue => issue.Worklogs)
                .GroupBy(w => w.StarteDate)
                .Select(g => new WorkDayViewModel { Worklogs = g.ToList() })
                .ToList();

            var projectViewModel = new ProjectViewModel(issuesViewModels, workDayViewModels, authorViewModel, period);
            return projectViewModel;
        }

        private ApiSearchRequest BuildRequest(Period period, string[] users)
        {
            var issuesRequestBuilder = new SearchRequestBuilder()
                .WorklogStartAt(period.StartDate)
                .WorklogEndAt(period.EndDate)
                .IncludeFields("summary", "worklog", "assignee", "status", "key");

            foreach (var user in users)
            {
                issuesRequestBuilder.WorklogAuthor(user);
            }

            return issuesRequestBuilder.Build();
        }

        private TextWriter GetFileWriter(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return TextWriter.Null;

            FileStream stream = new FileStream(fileName, FileMode.Create);
            return new StreamWriter(stream);
        }
    }
}
