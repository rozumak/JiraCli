using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraCli
{
    public class Table
    {
        public class ColumnHeader
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public List<string> Subheaders { get; set; }

            public ColumnHeader()
            {
                Subheaders = new List<string>();
            }

            public int MaxLength
            {
                get { return Math.Max(Name?.Length ?? 0, Subheaders.Select(sub => sub.Length).DefaultIfEmpty(0).Max()); }
            }
        }

        public List<ColumnHeader> Columns { get; protected set; }

        public List<object[]> Rows { get; protected set; }

        public List<object[]> FooterRows { get; protected set; }

        public Table(params object[] columns)
        {
            Columns = new List<ColumnHeader>(columns.Select(x => new ColumnHeader {Value = x}));
            Rows = new List<object[]>();
            FooterRows = new List<object[]>();
        }

        public Table AddColumn(IEnumerable<object> values)
        {
            foreach (var value in values)
                Columns.Add(new ColumnHeader {Value = value});
            return this;
        }

        public Table AddColumn(object value)
        {
            Columns.Add(new ColumnHeader {Value = value});
            return this;
        }

        public Table AddColumn(ColumnHeader column)
        {
            Columns.Add(column);
            return this;
        }

        public object[] AddRow(params object[] values)
        {
            return AddFooterRowImpl(Rows, values);
        }

        public object[] AddFooterRow(params object[] values)
        {
            return AddFooterRowImpl(FooterRows, values);
        }

        public object[] AddEmptyRow()
        {
            return AddEmptyRowImpl(Rows);
        }

        public object[] AddEmptyFooterRow()
        {
            return AddEmptyRowImpl(FooterRows);
        }

        public object[] AddEmptyRowImpl(List<object[]> rows)
        {
            if (!Columns.Any())
                throw new Exception("Please set the columns first");

            var emptyRow = new object[Columns.Count];
            rows.Add(emptyRow);
            return emptyRow;
        }

        private object[] AddFooterRowImpl(List<object[]> rows, object[] values)
        {
            if (Columns.Count < values.Length)
                throw new Exception(
                    $"The number columns in the row ({Columns.Count}) is less than values ({values.Length}");

            var row = AddEmptyRowImpl(rows);

            for (int i = 0; i < values.Length; i++)
            {
                row[i] = values[i];
            }
            return row;
        }
    }
}