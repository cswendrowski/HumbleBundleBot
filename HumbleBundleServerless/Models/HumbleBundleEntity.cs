using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace HumbleBundleServerless.Models
{
    public class HumbleBundleEntity : TableEntity
    {
        public HumbleBundleEntity(BundleTypes bundleType, HumbleBundle bundle)
        {
            PartitionKey = bundleType.ToString();
            RowKey = bundle.Name;

            Bundle = JsonConvert.SerializeObject(bundle);
        }

        public HumbleBundleEntity() { }

        public string Bundle { get; set; }

        public HumbleBundle GetBundle()
        {
            return JsonConvert.DeserializeObject<HumbleBundle>(Bundle);
        }
    }
}
