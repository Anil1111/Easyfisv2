using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.InnosoftPOS.Entities
{
    public class InnosoftPOSIntegrationMstItemPrice
    {
        public Int32 ItemId { get; set; }
        public String PriceDescription { get; set; }
        public Decimal Price { get; set; }
        public String Remarks { get; set; }
    }
}