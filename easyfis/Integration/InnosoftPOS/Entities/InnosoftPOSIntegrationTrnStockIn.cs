using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.InnosoftPOS.Entities
{
    public class InnosoftPOSIntegrationTrnStockIn
    {
        public String BranchCode { get; set; }
        public String Branch { get; set; }
        public String INNumber { get; set; }
        public String INDate { get; set; }
        public String ManualINNumber { get; set; }
        public List<InnosoftPOSIntegrationTrnStockInItem> ListStockInItem { get; set; }
    }
}