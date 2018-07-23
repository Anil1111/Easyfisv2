using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepConsignmentReport
    {
        public String ItemManualArticleOldCode { get; set; }
        public String ItemCode { get; set; }
        public String ItemDescription { get; set; }
        public String Unit { get; set; }
        public Decimal Quantity { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Amount { get; set; }
    }
}