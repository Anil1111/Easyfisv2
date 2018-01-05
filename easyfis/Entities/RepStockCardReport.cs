using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepStockCardReport
    {
        public String Document { get; set; }
        public String InventoryDate { get; set; }
        public String BranchCode { get; set; }
        public String Branch { get; set; }
        public Decimal InQuantity { get; set; }
        public Decimal OutQuantity { get; set; }
        public Decimal BalanceQuantity { get; set; }
        public Decimal RunningQuantity { get; set; }
        public String Unit { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Amount { get; set; }
        public Int32? RRId { get; set; }
        public String RRNumber { get; set; }
        public Int32? SIId { get; set; }
        public String SINumber { get; set; }
        public Int32? INId { get; set; }
        public String INNumber { get; set; }
        public Int32? OTId { get; set; }
        public String OTNumber { get; set; }
        public Int32? STId { get; set; }
        public String STNumber { get; set; }
        public String ManualNumber { get; set; }
    }
}