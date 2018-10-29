using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class TrnCollection
    {
        public Int32 Id { get; set; }
        public Int32 BranchId { get; set; }
        public String ORNumber { get; set; }
        public String ORDate { get; set; }
        public Int32 CustomerId { get; set; }
        public String Customer { get; set; }
        public String Particulars { get; set; }
        public Decimal Amount { get; set; }
        public String ManualORNumber { get; set; }
        public Int32 PreparedById { get; set; }
        public Int32 CheckedById { get; set; }
        public Int32 ApprovedById { get; set; }
        public String Status { get; set; }
        public Boolean IsCancelled { get; set; }
        public Boolean IsLocked { get; set; }
        public Int32 CreatedById { get; set; }
        public String CreatedBy { get; set; }
        public String CreatedDateTime { get; set; }
        public Int32 UpdatedById { get; set; }
        public String UpdatedBy { get; set; }
        public String UpdatedDateTime { get; set; }
    }
}