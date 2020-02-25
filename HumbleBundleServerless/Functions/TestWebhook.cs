using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HumbleBundleBot;
using HumbleBundleServerless.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace HumbleBundleServerless.Functions
{
    public static class TestWebhook
    {
        [FunctionName("TestWebhook")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage req,
            [Table("humbleBundles")] IQueryable<HumbleBundleEntity> currentTableBundles,
            [Table("webhookRegistration")] IQueryable<WebhookRegistrationEntity> existingWebhooks,
            [Queue("jsonmessagequeue")] ICollector<JsonMessage> jsonMessageQueue,
            [Queue("discordmessagequeue")] ICollector<DiscordMessage> discordMessageQueue,
            TraceWriter log)
        {
            req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            dynamic data = await req.Content.ReadAsAsync<object>();
            string webhook = data?.webhook;
            int bundleTypeValue = data?.type;
            WebhookType webhookType = data?.webhookType;
            string bundleName = data?.bundleName;
            var bundleType = (BundleTypes)bundleTypeValue;

            if (webhook == null)
            {
                log.Error("No webhook provided");
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a webhook in the request body");
            }

            var webhookToTest = existingWebhooks.ToList().FirstOrDefault(x => x.WebhookType == (int)webhookType && x.GetDecryptedWebhook() == webhook);
            if (webhookToTest == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound, $"Could not find registered webhook {webhook} for type {webhookType}");
            }

            var currentBundles = currentTableBundles.ToList().Select(x => x.GetBundle()).ToList();
            var bundleToSend = currentBundles.First(x => x.Type == bundleType || bundleType == BundleTypes.ALL);

            if (!string.IsNullOrEmpty(bundleName))
            {
                bundleToSend = currentBundles.FirstOrDefault(x => (x.Type == bundleType || bundleType == BundleTypes.ALL) && x.Name.Equals(bundleName, System.StringComparison.CurrentCultureIgnoreCase));

                if (bundleToSend == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound, "Could not find bundle " + bundleName);
                }
            }

            var queuedBundle = new BundleQueue()
            {
                Bundle = bundleToSend,
                IsUpdate = false
            };

            if (webhookType == WebhookType.Discord)
            {
                discordMessageQueue.Add(new DiscordMessage
                {
                    WebhookUrl = webhook,
                    Payload = new DiscordWebhookPayload(queuedBundle, bundleToSend)
                });
            }
            else if (webhookType == WebhookType.RawJson)
            {
                jsonMessageQueue.Add(new JsonMessage()
                {
                    WebhookUrl = webhook,
                    Payload = queuedBundle
                });
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
