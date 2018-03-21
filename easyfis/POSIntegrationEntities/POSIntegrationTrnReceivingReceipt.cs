using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.POSIntegrationEntities
{
    public class POSIntegrationTrnReceivingReceipt
    {
        public String BranchCode { get; set; }
        public String Branch { get; set; }
        public String RRNumber { get; set; }
        public String RRDate { get; set; }
        public List<POSIntegrationTrnReceivingReceiptItem> ListPOSIntegrationTrnReceivingReceiptItem { get; set; }
    }

    public class POSIntegrationTrnReceivingReceiptItem
    {
        public Int32 RRId { get; set; }
        public String ItemCode { get; set; }
        public String Item { get; set; }
        public String BranchCode { get; set; }
        public String Unit { get; set; }
        public Decimal Quantity { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Amount { get; set; }
    }
}