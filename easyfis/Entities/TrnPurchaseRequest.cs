using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class TrnPurchaseRequest
    {
        public Int32 Id { get; set; }
        public Int32 BranchId { get; set; }
        public String PRNumber { get; set; }
        public String PRDate { get; set; }
        public Int32 SupplierId { get; set; }
        public String Supplier { get; set; }
        public Int32 TermId { get; set; }
        public String ManualPRNumber { get; set; }
        public String DateNeeded { get; set; }
        public String Remarks { get; set; }
        public Decimal Amount { get; set; }
        public Boolean IsClose { get; set; }
        public Int32 RequestedById { get; set; }
        public Int32 PreparedById { get; set; }
        public Int32 CheckedById { get; set; }
        public Int32 ApprovedById { get; set; }
        public String Status { get; set; }
        public Boolean IsPrinted { get; set; }
        public Boolean IsLocked { get; set; }
        public Int32 CreatedById { get; set; }
        public String CreatedBy { get; set; }
        public String CreatedDateTime { get; set; }
        public Int32 UpdatedById { get; set; }
        public String UpdatedBy { get; set; }
        public String UpdatedDateTime { get; set; }
    }
}