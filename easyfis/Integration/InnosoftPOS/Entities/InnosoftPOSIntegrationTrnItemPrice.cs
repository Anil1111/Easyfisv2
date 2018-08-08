using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.InnosoftPOS.Entities
{
    public class InnosoftPOSIntegrationTrnItemPrice
    {
        public String BranchCode { get; set; }
        public String Branch { get; set; }
        public String IPNumber { get; set; }
        public String IPDate { get; set; }
        public String ManualIPNumber { get; set; }
        public List<InnosoftPOSIntegrationTrnItemPriceItem> ListItemPriceItem { get; set; }
    }
}