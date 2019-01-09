using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HumbleBundleServerless.Models
{
    [DebuggerDisplay("{Name}")]
    public class HumbleBundle
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public BundleTypes Type { get; set; }
        public List<HumbleSection> Sections { get; set; } = new List<HumbleSection>();

        public List<HumbleItem> Items
        {
            get
            {
                return Sections.SelectMany(x => x.Items).ToList();
            }
        }
    }

    public class HumbleSection
    {
        public string Title { get; set; }
        public List<HumbleItem> Items { get; set; } = new List<HumbleItem>();
    }

    public class HumbleItem
    {
        public string Name { get; set; }
    }
}
