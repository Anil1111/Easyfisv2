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
        public String Supplier { get; set; }
        public String Term { get; set; }
        public String DocumentReference { get; set; }
        public String ManualRRNumber { get; set; }
        public String Remarks { get; set; }
        public String ReceivedBy { get; set; }
        public String PreparedBy { get; set; }
        public String CheckedBy { get; set; }
        public String ApprovedBy { get; set; }
        public Boolean IsLocked { get; set; }
        public String CreatedBy { get; set; }
        public String CreatedDateTime { get; set; }
        public String UpdatedBy { get; set; }
        public String UpdatedDateTime { get; set; }
        public List<POSIntegrationTrnReceivingReceiptItem> ListPOSIntegrationTrnReceivingReceiptItem { get; set; }
    }

    public class POSIntegrationTrnReceivingReceiptItem
    {
        public Int32 RRId { get; set; }
        public String ItemCode { get; set; }
        public String Item { get; set; }
        public String Particulars { get; set; }
        public String BranchCode { get; set; }
        public String Branch { get; set; }
        public String Unit { get; set; }
        public Decimal Quantity { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Amount { get; set; }
        public String BaseUnit { get; set; }
        public Decimal BaseQuantity { get; set; }
        public Decimal BaseCost { get; set; }
    }
}