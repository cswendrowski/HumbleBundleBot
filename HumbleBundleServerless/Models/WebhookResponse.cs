using HumbleBundleServerless.Models;

namespace HumbleBundleServerless.Models
{
    public class WebhookResponse
    {
        public WebhookResponse(WebhookRegistrationEntity created)
        {
            Id = created.RowKey;
            Type = created.PartitionKey;
        }

        public string Id { get; set; }

        public string Type { get; set; }
    }
}