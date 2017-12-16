using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepAccountLedger
    {
        public String JournalDate { get; set; }
        public String DocumentReference { get; set; }
        public String Article { get; set; }
        public String Particulars { get; set; }
        public Decimal DebitAmount { get; set; }
        public Decimal CreditAmount { get; set; }
        public Decimal Balance { get; set; }
        public Int32? ORId { get; set; }
        public Int32? CVId { get; set; }
        public Int32? JVId { get; set; }
        public Int32? RRId { get; set; }
        public Int32? SIId { get; set; }
        public Int32? INId { get; set; }
        public Int32? OTId { get; set; }
        public Int32? STId { get; set; }
    }
}