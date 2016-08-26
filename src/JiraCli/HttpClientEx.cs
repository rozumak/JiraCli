using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JiraCli
{
    public static class HttpClientEx
    {
        public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string url, object data)
        {
            string content = JsonConvert.SerializeObject(data);
            var jsonContent = new StringContent(content, Encoding.UTF8, "application/json");
            return client.PostAsync(url, jsonContent, CancellationToken.None);
        }

        public static Task<T> GetJsonAsync<T>(this HttpClient client, string url)
        {
            return client.GetAsync(url).ReceiveJsonAsync<T>();
        }

        public static async Task<T> ReceiveJsonAsync<T>(this Task<HttpResponseMessage> response)
        {
            HttpResponseMessage httpResponseMessage = await response.ConfigureAwait(false);

            var serializer = new JsonSerializer();

            using (Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }
    }
}