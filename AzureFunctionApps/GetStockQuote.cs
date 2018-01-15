using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text;

namespace AzureFunctionApps
{
    public static class GetStockQuote
    {
        [FunctionName("GetStockQuote")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "GetStockQuote/{symbol}")]HttpRequestMessage req, string symbol, TraceWriter log)
        {
            log.Info($"C# HTTP trigger function GetStockQuote is processed for symbol {symbol}.");

            if(string.IsNullOrEmpty(symbol))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("symbol parameter is missing! ", Encoding.UTF8, "application/json")
                };
            }

            string stockQuoteEndPoint = GetEnvironmentVariable("StockQuoteEndPoint");
            stockQuoteEndPoint += symbol + "/quote";

            HttpClient httpClient = new HttpClient();
            var quoteResponse = await httpClient.GetAsync(stockQuoteEndPoint);
            quoteResponse.EnsureSuccessStatusCode();
            var responseText = await quoteResponse.Content.ReadAsStringAsync();
            
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseText, Encoding.UTF8, "application/json")
            };
        }

        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
