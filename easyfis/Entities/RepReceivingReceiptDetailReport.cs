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
        public String Quantity { get; set; }
        public String Unit { get; set; }
        public String Cost { get; set; }
        public String Amount { get; set; }
        public String VAT { get; set; }
        public String WTAX { get; set; }
    }
}