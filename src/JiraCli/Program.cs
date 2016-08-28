using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using JiraCli.Api;
using JiraCli.Configuration;
using JiraCli.ViewModels;
using Microsoft.Extensions.Configuration;

namespace JiraCli
{
    public class Options
    {
        [Option('c', "credentials", Required = true,
            HelpText = "Jira login and password separated by colon. Example \"login:pass\".")]
        public string JiraCredentials { get; set; }

        [Option('j', "jiraurl", Required = true, HelpText = "Jira site url.")]
        public string JiraUrl { get; set; }

        [Option('u', "users", Required = false, HelpText = "Jira user names separated by comma.")]
        public string Users { get; set; }

        [Option('d', "days", DefaultValue = 15, Required = false, HelpText = "Period in days for logs download.")]
        public int Days { get; set; }

        [Option('p', "period", Required = false,
            HelpText = "Period in time for logs download. Format yyyy/MM/dd-yyyy/MM/dd.")]
        public string PeriodInfo { get; set; }

        [Option('m', "month", DefaultValue = false, Required = false,
            HelpText = "TODO")]
        public bool PrevMonth { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        public IEnumerable<string> GetUsers()
            => Users?.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();

        public string GetLogin() => JiraCredentials.Split(':')[0];

        public string GetPassword() => JiraCredentials.Split(':')[1];

        public Period GetPeriod()
        {
            if (!string.IsNullOrWhiteSpace(PeriodInfo))
            {
                return Period.FromString(PeriodInfo);
            }

            if (PrevMonth)
            {
                return Period.ForPrevMonth();
            }

            return Period.FromDays(Days);
        }
    }

    class Program
    {
        private static AppConfiguration _configuration;

        private static int Main(string[] args)
        {
            InitConfiguration();

            //TODO: remove CommandLine dependency
            //TODO: use url, login, password from settings and not from args
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                return -1;
            }

            Period period = options.GetPeriod();
            Console.WriteLine("Downloading worklogs for period of {0}.", period);

            var jiraClient = new RestApiServiceClient(new HttpClientHandler(), options.JiraUrl, options.GetLogin(), options.GetPassword());
            var views = DownloadUsersWorklogs(jiraClient, options.GetUsers(), options.GetPeriod()).Result;

            foreach (var view in views)
            {
                Console.WriteLine("**** Logs for user \"{0}\".", view.Author.Name);
                view.Print(new PlainTextOutput(Console.Out));
                Console.WriteLine("******************************************************************");
                Console.WriteLine();
            }

            return 0;
        }

        private static void InitConfiguration()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddPersistenJsonFile("appsettings.json", true);

            var configRoot = configurationBuilder.Build();
            _configuration = new AppConfiguration();
            configRoot.Bind(_configuration);

            if (_configuration.IsFirstRun)
            {
                var jiraConfigSection = configRoot.GetSection(nameof(_configuration.JiraSettings));
                
                Console.WriteLine("Jira CommandLine interface setup.");
                Console.Write("Url: ");
                jiraConfigSection[nameof(_configuration.JiraSettings.BaseUrl)] = Console.ReadLine();

                Console.Write("Username: ");
                jiraConfigSection[nameof(_configuration.JiraSettings.Login)] = Console.ReadLine();

                Console.Write("Password: ");
                jiraConfigSection[nameof(_configuration.JiraSettings.Password)] = Console.ReadLine();

                //rebing configuration
                configRoot.Bind(_configuration);
            }
        }

        private static async Task<IEnumerable<ProjectViewModel>> DownloadUsersWorklogs(RestApiServiceClient client,
            IEnumerable<string> users, Period period)
        {
            //NOTE: downloading timesheets for users not in single search request but simultaneously to increase speed 
            var downloadTasks = users.Select(user => client.DownloadTimesheetAsync(period, user)).ToList();
            return await Task.WhenAll(downloadTasks);
        }
    }
}
