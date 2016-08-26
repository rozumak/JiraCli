namespace JiraCli.Api.Models
{
    public class SearchRequest
    {
        public string jql { get; set; }
        public int startAt { get; set; }
        public int maxResults { get; set; }
        public string[] fields { get; set; }
        public bool? validateQuery { get; set; }
    }
}