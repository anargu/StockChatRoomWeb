
// Generate a class of StooqAPI, it should make a http call to stooq, the function receives the url to make the call and return the data
using System.Net.Http;
using System.Threading.Tasks;

namespace StooqBot
{
    public class StooqApi
    {
        private readonly HttpClient _httpClient;

        public StooqApi()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetStockDataAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}