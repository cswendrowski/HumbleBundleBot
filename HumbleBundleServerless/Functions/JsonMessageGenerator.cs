using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using HumbleBundleBot;
using HumbleBundleServerless.Models;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HumbleBundleServerless
{
    public static class JsonMessageGenerator
    {
        [FunctionName("JsonMessageGenerator")]
        public static void Run(
            [QueueTrigger("jsonbundlequeue")] BundleQueue queuedBundle,
            [Queue("jsonmessagequeue")] ICollector<JsonMessage> messageQueue,
            [Table("webhookRegistration")] IQueryable<WebhookRegistrationEntity> existingWebhooks,
            TraceWriter log)
        {
            log.Info($"JSON Message generator trigger function processed: {queuedBundle.Bundle.Name}");

            var webhooks = GetAllWebhooksForBundleType(existingWebhooks, queuedBundle.Bundle.Type, queuedBundle.IsUpdate);

            log.Info($"Found {webhooks.Count} webhooks for type {queuedBundle.Bundle.Type}");

            foreach (var webhook in webhooks)
            {
                messageQueue.Add(new JsonMessage()
                {
                    WebhookUrl = webhook,
                    Payload = queuedBundle
                });
            }
        }

        private static List<String> GetAllWebhooksForBundleType(IQueryable<WebhookRegistrationEntity> existingWebhooks, BundleTypes type, bool isUpdate)
        {
            var webhooksForType = existingWebhooks.Where(x => x.PartitionKey == type.ToString()).ToList();

            var discordHooks = webhooksForType.Where(x => x.WebhookType == (int)WebhookType.RawJson);

            if (isUpdate)
            {
                discordHooks = discordHooks.Where(x => x.ShouldRecieveUpdates);
            }

            return discordHooks.ToList().Select(x => x.GetDecryptedWebhook()).ToList();
        }
    }
}
