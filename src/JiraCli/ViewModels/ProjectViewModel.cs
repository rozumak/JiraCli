using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JiraCli.Extensions;

namespace JiraCli.ViewModels
{
    public class ProjectViewModel
    {
        private const int WorkDayHours = 8;
        private const int WorkingDaysInWeek = 5;

        private readonly Period _period;

        public List<IssueViewModel> Issues { get; }

        public List<WorkDayViewModel> WorkingDays { get; }

        public AuthorViewModel Author { get; }

        public double TotalHoursInPeriod => WorkingDays.Select(x => x.TotalHours).Sum();

        public double ExpectedTotalHoursInPeriod
            =>
                WorkingDays.Count(
                    d =>
                        !d.Date.IsWeekendDay(WorkingDaysInWeek,
                            CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek))*WorkDayHours;

        public ProjectViewModel(IEnumerable<IssueViewModel> issues, IEnumerable<WorkDayViewModel> workingDays,
            AuthorViewModel author, Period period)
        {
            _period = period;
            Issues = new List<IssueViewModel>(issues);
            WorkingDays = new List<WorkDayViewModel>(workingDays);
            Author = author;
        }

        public void Print(ITextOutput output)
        {
            Print(output, new TableFormatter());
        }

        public void Print(ITextOutput output, TableFormatter tableFormatter)
        {
            var table = CreateTable();

            foreach (var issueView in Issues)
            {
                object[] currentRow = table.AddRow(issueView.Name);
                for (int i = 1; i < table.Columns.Count; i++)
                {
                    var currentWorkDay = WorkingDays.SingleOrDefault(x => x.Date.Equals(table.Columns[i].Value));
                    double? totalHoursPerIssue = currentWorkDay?.GetTotalHoursByIssue(issueView);
                    // TODO: move tostring into formatter it doesn't belongs here
                    currentRow[i] = totalHoursPerIssue > 0 ? totalHoursPerIssue.Value.ToString("0.##") : null;
                }
            }

            var currentFooterRow = table.AddFooterRow("Total hours:");
            for (int i = 1; i < table.Columns.Count; i++)
            {
                var day = WorkingDays.SingleOrDefault(x => x.Date.Equals(table.Columns[i].Value));
                //TODO: move tostring into formatter it doesn't belongs here
                currentFooterRow[i] = day?.TotalHours.ToString("0.##");
            }

            //TODO: move tostring into formatter it doesn't belongs here
            table.AddFooterRow($"Total hours for period logged/expected: {TotalHoursInPeriod.ToString("0.##")}" +
                               $"/{ExpectedTotalHoursInPeriod.ToString("0.##")}");

            tableFormatter.Write(table, output);
        }

        private Table CreateTable()
        {
            var table = new Table();
            table.AddColumn(new Table.ColumnHeader());
            foreach (var day in _period.EnumerateDays())
            {
                table.AddColumn(new Table.ColumnHeader
                {
                    Value = day,
                    Name = day.Day.ToString(),
                    Subheaders = {day.DayOfWeek.ToString().Substring(0, 1).ToUpper()}
                });
            }

            return table;
        }
    }
}