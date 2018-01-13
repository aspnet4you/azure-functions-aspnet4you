using System;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace AzureFunctionApps
{
    public static class GetStockSymbols
    {
        [FunctionName("GetStockSymbols")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var connectionString = GetEnvironmentVariable("AzureWebJobsStorage");
            
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("stocksymbols");
            //CloudBlockBlob blockBlob = container.GetBlockBlobReference("stocksymbols.txt");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("symbols2.txt");

            log.Info($"BlockBlob name: {blockBlob.Name}");

            var jsonBlob = await blockBlob.DownloadTextAsync();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonBlob, Encoding.UTF8, "application/json")
            };
        }

        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
