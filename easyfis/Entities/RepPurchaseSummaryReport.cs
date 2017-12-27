using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepPurchaseSummaryReport
    {
        public Int32 POId { get; set; }
        public String Branch { get; set; }
        public String PONumber { get; set; }
        public String PODate { get; set; }
        public String Supplier { get; set; }
        public String Term { get; set; }
        public String ManualRequestNumber { get; set; }
        public String DateNeeded { get; set; }
        public Boolean IsClose { get; set; }
        public Decimal Amount { get; set; }
    }
}