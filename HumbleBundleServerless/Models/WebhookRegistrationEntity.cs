using Microsoft.WindowsAzure.Storage.Table;

namespace HumbleBundleServerless.Models
{
    public class WebhookRegistrationEntity : TableEntity
    {
        public WebhookRegistrationEntity(BundleTypes type, string webhook, bool shouldRecieveUpdates)
        {
            PartitionKey = type.ToString();
            RowKey = System.Guid.NewGuid().ToString();

            Webhook = webhook;
            ShouldRecieveUpdates = shouldRecieveUpdates;
            BundleType = (int) type;
        }

        public WebhookRegistrationEntity() { }

        public string Webhook { get; set; }

        public bool ShouldRecieveUpdates { get; set; } = false;

        public int BundleType { get; set;  }
    }
}
