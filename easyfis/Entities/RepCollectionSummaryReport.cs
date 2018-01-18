using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepCollectionSummaryReport
    {
        public Int32 ORId { get; set; }
        public String Branch { get; set; }
        public String ORNumber { get; set; }
        public String ORDate { get; set; }
        public String ManualORNumber { get; set; }
        public String Customer { get; set; }
        public Decimal Amount { get; set; }
    }
}