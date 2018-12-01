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
        public String DocumentReference { get; set; }
        public String ManualReferenceNumber { get; set; }
        public String ItemCode { get; set; }
        public Decimal DiscountAmount { get; set; }
        public Decimal Amount { get; set; }
        public Decimal VATableSalesAmount { get; set; }
        public Decimal VATExemptSalesAmount { get; set; }
        public Decimal ZeroRatedSalesAmount { get; set; }
        public Decimal VATAmount { get; set; }
    }
}