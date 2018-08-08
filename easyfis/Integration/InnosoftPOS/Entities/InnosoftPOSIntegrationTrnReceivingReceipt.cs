using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.InnosoftPOS.Entities
{
    public class InnosoftPOSIntegrationTrnReceivingReceipt
    {
        public String BranchCode { get; set; }
        public String Branch { get; set; }
        public String RRNumber { get; set; }
        public String RRDate { get; set; }
        public String ManualRRNumber { get; set; }
        public List<InnosoftPOSIntegrationTrnReceivingReceiptItem> ListReceivingReceiptItem { get; set; }
    }
}