using System;

namespace JiraCli.Api.Models
{
    public class Worklog
    {
        private string _authorName;
        public string self { get; set; }
        public Author author { get; set; }

        public Author updateAuthor { get; set; }
        public string comment { get; set; }

        public string timeSpent { get; set; }
        public int timeSpentSeconds { get; set; }

        public string id { get; set; }
        public string issueId { get; set; }

        public DateTime started { get; set; }

        public string authorName
        {
            get { return _authorName ?? author?.name; }
            set { _authorName = value; }
        }
    }

    public class Author
    {
        public string name { get; set; }
        public string key { get; set; }
        public string self { get; set; }
        public string displayName { get; set; }
    }
}