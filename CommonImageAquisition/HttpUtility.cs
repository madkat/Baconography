using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CommonImageAquisition
{
    class HttpClientUtility
    {
        private static HttpClient _httpClient = new HttpClient(new HttpClientHandler());
        public static async Task<string> Get(string uri, bool ignoreErrors = false)
        {
            if (ignoreErrors)
            {
                var httpRequest = await _httpClient.GetAsync(uri);
                return await httpRequest.Content.ReadAsStringAsync();
            }
            else
                return await _httpClient.GetStringAsync(uri);
        }
    }
}
