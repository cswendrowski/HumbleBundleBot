using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleServerless.Models
{
    public class BundleQueue
    {
        public BundleTypes Type { get; set; }

        public HumbleBundle Bundle { get; set; }

        public bool IsUpdate { get; set; } = false;
    }
}
