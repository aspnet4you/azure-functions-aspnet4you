using System.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System.Web.Http;
using AzureFunctionApps.Security;
using System.Security.Claims;

namespace AzureFunctionApps
{
    /// <summary>
    /// QueueOrder: Insert a POCO order object to Azure Queue Storage. Parameter order object must be defined as HttpTrigger.
    /// This is needed for the object to be deserialized automatically by Azure Function. Req object should not be defined at HttpTrigger.
    /// https://docs.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues
    /// </summary>
    public static class QueueOrder
    {
        [FunctionName("QueueOrder")]
        public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, 
            [HttpTrigger(AuthorizationLevel.User, "post", Route = null)] POCOOrder order, TraceWriter log)
        {
            ClaimsPrincipal claimsPrincipal;
            CustomValidator customValidator = new CustomValidator(log);
            if ((claimsPrincipal = customValidator.ValidateToken(req.Headers.Authorization)) == null)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Invalid Token!", Encoding.UTF8, "application/json")
                };
            }

            string[] allowedRoles = GetEnvironmentVariable("AllowedRoles").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);         

            if(customValidator.IsInRole(claimsPrincipal, allowedRoles) == false)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Invalid Roles!", Encoding.UTF8, "application/json")
                };
            }

            string msg = $"Hello " + order.CustomerName + "! Your order for " + order.ProductName + " with quantity of " + order.OrderCount + " has been received. You will receive email at " + order.CustomerEmail + " as soon as order is shipped.";
            log.Info(msg);

            var connectionString = GetEnvironmentVariable("AzureWebJobsStorage");

            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            CloudQueue queue = queueClient.GetQueueReference("orderqueue");

            // Create the queue if it doesn't already exist
            queue.CreateIfNotExists();

            // Serialize POCO Order
            var orderData = JsonConvert.SerializeObject(order, Formatting.Indented);

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage(orderData);
            queue.AddMessage(message);

            // Send an acknowledgement to client app
            return await Task.Run(() =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(msg, Formatting.Indented), Encoding.UTF8, "application/json")
                };
            });
        }

        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }

    /// <summary>
    /// This is just a simple/sample object class to demonstrate intake of parameter as POCO object.
    /// </summary>
    public class POCOOrder
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string ProductName { get; set; }
        public int OrderCount { get; set; }
    }
}
