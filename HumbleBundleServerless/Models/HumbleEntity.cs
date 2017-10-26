using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleServerless.Models
{
    public class HumbleEntity : TableEntity
    {
        public HumbleEntity(string keyName)
        {
            PartitionKey = keyName;
            RowKey = keyName;
        }

        public HumbleEntity() { }

        public List<string> BundleList
        {
            get
            {
                if (string.IsNullOrEmpty(Bundles))
                {
                    return new List<string>();
                }
                return Bundles.Split(',').ToList();
            }
        }

        public string Bundles { get; set; }

    }
}
