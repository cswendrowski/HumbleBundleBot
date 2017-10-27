using System;
using System.Collections.Generic;

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
