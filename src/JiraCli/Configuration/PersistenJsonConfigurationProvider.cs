using System.IO;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;

namespace JiraCli.Configuration
{
    public class PersistenJsonConfigurationProvider : JsonConfigurationProvider
    {
        public PersistenJsonConfigurationProvider(JsonConfigurationSource source) : base(source)
        {
        }

        /// <summary>
        /// NOTE: Updating file on every Set method call.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public override void Set(string key, string value)
        {
            base.Set(key, value);

            var fileInfo = Source.FileProvider.GetFileInfo(Source.Path);

            using (var stream = new FileStream(fileInfo.PhysicalPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                //flush all data to the file
                //TODO: write hierarchy json view
                SerializeToStream(stream, Data);
            }
        }

        public static void SerializeToStream(Stream stream, object value)
        {
            var serializer = new JsonSerializer();

            using (var sw = new StreamWriter(stream))
            using (var jsonWritter = new JsonTextWriter(sw) {Formatting = Formatting.Indented})
            {
                serializer.Serialize(jsonWritter, value);
            }
        }
    }
}