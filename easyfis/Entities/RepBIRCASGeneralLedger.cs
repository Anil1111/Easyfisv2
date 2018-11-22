using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepBIRCASGeneralLedger
    {
        public Int32 Id { get; set; }
        public String Date { get; set; }
        public String ReferenceNumber { get; set; }
        public String AccountCode { get; set; }
        public String Account { get; set; }
        public Decimal DebitAmount { get; set; }
        public Decimal CreditAmount { get; set; }
    }
}