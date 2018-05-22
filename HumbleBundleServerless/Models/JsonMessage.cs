using System;
using System.Collections.Generic;
using HumbleBundleServerless.Models;

namespace HumbleBundleBot
{
    public class JsonMessage
    {
        public string WebhookUrl { get; set; }

        public BundleQueue Payload { get; set; }
    }
    
}
