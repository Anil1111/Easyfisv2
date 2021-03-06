﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepDisbursementBook
    {
        public String DocumentReference { get; set; }
        public String ManualDocumentCode { get; set; }
        public String AccountCode { get; set; }
        public String Account { get; set; }
        public String Article { get; set; }
        public String Particulars { get; set; }
        public String CheckNumber { get; set; }
        public String CheckDate { get; set; }
        public Decimal Amount { get; set; }
        public Decimal DebitAmount { get; set; }
        public Decimal CreditAmount { get; set; }
        public Decimal Balance { get; set; }
    }
}