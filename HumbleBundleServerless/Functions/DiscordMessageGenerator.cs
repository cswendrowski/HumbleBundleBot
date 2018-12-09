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

                foreach (var item in section.Items.Take(25))
                {
                    embed.description += GetItemName(item, queuedBundle.UpdatedItems);
                }

                message.embeds.Add(embed);

                if (section.Items.Count > 25)
                {
                    var embedContinued = new DiscordEmbed
                    {
                        title = section.Title,
                        url = bundle.URL,
                        description = ""
                    };

                    foreach (var item in section.Items.Skip(25).Take(25))
                    {
                        embedContinued.description += GetItemName(item, queuedBundle.UpdatedItems);
                    }

                    message.embeds.Add(embedContinued);
                }
            }

            log.Info("Created message " + JsonConvert.SerializeObject(message));

            foreach (var webhook in webhooks)
            {
                messageQueue.Add(new DiscordMessage
                {
                    WebhookUrl = webhook,
                    Payload = message
                });
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

        private static List<String> GetAllWebhooksForBundleType(IQueryable<WebhookRegistrationEntity> existingWebhooks, BundleTypes type, bool isUpdate)
        {
            var webhooksForType = existingWebhooks.Where(x => x.PartitionKey == type.ToString()).ToList();

            var discordHooks = webhooksForType.Where(x => x.WebhookType == (int) WebhookType.Discord);

            if (isUpdate)
            {
                discordHooks = discordHooks.Where(x => x.ShouldRecieveUpdates);
            }

            return discordHooks.ToList().Select(x => x.GetDecryptedWebhook()).ToList();
        }
    }
}
