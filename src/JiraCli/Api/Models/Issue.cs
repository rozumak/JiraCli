using System.Collections.Generic;

namespace JiraCli.Api.Models
{
    public class Issue
    {
        public string expand { get; set; }
        public string key { get; set; }
        public string id { get; set; }
        public string self { get; set; }

        public string summary { get; set; }

        //public Dictionary<string, object> fields { get; set; }

        public List<Worklog> worklogs { get; set; } 
    }
}