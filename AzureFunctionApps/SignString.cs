using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Text;
using System;
using System.Web;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace AzureFunctionApps
{
    /// <summary>
    /// References:
    /// https://docs.microsoft.com/en-us/rest/api/storageservices/authorize-with-shared-key
    /// https://github.com/Azure-Samples/storage-dotnet-rest-api-with-auth/blob/master/StorageRestApiAuth/AzureStorageAuthenticationHelper.cs
    /// </summary>
    public static class SignString
    {
        [FunctionName("SignString")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request- " + req.RequestUri.PathAndQuery);

            HttpClient httpClient = new HttpClient();
            string clientIPHeader = GetIpFromRequestHeaders(req);
            log.Info($"Request processed- {req.RequestUri.PathAndQuery} and clientip is {clientIPHeader}.");

            var content = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            ReqPayload payload = JsonConvert.DeserializeObject<ReqPayload>(content);

            byte[] bKey = Convert.FromBase64String(payload.Payload.stringkey);
            byte[] signatureBytes = Encoding.UTF8.GetBytes(payload.Payload.stringtosign);


            HMACSHA256 hmac = new HMACSHA256(bKey);
            byte[] signatureHash = hmac.ComputeHash(signatureBytes);

            string base64Signature = Convert.ToBase64String(signatureHash);


            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(base64Signature, Encoding.UTF8, "application/json")
            };
        }

        private static string GetIpFromRequestHeaders(HttpRequestMessage request)
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<string> values;

            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                HttpContextWrapper httpCtxWraper = request.Properties["MS_HttpContext"] as HttpContextWrapper;
                sb.AppendFormat($"{httpCtxWraper.Request.UserHostAddress}");
            }
            else if (request.Headers.TryGetValues("X-Forwarded-For", out values))
            {
                if (values != null)
                {
                    sb.AppendFormat($"{string.Join("|", values)}");
                }
            }

            return sb.ToString();
        }


        private class Payload
        {
            public string stringtosign { get; set; }
            public string stringkey { get; set; }
        }


        private class ReqPayload
        {
            public Payload Payload { get; set; }
        }

    }
}
