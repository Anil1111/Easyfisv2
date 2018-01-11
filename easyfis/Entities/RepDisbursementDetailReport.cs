using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepDisbursementDetailReport
    {
        public Int32 CVId { get; set; }
        public String CVBranch { get; set; }
        public String CVNumber { get; set; }
        public String CVDate { get; set; }
        public String Payee { get; set; }
        public String Branch { get; set; }
        public String Account { get; set; }
        public String Article { get; set; }
        public String RRNumber { get; set; }
        public Decimal Amount { get; set; }
    }
}