using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Clickatell.AzFunction.DeliveryCallback
{
    public static class HttpTriggerCSharp
    {
        [FunctionName("HttpTriggerCSharp")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            log.Flush();
            foreach (var item in req.GetQueryNameValuePairs())
            {
                log.Info(item.Key + ": " + item.Value + ", ");
            }
            log.Flush();
            // parse query parameter
            string to = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "to", true) == 0)
                .Value;

            string status = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "status", true) == 0)
                .Value;

            string integrationName = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "integrationName", true) == 0)
                .Value;
            
            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            to = to ?? data?.to;
            status = status ?? data?.status;
            integrationName = integrationName ?? data?.integrationName;

            log.Info($"integrationName = {integrationName}, to = {to}, status {status}");

            return to == null || status == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a to and status on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, "Sent to " + to + " - Status: " + status);
        }
    }
}
