using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepHourlyTopSellingItemsReport
    {
        public String Branch { get; set; }
        public Int32 ItemId { get; set; }
        public String Item { get; set; }
        public String BaseUnit { get; set; }
        public Decimal BaseQuantity { get; set; }
        public Decimal BasePrice { get; set; }
        public Decimal Amount { get; set; }
        public String SalesItemTimeStamp { get; set; }
    }
}