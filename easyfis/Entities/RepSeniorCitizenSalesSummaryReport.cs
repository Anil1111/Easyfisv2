using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepSeniorCitizenSalesSummaryReport
    {
        public Int32 Id { get; set; }
        public String Branch { get; set; }
        public Int32 SIId { get; set; }
        public String SI { get; set; }
        public String SIDate { get; set; }
        public String Customer { get; set; }
        public String Item { get; set; }
        public String ItemInventory { get; set; }
        public Decimal Price { get; set; }
        public String Unit { get; set; }
        public Decimal Quantity { get; set; }
        public Decimal Amount { get; set; }
        public String Discount { get; set; }
        public Decimal DiscountRate { get; set; }
        public Decimal DiscountAmount { get; set; }
    }
}