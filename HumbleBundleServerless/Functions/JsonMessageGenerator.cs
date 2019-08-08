using HumbleBundleBot;
using HumbleBundleServerless.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;

namespace HumbleBundleServerless
{
    public static class JsonMessageGenerator
    {
        [FunctionName("JsonMessageGenerator")]
        public static void Run(
            [QueueTrigger("jsonbundlequeue")] BundleQueue queuedBundle,
            [Queue("jsonmessagequeue")] ICollector<JsonMessage> messageQueue,
            [Table("webhookRegistration")] CloudTable existingWebhooks,
            TraceWriter log)
        {
            log.Info($"JSON Message generator trigger function processed: {queuedBundle.Bundle.Name}");

            var webhooks = existingWebhooks.GetAllWebhooksForBundleType(queuedBundle.Bundle.Type, queuedBundle.IsUpdate);

            log.Info($"Found {webhooks.Count()} webhooks for type {queuedBundle.Bundle.Type}");

            foreach (var webhook in webhooks)
            {
                if (!string.IsNullOrEmpty(webhook.Partner))
                {
                    queuedBundle.Bundle.URL += "?partner=" + webhook.Partner;
                }

                messageQueue.Add(new JsonMessage()
                {
                    WebhookUrl = webhook.GetDecryptedWebhook(),
                    Payload = queuedBundle
                });
            }
        }
    }
}
