// See https://aka.ms/new-console-template for more information
using System.ComponentModel.DataAnnotations;

namespace StooqBot
{
    public class StooqBot
    {
        private const string BASE_URL = "https://stooq.com/q/l/?s=";
        private const string URL_PARAMS = ".us&f=sd2t2ohlcv&h&e=csv";
        private StooqApi stooqApi;

        public StooqBot()
        {
            stooqApi = new StooqApi();
        }

        public async Task<string> ExecuteRequest(string keyword)
        {
            string url = BASE_URL + keyword + URL_PARAMS;

            var rawData = await stooqApi.GetStockDataAsync(url);

            var isValidData = validateStockData(rawData);

            return isValidData ? rawData : "Data not found";
        }

        public bool validateStockData(string data)
        {
            return !data.Contains("N/D");
        }


        // Demo using command line
        // static async Task Main(string[] args)
        // {
        //     var bot = new StooqBot();
        //     Console.Write("Enter stock symbol: ");
        //     string symbol = Console.ReadLine();
        //     string result = await bot.ExecuteRequest(symbol);
        //     Console.WriteLine("\nResult:\n" + result);
        // }
    }
}