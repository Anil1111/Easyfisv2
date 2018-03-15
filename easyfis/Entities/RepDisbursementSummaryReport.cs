using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepDisbursementSummaryReport
    {
        public Int32 CVId { get; set; }
        public String Branch { get; set; }
        public String CVNumber { get; set; }
        public String CVDate { get; set; }
        public String ManualCVNumber { get; set; }
        public String Payee { get; set; }
        public String Particulars { get; set; }
        public String Bank { get; set; }
        public String CheckNumber { get; set; }
        public String CheckDate { get; set; }
        public Decimal Amount { get; set; }
    }
}