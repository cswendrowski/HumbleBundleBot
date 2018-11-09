using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HumbleBundleBot;
using HumbleBundleServerless.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace HumbleBundleServerless.Functions
{
    public static class FireDeveloperMessage
    {
        [FunctionName("FireDeveloperMessage")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestMessage req,
            [Queue("discordmessagequeue")] ICollector<DiscordMessage> messageQueue,
            [Table("webhookRegistration")] IQueryable<WebhookRegistrationEntity> existingWebhooks,
            TraceWriter log)
        {
            var message = new DiscordWebhookPayload
            {
                embeds = new List<DiscordEmbed>()
            };

            message.embeds.Add(new DiscordEmbed()
            {
                title = "Developer Message",
                description = await req.Content.ReadAsStringAsync(),
                author = new AuthorField()
                {
                    name = "HumbleBundleBot Developer",
                    url = "https://github.com/cswendrowski/HumbleBundleBot"
                }
            });

            foreach (var webhook in GetAllWebhooksForDeveloperMessage(existingWebhooks))
            {
                messageQueue.Add(new DiscordMessage
                {
                    WebhookUrl = webhook,
                    Payload = message
                });
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }

        private static List<string> GetAllWebhooksForDeveloperMessage(IQueryable<WebhookRegistrationEntity> existingWebhooks)
        {
            var webhooksForType = existingWebhooks.Where(x => x.PartitionKey == BundleTypes.DEVELOPER_MESSAGES.ToString()).ToList();

            var discordHooks = webhooksForType.Where(x => x.WebhookType == (int)WebhookType.Discord);

            return discordHooks.ToList().Select(x => x.GetDecryptedWebhook()).ToList();
        }
    }
}
