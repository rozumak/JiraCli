using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraCli
{
    public class TableFormatter
    {
        public virtual void Write(Table table, ITextOutput output)
        {
            var rows = table.Rows.Select(x => x.Select(r => r?.ToString()).ToArray())
                .Concat(table.FooterRows.Select(x => x.Select(r => r?.ToString()).ToArray()));

            //find the longest column by searching each row
            var columnLengths = ColumnLengths(table.Columns, rows);

            //create the string format with padding
            var format = Format(columnLengths);

            //format column headers
            var columnHeaders = string.Format(format, table.Columns.Select(x => (object) x.Name).ToArray());
            int maxColumnSubheaders = table.Columns.Select(c => c.Subheaders.Count).DefaultIfEmpty(0).Max();
            var formattedColumSubheaders = new List<string>(maxColumnSubheaders);
            for (int i = 0; i < maxColumnSubheaders; i++)
            {
                var currentIndex = i;
                string subheader = string.Format(format, table.Columns.Select(
                    c => c.Subheaders.Count > currentIndex ? (object) c.Subheaders[currentIndex] : string.Empty)
                    .ToArray());
                formattedColumSubheaders.Add(subheader);
            }

            //create the divider
            var longestLine = columnHeaders.Length;
            var divider = " " + string.Join("", Enumerable.Repeat("-", longestLine - 1)) + " ";

            //add each row
            var formattedRows = table.Rows.Select(row => string.Format(format, row)).ToList();
            var formattedFooterRows = table.FooterRows.Select(row => string.Format(format, row)).ToList();

            output.WriteLine(divider);
            output.WriteLine(columnHeaders);
            foreach (var formattedColumSubheader in formattedColumSubheaders)
            {
                output.WriteLine(formattedColumSubheader);
            }

            output.WriteLine(divider);
            foreach (var row in formattedRows)
            {
                output.WriteLine(row);
            }

            output.WriteLine(divider);
            foreach (var row in formattedFooterRows)
            {
                output.WriteLine(row);
            }

            output.WriteLine(divider);
        }

        private string Format(int[] columnLengths)
        {
            var format = (Enumerable.Range(0, columnLengths.Length)
                .Select(i => " | {" + i + ",-" + columnLengths[i] + "}")
                .Aggregate((s, a) => s + a) + " |").Trim();
            return format;
        }

        private int[] ColumnLengths(IEnumerable<Table.ColumnHeader> columns, IEnumerable<string[]> rows)
        {
            var columnLengths = columns.Select(column => column.MaxLength).ToList();

            var resultLengths = columnLengths
                .Select((t, i) => Math.Max(t, rows.Select(x => x[i]?.ToString().Length ?? 0).DefaultIfEmpty(0).Max()))
                .ToArray();

            return resultLengths;
        }
    }
}