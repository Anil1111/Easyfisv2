using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepBIRCASSalesJournal
    {
        public String Date { get; set; }
        public String ReferenceNumber { get; set; }
        public String Customer { get; set; }
        public String CustomerTIN { get; set; }
        public String Address { get; set; }
        public String ManualReferenceNumber { get; set; }
        public Decimal TotalAmount { get; set; }
        public Decimal Discount { get; set; }
        public Decimal VAT { get; set; }
        public Decimal NetSales { get; set; }
    }
}