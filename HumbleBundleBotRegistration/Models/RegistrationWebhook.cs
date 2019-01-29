using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HumbleBundleBotRegistration.Models
{
    public class RegistrationWebhook : WebhookInfoBase
    {

        public bool SendUpdates { get; set; }

        public string Partner { get; set; }

    }
}
