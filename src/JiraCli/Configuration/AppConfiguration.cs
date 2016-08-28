namespace JiraCli.Configuration
{
    public class AppConfiguration
    {
        public JiraConfiguration JiraSettings { get; set; }

        public bool IsFirstRun => JiraSettings == null;

        public class JiraConfiguration
        {
            public string BaseUrl { get; set; }

            public string Login { get; set; }

            public string Password { get; set; }
        }
    }
}