using System;
using System.Collections.Generic;
using System.Linq;

namespace HumbleBundleServerless.Models
{
    public class HumbleBundle
    {
        public String Name { get; set; }
        public String URL { get; set; }
        public String Description { get; set; }
        public String ImageUrl { get; set; }
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
        public String Title { get; set; }
        public List<HumbleItem> Items { get; set; } = new List<HumbleItem>();
    }

    public class HumbleItem
    {
        public String Name { get; set; }
    }
}
