using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepPurchaseDetailReport
    {
        public Int32 POId { get; set; }
        public String Branch { get; set; }
        public String PONumber { get; set; }
        public String PODate { get; set; }
        public String Supplier { get; set; }
        public Decimal Quantity { get; set; }
        public String Unit { get; set; }
        public String Item { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Amount { get; set; }
    }
}