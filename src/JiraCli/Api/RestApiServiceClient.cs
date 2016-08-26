using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using JiraCli.Api.Models;

namespace JiraCli.Api
{
    public class RestApiServiceClient
    {
        private const string SearchApi = "rest/api/2/search";
        private readonly HttpClient _httpClient;

        public RestApiServiceClient(HttpMessageHandler httpMessageHandler, string baseUrl, string login, string password)
        {
            _httpClient = new HttpClient(httpMessageHandler) {BaseAddress = new Uri(baseUrl)};

            //add basic authorization header
            var byteArray = Encoding.UTF8.GetBytes($"{login}:{password}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(byteArray));
        }

        public async Task<IEnumerable<Issue>> GetIssuesAsync(ApiSearchRequest request)
        {
            List<Issue> result = new List<Issue>();

            var downloadWorklogs = new List<Task<IEnumerable<Worklog>>>();
            int startAt = 0;
            dynamic response;
            do
            {
                response = await _httpClient
                    .PostJsonAsync(SearchApi, new SearchRequest
                    {
                        startAt = startAt,
                        jql = request.Search,
                        maxResults = 50,
                        fields = request.IncludedFields
                    })
                    .ReceiveJsonAsync<dynamic>();

                foreach (var issue in response.issues)
                {
                    var issueObj = new Issue
                    {
                        id = issue.id,
                        expand = issue.expand,
                        worklogs = new List<Worklog>(),
                        key = issue.key,
                        self = issue.self,
                        summary = issue.fields.summary
                    };
                    result.Add(issueObj);

                    int maxResults = issue.fields.worklog.maxResults;
                    if (issue.fields.worklog.total <= maxResults)
                    {
                        foreach (var worklog in issue.fields.worklog.worklogs)
                        {
                            issueObj.worklogs.Add(new Worklog
                            {
                                timeSpentSeconds = worklog.timeSpentSeconds,
                                self = worklog.self,
                                id = worklog.id,
                                started = worklog.started,
                                issueId = worklog.issueId,
                                comment = worklog.comment,
                                timeSpent = worklog.timeSpent,
                                authorName = worklog.author.name
                            });
                        }

                    }
                    else
                    {
                        var downloadWorklogsTask = GetAllWorklogsByIssue(issueObj.id);
                        downloadWorklogs.Add(downloadWorklogsTask);
                    }
                }

                startAt = response.maxResults + startAt;
            } while (response.maxResults + response.startAt <= response.total);

            var additionalWorklogs = (await Task.WhenAll(downloadWorklogs))
                .SelectMany(x => x)
                .GroupBy(x => x.issueId);

            foreach (var worklogs in additionalWorklogs)
            {
                result.Single(x => x.id == worklogs.Key).worklogs.AddRange(worklogs);
            }

            return result;
        }

        private async Task<IEnumerable<Worklog>> GetAllWorklogsByIssue(string issueId)
        {
            string getIssueWorklogApi = "/rest/api/2/issue/{0}/worklog";

            //NOTE: here we don't have pagination, 
            //documentation says that all worklogs will be returned by single request https://docs.atlassian.com/jira/REST/latest/#api/2/issue-getWorklog
            var result = await _httpClient.GetJsonAsync<WorklogResponse>(string.Format(getIssueWorklogApi, issueId));
            return result.worklogs;
        }
    }
}