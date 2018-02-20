using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using HumbleBundleServerless.Models;
using System.Linq;

namespace HumbleBundleServerless.Functions
{
    public static class RegisterWebhook
    {
        [FunctionName("RegisterWebhook")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequestMessage req,
            [Table("webhookRegistration")] IQueryable<WebhookRegistrationEntity> inTable,
            [Table("webhookRegistration")] ICollector<WebhookRegistrationEntity> outTable,
            TraceWriter log)
        {
            try
            {
                dynamic data = await req.Content.ReadAsAsync<object>();
                int type = data?.type;
                string webhook = data?.webhook;
                bool recieveUpdates = data?.sendUpdates;

                log.Info($"Recieved webhook registration for type {type}. RecieveUpdates? {recieveUpdates}");

                var lastBundleType = BundleTypes.SPECIAL;

                if (type < 0 || type > (int) lastBundleType)
                {
                    log.Error("Invalid type passed in");
                    return req.CreateResponse(HttpStatusCode.BadRequest,
                        "Please pass in a valid Bundle Type int (0 - " + (int) lastBundleType + ")");
                }

                var bundleType = (BundleTypes) type;

                if (webhook == null)
                {
                    log.Error("No webhook provided");
                    return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a webhook in the request body");
                }

                if (!webhook.Contains("discordapp.com/api/webhooks/"))
                {
                    log.Error("Invalid webhook provided");
                    return req.CreateResponse(HttpStatusCode.BadRequest,
                        "Please pass in a valid Discord Webhook URL in the request body");
                }

                if (inTable.ToList().Any(x => x.GetDecryptedWebhook() == webhook && x.BundleType == type))
                {
                    log.Error("Webhook already registered");
                    return req.CreateResponse(HttpStatusCode.Conflict,
                        "This webhook URL is already registered for this Bundle type");
                }

                outTable.Add(new WebhookRegistrationEntity(bundleType, webhook, recieveUpdates));

                return req.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return req.CreateResponse(HttpStatusCode.BadRequest, "Webhook registration failed - Exception: " + e.Message);
            }
        }
    }
}
