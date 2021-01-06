using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PMDividendDataLoader.Models;
using Serilog;

namespace PMDividendDataLoader
{
    public static class AlphaVantageClient
    {
        public static async Task<List<DividendData>> GetDividendData(List<string> symbols, string apiKey)
        {
            var client = new HttpClient();
            List<DividendData> dividendData = new List<DividendData>();
            foreach (string symbol in symbols)
            {
                string endpoint = $"https://www.alphavantage.co/query?function=OVERVIEW&symbol=" + symbol +
                                  "&apikey=" + apiKey;
                var response = await client.GetAsync(endpoint);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                var fundementalData = JsonConvert.DeserializeObject<FundementalData>(jsonResponse);


                Double.TryParse(fundementalData?.DividendPerShare, out var divPerShare);
                Double.TryParse(fundementalData?.DividendYield, out var divYield);
                DateTime.TryParse(fundementalData?.ExDividendDate, out var exDate);
                DateTime.TryParse(fundementalData?.DividendDate, out var paymentDate);

                DividendData dividend = new DividendData()
                {
                    symbol = symbol,
                    dividendPerShare =  divPerShare,
                    yield = divYield,
                    exDate = exDate,
                    paymentDate = paymentDate
                };

                dividendData.Add(dividend);
                Log.Information($"Got Dividend Data for {symbol}");
                // Keep the AlphaVatange API happy by staying within the 5 requests per minute limit
                Thread.Sleep(20000);
            }

            return dividendData;
        }
    }
}
