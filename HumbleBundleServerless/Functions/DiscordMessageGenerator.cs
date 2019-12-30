using HumbleBundleBot;
using HumbleBundleServerless.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            var webhooks = existingWebhooks.GetAllWebhooksForBundleType(queuedBundle.Bundle.Type, queuedBundle.IsUpdate);
            int queued = 0;

            foreach (var webhook in webhooks)
            {
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
                    },
                    fields = new List<EmbedField>()
                {
                    new EmbedField
                    {
                        name = "Powered By",
                        value = "https://github.com/cswendrowski/HumbleBundleBot"
                    }
                }
                });

                var random = new Random();

                foreach (var section in bundle.Sections)
                {
                    var embed = new DiscordEmbed
                    {
                        title = section.Title,
                        url = bundle.URL + "?dedupe=" + random.Next(),
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
                                url = bundle.URL + "?dedupe=" + random.Next(),
                                description = ""
                            };
                        }
                    }

                    // Add last embed
                    message.embeds.Add(embed);
                }

                if (!string.IsNullOrEmpty(webhook.Partner))
                {
                    AddPartnerLink(message, webhook.Partner);
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
    }
}
