using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleBot
{
    public class HumbleMobileScraper : HumbleScraper
    {
        public override string GetBaseUrl()
        {
            return "https://www.humblebundle.com/mobile";
        }
    }
}
