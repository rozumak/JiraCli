using System.Collections.Generic;
using JiraCli.Api.Models;

namespace JiraCli.ViewModels
{
    public class IssueViewModel
    {
        private readonly Issue _issue;

        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(_issue.summary))
                    return _issue.key + " - " + _issue.summary;
                return _issue.key;
            }
        }

        public List<WorklogViewModel> Worklogs { get; set; }

        public IssueViewModel(Issue issue)
        {
            _issue = issue;
        }
    }
}