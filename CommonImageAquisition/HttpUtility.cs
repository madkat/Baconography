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
        public static Task<string> Get(string uri)
        {
            return _httpClient.GetStringAsync(uri);
        }
    }
}
