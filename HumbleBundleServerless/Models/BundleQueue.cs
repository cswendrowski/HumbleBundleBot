using System.Collections.Generic;

namespace HumbleBundleServerless.Models
{
    public class BundleQueue
    {
        public HumbleBundle Bundle { get; set; }

        public bool IsUpdate { get; set; } = false;

        public List<HumbleItem> UpdatedItems { get; internal set; } = new List<HumbleItem>();
    }
}
