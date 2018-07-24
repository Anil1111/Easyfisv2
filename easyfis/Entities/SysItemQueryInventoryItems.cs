using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class SysItemQueryInventoryItems
    {
        public String InventoryCode { get; set; }
        public String ManualArticleCode { get; set; }
        public String Article { get; set; }
        public Decimal Price { get; set; }
        public Decimal Quantity { get; set; }
        public String Unit { get; set; }
    }
}