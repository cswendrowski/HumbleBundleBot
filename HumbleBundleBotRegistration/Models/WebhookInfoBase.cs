using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HumbleBundleBotRegistration.Models
{
    public enum BundleType { Games, Books, Mobile, Software, Mixed }

    public enum WebhookType { Discord, RawJson }

    public class WebhookInfoBase
    {
        public BundleType Type { get; set; }

        public string Webhook { get; set; }

        public WebhookType WebhookType { get; set; }

    }
}
