using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.InnosoftPOS.Entities
{
    public class InnosoftPOSIntegrationTrnItemPriceItem
    {
        public Int32 IPId { get; set; }
        public String ManualItemCode { get; set; }
        public String ItemDescription { get; set; }
        public Decimal Price { get; set; }
        public Decimal TriggerQuantity { get; set; }
    }
}