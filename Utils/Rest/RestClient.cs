using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rest
{
    public class RestClient
    {
        private readonly HttpClient client;

        public RestClient()
        {
            this.client = new HttpClient();
        }

        public async Task<string> GetStringAsync(string uri)
        {
            var result = await this.client.GetAsync(uri);

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Get request failed");
            }

            var resultedContent = await result.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(resultedContent))
            {
                throw new Exception("Nothing to return");
            }

            return resultedContent;
        }
    }
}
