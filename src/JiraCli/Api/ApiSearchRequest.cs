namespace JiraCli.Api
{
    public class ApiSearchRequest
    {
        public string Search { get; set; }
        public string[] IncludedFields { get; set; }
    }
}