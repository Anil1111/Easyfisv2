using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepCancelledSalesSummaryReport
    {
        public Int32 Id { get; set; }
        public String Branch { get; set; }
        public String SINumber { get; set; }
        public String SIDate { get; set; }
        public String Customer { get; set; }
        public String Remarks { get; set; }
        public String SoldBy { get; set; }
        public Decimal Amount { get; set; }
    }
}