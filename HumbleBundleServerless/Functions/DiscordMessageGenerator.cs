using HumbleBundleBot;
using HumbleBundleServerless.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;

namespace HumbleBundleServerless
{
    public static class DiscordMessageGenerator
    {
        [FunctionName("DiscordMessageGenerator")]
        public static void Run(
            [QueueTrigger("bundlequeue")] BundleQueue queuedBundle,
            [Queue("discordmessagequeue")] ICollector<DiscordMessage> messageQueue,
            [Table("webhookRegistration")] CloudTable existingWebhooks,
            TraceWriter log)
        {
            log.Info($"Message generator trigger function processed: {queuedBundle.Bundle.Name}");

            var bundle = queuedBundle.Bundle;
            var webhooks = existingWebhooks.GetAllWebhooksForBundleType(queuedBundle.Bundle.Type, queuedBundle.IsUpdate);
            int queued = 0;

            foreach (var webhook in webhooks)
            {
                var message = new DiscordWebhookPayload(queuedBundle, bundle);

                if (!string.IsNullOrEmpty(webhook.Partner))
                {
                    message.AddPartnerLink(webhook.Partner);
                }

                messageQueue.Add(new DiscordMessage
                {
                    WebhookUrl = webhook.GetDecryptedWebhook(),
                    Payload = message
                });
                queued++;
            }

            log.Info($"Queued {queued} payloads for type {queuedBundle.Bundle.Type}");
        }

    }
}
