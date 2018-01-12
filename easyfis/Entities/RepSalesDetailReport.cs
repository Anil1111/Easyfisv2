using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepSalesDetailReport
    {
        public Int32 SIId { get; set; }
        public String Branch { get; set; }
        public String SINumber { get; set; }
        public String SIDate { get; set; }
        public String Customer { get; set; }
        public String Sales { get; set; }
        public String Item { get; set; }
        public String ItemCategory { get; set; }
        public Decimal Quantity { get; set; }
        public String Unit { get; set; }
        public Decimal Price { get; set; }
        public Decimal DiscountAmount { get; set; }
        public Decimal NetPrice { get; set; }
        public Decimal Amount { get; set; }
        public Decimal VATAmount { get; set; }
    }
}