using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepSalesSummaryReport
    {
        public Int32 SIId { get; set; }
        public String Branch { get; set; }
        public String SINumber { get; set; }
        public String SIDate { get; set; }
        public String DocumentReference { get; set; }
        public String Customer { get; set; }
        public String Term { get; set; }
        public String Sales { get; set; }
        public String Time { get; set; }
        public Decimal Amount { get; set; }
    }
}