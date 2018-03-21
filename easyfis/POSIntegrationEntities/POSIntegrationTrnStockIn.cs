using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.POSIntegrationEntities
{
    public class POSIntegrationTrnStockIn
    {
        public String BranchCode { get; set; }
        public String Branch { get; set; }
        public String INNumber { get; set; }
        public String INDate { get; set; }
        public List<POSIntegrationTrnStockInItem> ListPOSIntegrationTrnStockInItem { get; set; }
    }

    public class POSIntegrationTrnStockInItem
    {
        public Int32 INId { get; set; }
        public String ItemCode { get; set; }
        public String Item { get; set; }
        public String Unit { get; set; }
        public Decimal Quantity { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Amount { get; set; }
    }
}