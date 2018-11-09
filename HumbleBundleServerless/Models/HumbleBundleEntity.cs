using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace HumbleBundleServerless.Models
{
    public class HumbleBundleEntity : TableEntity
    {
        public HumbleBundleEntity(HumbleBundle bundle)
        {
            PartitionKey = bundle.Type.ToString();
            RowKey = bundle.Name.Replace("#", "");

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
