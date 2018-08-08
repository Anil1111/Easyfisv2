using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.InnosoftPOS.Entities
{
    public class InnosoftPOSIntegrationTrnStockTransfer
    {
        public String ToBranchCode { get; set; }
        public String ToBranch { get; set; }
        public String STNumber { get; set; }
        public String STDate { get; set; }
        public String ManualSTNumber { get; set; }
        public List<InnosoftPOSIntegrationTrnStockTransferItem> ListStockTransferItem { get; set; }
    }
}