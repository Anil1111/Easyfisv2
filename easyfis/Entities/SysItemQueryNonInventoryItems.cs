using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class SysItemQueryNonInventoryItems
    {
        public String ManualArticleCode { get; set; }
        public String Article { get; set; }
        public Decimal Price { get; set; }
    }
}