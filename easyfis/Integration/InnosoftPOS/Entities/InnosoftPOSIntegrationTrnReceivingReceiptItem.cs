using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.InnosoftPOS.Entities
{
    public class InnosoftPOSIntegrationTrnReceivingReceiptItem
    {
        public Int32 RRId { get; set; }
        public String BranchCode { get; set; }
        public String Branch { get; set; }
        public String ManualItemCode { get; set; }
        public String ItemDescription { get; set; }
        public String Particulars { get; set; }
        public String Unit { get; set; }
        public Decimal Quantity { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Amount { get; set; }
    }
}