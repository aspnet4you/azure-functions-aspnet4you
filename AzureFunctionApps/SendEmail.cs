using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace AzureFunctionApps
{
    /// <summary>
    /// SendEmail: This function is triggered when an order is queued into Azure Queue Storage. Additionally, this function sends Email
    /// using SendGrid and IAsyncCollector<T> as output parameter. All you have to do is- add a message to output parameter!
    /// </summary>
    public static class SendEmail
    {
        [FunctionName("SendEmail")]
        public static async void Run([QueueTrigger("orderqueue", Connection = "AzureWebJobsStorage")]POCOOrder order, 
            TraceWriter log, [SendGrid( ApiKey = "AzureWebJobsSendGridApiKey")] IAsyncCollector<Mail>  outputMessage)
        {
            string msg = $"Hello " + order.CustomerName + "! Your order for " + order.ProductName + " with quantity of " + order.OrderCount + " has been shipped.";
            log.Info(msg);

            Mail message = new Mail
            {
                Subject = msg,
                From = new Email("noreply@aspnet4you.com", "No Reply"),
                ReplyTo= new Email("Prodip.Kumar@outlook.com", "Prodip Saha")
            };

            
            var personalization = new Personalization();
            // Change To email of recipient
            personalization.AddTo(new Email(order.CustomerEmail)); 

            Content content = new Content
            {
                Type = "text/plain",
                Value = msg + "Thank you for being such a nice customer!"
            };
            message.AddContent(content);
            message.AddPersonalization(personalization);
            

            await outputMessage.AddAsync(message);
        }
    }
}
