using System;
using System.Collections.Generic;

namespace JiraCli.Api
{
    public class SearchRequestBuilder
    {
        private readonly List<string> _worklogAuthors = new List<string>();
        private DateTime? _worklogStartDate;
        private DateTime? _worklogEndDate;
        private string[] _includedFields = new string[0];

        public SearchRequestBuilder WorklogAuthor(string worklogAuthorLogin)
        {
            _worklogAuthors.Add(worklogAuthorLogin);
            return this;
        }

        public SearchRequestBuilder WorklogStartAt(DateTime dateTime)
        {
            _worklogStartDate = dateTime;
            return this;
        }

        public SearchRequestBuilder WorklogEndAt(DateTime dateTime)
        {
            _worklogEndDate = dateTime;
            return this;
        }

        public SearchRequestBuilder IncludeFields(params string[] fields)
        {
            _includedFields = fields;
            return this;
        }

        public ApiSearchRequest Build()
        {
            string searchQuery = string.Empty;

            if (_worklogAuthors.Count == 0)
            {
                throw new NotSupportedException(
                    "Other queries besides worklogs searches are not supported. So WorklogAuthors is mandatory field.");
            }

            searchQuery += $"(worklogAuthor IN ({string.Join(",", _worklogAuthors)}))";

            if (_worklogStartDate != null)
                searchQuery += $" AND worklogDate >= \"{_worklogStartDate.Value.ToString("yyyy/MM/dd")}\"";

            if (_worklogEndDate != null)
                searchQuery += $" AND worklogDate <= \"{_worklogEndDate.Value.ToString("yyyy/MM/dd")}\"";

            return new ApiSearchRequest {Search = searchQuery, IncludedFields = _includedFields};
        }
    }
}