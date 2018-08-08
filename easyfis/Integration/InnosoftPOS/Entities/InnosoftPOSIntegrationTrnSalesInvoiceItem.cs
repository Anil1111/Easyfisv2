using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.InnosoftPOS.Entities
{
    public class InnosoftPOSIntegrationTrnSalesInvoiceItem
    {
        public Int32 SIId { get; set; }
        public String BranchCode { get; set; }
        public String Branch { get; set; }
        public String ManualItemCode { get; set; }
        public String ItemDescription { get; set; }
        public String Particulars { get; set; }
        public String Unit { get; set; }
        public Decimal Quantity { get; set; }
        public Decimal Price { get; set; }
        public String Discount { get; set; }
        public Decimal DiscountAmount { get; set; }
        public Decimal NetPrice { get; set; }
        public Decimal Amount { get; set; }
        public String VAT { get; set; }
        public Decimal VATAmount { get; set; }
        public String SalesItemTimeStamp { get; set; }
    }
}