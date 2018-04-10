using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.CSVIntegratorEntities
{
    public class CSVIntegratorTrnStockIn
    {
        public String BranchCode { get; set; }
        public String INDate { get; set; }
        public String Particulars { get; set; }
        public String ManualINNumber { get; set; }
        public Boolean IsProduced { get; set; }
    }
    
    public class CSVIntegratorTrnStockInItem
    {
        public String BranchCode { get; set; }
        public String ManualINNumber { get; set; }
        public String ItemCode { get; set; }
        public String Particulars { get; set; }
        public String Unit { get; set; }
        public Decimal Quantity { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Amount { get; set; }
    }
}