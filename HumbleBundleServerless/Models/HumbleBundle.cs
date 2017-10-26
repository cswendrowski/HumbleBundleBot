using System;
using System.Collections.Generic;

namespace HumbleBundleServerless.Models
{
    public class HumbleBundle
    {
        public String Name { get; set; }
        public String URL { get; set; }
        public List<HumbleSection> Sections { get; set; } = new List<HumbleSection>();
    }

    public class HumbleSection
    {
        public String Title { get; set; }
        public List<String> Games { get; set; } = new List<String>();
    }
}
