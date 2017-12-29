using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepReceivingReceiptSummaryReport
    {
        public Int32 RRId { get; set; }
        public String Branch { get; set; }
        public String RRNumber { get; set; }
        public String RRDate { get; set; }
        public String Supplier { get; set; }
        public String Term { get; set; }
        public String DocumentReference { get; set; }
        public Decimal Amount { get; set; }
        public Decimal WTaxAmount { get; set; }
        public Decimal RRAmount { get; set; }
    }
}