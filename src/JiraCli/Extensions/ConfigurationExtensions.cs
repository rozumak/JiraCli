using System;
using JiraCli.Configuration;
using Microsoft.Extensions.Configuration;

namespace JiraCli.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddPersistenJsonFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException(nameof(path));

            var configSource = new PersistenJsonConfigurationSource
            {
                Path = path,
                FileProvider = null,
                ReloadOnChange = false,
                Optional = optional
            };

            builder.Add(configSource);
            return builder;
        }
    }
}