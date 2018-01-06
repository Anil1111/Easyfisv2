using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepReceivingReceiptDetailReport
    {
        public Int32 RRId { get; set; }
        public String Branch { get; set; }
        public String RRNumber { get; set; }
        public String RRDate { get; set; }
        public String Supplier { get; set; }
        public String PONumber { get; set; }
        public String ItemCode { get; set; }
        public String ItemDescription { get; set; }
        public Decimal Quantity { get; set; }
        public String Unit { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Amount { get; set; }
        public Decimal VAT { get; set; }
        public Decimal WTAX { get; set; }
    }
}