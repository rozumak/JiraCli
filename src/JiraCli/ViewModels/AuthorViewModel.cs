namespace JiraCli.ViewModels
{
    public class AuthorViewModel
    {
        public string Name { get; set; }

        public AuthorViewModel(string name)
        {
            Name = name;
        }
    }
}