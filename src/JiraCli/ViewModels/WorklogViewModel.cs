using System;
using JiraCli.Api.Models;

namespace JiraCli.ViewModels
{
    public class WorklogViewModel
    {
        private readonly Worklog _worklog;

        public IssueViewModel Issue { get; set; }

        public AuthorViewModel Author { get; set; }

        public DateTime StarteDate => _worklog.started.Date;

        public int TimeSpendSeconds => _worklog.timeSpentSeconds;

        public WorklogViewModel(Worklog worklog)
        {
            _worklog = worklog;
        }
    }
}