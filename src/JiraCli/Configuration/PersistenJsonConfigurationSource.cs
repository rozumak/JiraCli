using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace JiraCli.Configuration
{
    public class PersistenJsonConfigurationSource : JsonConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider();
            return new PersistenJsonConfigurationProvider(this);
        }
    }
}