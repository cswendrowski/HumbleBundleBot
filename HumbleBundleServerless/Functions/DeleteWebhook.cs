using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using HumbleBundleServerless.Models;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace HumbleBundleServerless.Functions
{
    public static class DeleteWebhook
    {
        [FunctionName("DeleteWebhook")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete")]HttpRequestMessage req,
            [Table("webhookRegistration")] IQueryable<WebhookRegistrationEntity> existingWebhooks,
            [Table("webhookRegistration")] CloudTable webhookRegistrationTable,
            TraceWriter log)
        {
            dynamic data = await req.Content.ReadAsAsync<object>();
            int type = data?.type;
            string webhook = data?.webhook;
            WebhookType webhookTypeValue = data?.webhookType;

            var lastBundleType = BundleTypes.SPECIAL;

            if (type < 0 || type > (int) lastBundleType)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass in a valid Bundle Type int (0 - " + (int)lastBundleType + ")");
            }

            var bundleType = (BundleTypes)type;

            if (webhook == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a webhook in the request body");
            }

            if (webhookTypeValue == WebhookType.Discord)
            {
                if (!webhook.Contains("discordapp.com/api/webhooks/"))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest,
                        "Please pass in a valid Discord Webhook URL in the request body");
                }
            }

            var entity = existingWebhooks.ToList().FirstOrDefault(x => x.BundleType == type && x.GetDecryptedWebhook() == webhook);

            if (entity == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound, "Could not find a registered Webhook for the given Bundle Type");
            }

            var deleteOperation = TableOperation.Delete(entity);
            webhookRegistrationTable.Execute(deleteOperation);

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
