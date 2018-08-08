using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.InnosoftPOS.Entities
{
    public class InnosoftPOSIntegrationMstItem
    {
        public String ManualItemCode { get; set; }
        public String ItemDescription { get; set; }
        public String Category { get; set; }
        public String Unit { get; set; }
        public Decimal Price { get; set; }
        public Decimal? Cost { get; set; }
        public Boolean IsInventory { get; set; }
        public String Particulars { get; set; }
        public String OutputTax { get; set; }
        public List<InnosoftPOSIntegrationMstItemPrice> ListItemPrice { get; set; }
    }
}