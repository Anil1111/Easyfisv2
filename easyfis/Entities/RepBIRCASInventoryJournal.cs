using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepBIRCASInventoryJournal
    {
        public String Date { get; set; }
        public String ReferenceNumber { get; set; }
        public String AccountCode { get; set; }
        public String Account { get; set; }
        public String Item { get; set; }
        public Decimal DebitAmount { get; set; }
        public Decimal CreditAmount { get; set; }
    }
}