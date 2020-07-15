using HumbleBundleBot;
using HumbleBundleServerless.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;

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
            var webhooks = existingWebhooks.GetAllWebhooksForBundleType(queuedBundle.Bundle.Type, queuedBundle.IsUpdate).ToList();

            if (queuedBundle.Bundle.Type == BundleTypes.BOOKS)
            {
                var isComicOrRpg = false;

                // If the same webhook is registered as RPG / COMIC, remove it unless this is the right type
                if (bundle.Name.ToLower().Contains("rpg"))
                {
                    var rpgWebhooks = existingWebhooks.GetAllWebhooksForBundleType(BundleTypes.RPG, queuedBundle.IsUpdate);
                    webhooks.AddRange(rpgWebhooks);
                    isComicOrRpg = true;
                }

                if (!bundle.Name.ToLower().Contains("comic"))
                {
                    var comicWebhooks = existingWebhooks.GetAllWebhooksForBundleType(BundleTypes.COMIC, queuedBundle.IsUpdate);
                    webhooks.AddRange(comicWebhooks);
                    isComicOrRpg = true;
                }

                if (!isComicOrRpg)
                {
                    var bookOtherWebhooks = existingWebhooks.GetAllWebhooksForBundleType(BundleTypes.BOOK_OTHER, queuedBundle.IsUpdate);
                    webhooks.AddRange(bookOtherWebhooks);
                }
            }
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
