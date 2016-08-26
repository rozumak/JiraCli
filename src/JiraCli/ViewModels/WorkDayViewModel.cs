using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraCli.ViewModels
{
    public class WorkDayViewModel
    {
        public List<WorklogViewModel> Worklogs { get; set; }

        public DateTime Date
        {
            get { return Worklogs.Select(x => x.StarteDate).First(); }
        }

        public double TotalHours
        {
            get { return TimeSpan.FromSeconds(Worklogs.Sum(x => x.TimeSpendSeconds)).TotalHours; }
        }

        public double GetTotalHoursByIssue(IssueViewModel issue)
        {
            return TimeSpan.FromSeconds(Worklogs.Where(x => x.Issue == issue).Sum(x => x.TimeSpendSeconds)).TotalHours;
        }
    }
}