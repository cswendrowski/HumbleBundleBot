using HumbleBundleBot;
using NLog;
using RestSharp;
using System;
using System.Linq;
using System.Text;

namespace HumbleBundleService.Services
{
    public class CheckService
    {
        public static void CheckForNewBundles()
        {
            var humbleGameService = new HumbleService(new HumbleScraper(), "games.jsonl");
            var humbleBookService = new HumbleService(new HumbleBookScraper(), "books.jsonl");

            CheckBundles(humbleGameService, "webhooks/322500154621952000/RCbruxtp6vOGkNd0kHbR4QEI0chL7ubTs8VQCc7XthidFkoMkM1gk9s9gpfZd8Bv6Exr");
            CheckBundles(humbleBookService, "webhooks/322500229754519564/JN-WAsX-BdctgTwdY6Vu2ip3hZY4r3Xnbr7bJv0_Z1FRHfKh4spn7b3v8ysxm9wYA2xR");
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static void CheckBundles(HumbleService service, String webhook)
        {
            var newBundles = service.GetNewBundles();

            if (newBundles.Any())
            {
                foreach (var bundle in newBundles)
                {
                    logger.Info("Found new Bundle " + bundle.Name);
                    var builder = new StringBuilder();

                    builder.AppendLine(bundle.URL);
                    builder.AppendLine();

                    SendMessage(webhook, builder.ToString());

                    var embed = new DiscordEmbed()
                    {
                        url = bundle.URL
                    };

                    foreach (var section in bundle.Sections)
                    {
                        builder = new StringBuilder();

                        foreach (var game in section.Games)
                        {
                            builder.Append(game + "\n");
                        }

                        embed.fields.Add(new EmbedField
                        {
                            name = section.Title,
                            value = builder.ToString(),
                            inline = true
                        });
                    }

                    SendMessage(webhook, embed);
                }
            }
        }

        private static void SendMessage(String webhook, String message)
        {
            var toSend = new DiscordWebhookPayload
            {
                content = message
            };

            var client = new RestClient("https://discordapp.com/api");

            var request = new RestRequest(webhook, Method.POST);

            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(toSend);

            client.Execute(request);
        }

        private static void SendMessage(String webhook, DiscordEmbed embed)
        {
            var toSend = new DiscordWebhookPayload
            {
                embeds = { embed }
            };

            var client = new RestClient("https://discordapp.com/api");

            var request = new RestRequest(webhook, Method.POST);

            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(toSend);

            client.Execute(request);
        }
    }
}
