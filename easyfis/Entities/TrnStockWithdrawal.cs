using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class TrnStockWithdrawal
    {
        public Int32 Id { get; set; }
        public Int32 BranchId { get; set; }
        public String SWNumber { get; set; }
        public String SWDate { get; set; }
        public Int32 CustomerId { get; set; }
        public Int32 SIBranchId { get; set; }
        public Int32 SIId { get; set; }
        public String SIBranch { get; set; }
        public String SINumber { get; set; }
        public String Remarks { get; set; }
        public String DocumentReference { get; set; }
        public String ContactPerson { get; set; }
        public String ContactNumber { get; set; }
        public String Address { get; set; }
        public Int32 PreparedById { get; set; }
        public Int32 CheckedById { get; set; }
        public Int32 ApprovedById { get; set; }
        public Boolean IsLocked { get; set; }
        public Int32 CreatedById { get; set; }
        public String CreatedBy { get; set; }
        public String CreatedDateTime { get; set; }
        public Int32 UpdatedById { get; set; }
        public String UpdatedBy { get; set; }
        public String UpdatedDateTime { get; set; }
    }
}