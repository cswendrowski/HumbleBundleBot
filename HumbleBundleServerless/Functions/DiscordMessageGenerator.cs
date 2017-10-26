using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using HumbleBundleBot;
using HumbleBundleServerless.Models;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace HumbleBundleServerless
{
    public static class DiscordMessageGenerator
    {
        [FunctionName("DiscordMessageGenerator")]
        public static void Run(
            [QueueTrigger("bundlequeue")] BundleQueue myQueueItem,
            [Queue("messagequeue")] ICollector<DiscordMessage> messageQueue,
            TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem.Bundle.Name}");

            var bundle = myQueueItem.Bundle;

            QueueMessagesToAllWebhooks(messageQueue, new DiscordWebhookPayload()
            {
                content = bundle.URL
            });


            var message = new DiscordWebhookPayload
            {
                embeds = new List<DiscordEmbed>()
            };

            foreach (var section in bundle.Sections)
            {
                var embed = new DiscordEmbed
                {
                    title = section.Title,
                    url = bundle.URL,
                    description = ""
                };

                foreach (var game in section.Games)
                {
                    embed.description += game + "\n";
                }

                message.embeds.Add(embed);
            }

            QueueMessagesToAllWebhooks(messageQueue, message);
        }

        private static void QueueMessagesToAllWebhooks(ICollector<DiscordMessage> messageQueue, DiscordWebhookPayload message)
        {
            messageQueue.Add(new DiscordMessage
            {
                WebhookUrl = "https://discordapp.com/api/webhooks/372539156921974784/dhmYS4QhYhlSGUT3FJQR_miXDvnd5WsFZNauhHw3xlRqTKNyL61xTngB44kr0U8QpsKS",
                Payload = message
            });
        }
    }
}
