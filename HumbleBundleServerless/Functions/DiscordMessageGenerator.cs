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
    public static class DiscordMessageGenerator
    {
        [FunctionName("DiscordMessageGenerator")]
        public static void Run(
            [QueueTrigger("bundlequeue")] BundleQueue queuedBundle,
            [Queue("discordmessagequeue")] ICollector<DiscordMessage> messageQueue,
            [Table("webhookRegistration")] IQueryable<WebhookRegistrationEntity> existingWebhooks,
            TraceWriter log)
        {
            log.Info($"Message generator trigger function processed: {queuedBundle.Bundle.Name}");

            var bundle = queuedBundle.Bundle;

            var webhooks = GetAllWebhooksForBundleType(existingWebhooks, queuedBundle.Bundle.Type, queuedBundle.IsUpdate);

            log.Info($"Found {webhooks.Count} webhooks for type {queuedBundle.Bundle.Type}");

            var content = "New Bundle: " + bundle.Name;

            if (queuedBundle.IsUpdate)
            {
                content = "Bundle Updated: " + bundle.Name;
            }

            var message = new DiscordWebhookPayload
            {
                content = content,
                embeds = new List<DiscordEmbed>()
            };

            message.embeds.Add(new DiscordEmbed()
            {
                url = bundle.URL,
                title = bundle.Description,
                image = new ImageField()
                {
                    url = bundle.ImageUrl
                },
                author = new AuthorField()
                {
                    name = "Humble Bundle",
                    url = bundle.URL
                }
            });

            foreach (var section in bundle.Sections)
            {
                var embed = new DiscordEmbed
                {
                    title = section.Title,
                    url = bundle.URL,
                    description = ""
                };

                var itemsAdded = 0;

                foreach (var item in section.Items)
                {
                    embed.description += GetItemName(item, queuedBundle.UpdatedItems);
                    itemsAdded++;

                    // Create a new embed every 25 items
                    if (itemsAdded % 25 == 0)
                    {
                        message.embeds.Add(embed);
                        embed = new DiscordEmbed
                        {
                            title = section.Title + " (Continued)",
                            url = bundle.URL,
                            description = ""
                        };
                    }
                }

                // Add last embed
                message.embeds.Add(embed);
            }

            log.Info("Created message " + JsonConvert.SerializeObject(message));

            foreach (var webhook in webhooks)
            {
                if (!string.IsNullOrEmpty(webhook.Partner))
                {
                    AddPartnerLink(message, webhook.Partner);
                }

                messageQueue.Add(new DiscordMessage
                {
                    WebhookUrl = webhook.GetDecryptedWebhook(),
                    Payload = message
                });
            }
        }

        private static void AddPartnerLink(DiscordWebhookPayload message, string partner)
        {
            foreach (var embed in message.embeds)
            {
                if (!string.IsNullOrEmpty(embed.url))
                {
                    embed.url += "?partner=" + partner;
                }

                if (embed.author != null && !string.IsNullOrEmpty(embed.author.url))
                {
                    embed.author.url += "?partner=" + partner;
                }
            }
        }

        private static string GetItemName(HumbleItem item, List<HumbleItem> updated)
        {
            if (updated.Any(x => x.Name == item.Name))
            {
                return "[New] " + item.Name + "\n";
            }
            return item.Name + "\n";
        }

        private static List<WebhookRegistrationEntity> GetAllWebhooksForBundleType(IQueryable<WebhookRegistrationEntity> existingWebhooks, BundleTypes type, bool isUpdate)
        {
            var webhooksForType = existingWebhooks.Where(x => x.PartitionKey == type.ToString()).ToList();

            var discordHooks = webhooksForType.Where(x => x.WebhookType == (int) WebhookType.Discord);

            if (isUpdate)
            {
                discordHooks = discordHooks.Where(x => x.ShouldRecieveUpdates);
            }

            return discordHooks.ToList();
        }
    }
}
