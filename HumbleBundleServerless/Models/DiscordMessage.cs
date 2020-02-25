using HumbleBundleServerless.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HumbleBundleBot
{
    public class DiscordMessage
    {
        public String WebhookUrl { get; set; }

        public DiscordWebhookPayload Payload { get; set; }
    }

    public class DiscordWebhookPayload
    {
        public String content { get; set; } = "";

        public List<DiscordEmbed> embeds { get; set; } = new List<DiscordEmbed>();

        public DiscordWebhookPayload() { }

        public DiscordWebhookPayload(BundleQueue queuedBundle, HumbleBundle bundle)
        {
            content = "New Bundle: " + bundle.Name;

            if (queuedBundle.IsUpdate)
            {
                content = "Bundle Updated: " + bundle.Name;
            }

            embeds.Add(new DiscordEmbed()
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
                        embeds.Add(embed);
                        embed = new DiscordEmbed
                        {
                            title = section.Title + " (Continued)",
                            url = bundle.URL + "?dedupe=" + random.Next(),
                            description = ""
                        };
                    }
                }

                // Add last embed
                embeds.Add(embed);
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

        public void AddPartnerLink(string partner)
        {
            foreach (var embed in embeds)
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
    }

    public class DiscordEmbed
    {
        public String title { get; set; }

        public String description { get; set; }

        public String url { get; set; }

        public DateTime timestamp { get; set; } = DateTime.Now;

        public int color { get; set; }

        public AuthorField author { get; set; } = new AuthorField();

        public ImageField image { get; set; } = new ImageField();

        public List<EmbedField> fields { get; set; } = new List<EmbedField>();
    }

    public class EmbedField
    {
        public String name { get; set; }

        public String value { get; set; }

        public bool inline { get; set; } = false;
    }

    public class AuthorField
    {
        public String name { get; set; }

        public String url { get; set; }
    }

    public class ImageField
    {
        public String url { get; set; }
    }
}
