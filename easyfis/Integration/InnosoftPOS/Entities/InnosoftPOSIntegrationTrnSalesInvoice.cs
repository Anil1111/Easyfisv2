using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.InnosoftPOS.Entities
{
    public class InnosoftPOSIntegrationTrnSalesInvoice
    {
        public String BranchCode { get; set; }
        public String Branch { get; set; }
        public String SINumber { get; set; }
        public String SIDate { get; set; }
        public String ManualSINumber { get; set; }
        public String DocumentReference { get; set; }
        public String ManualCustomerCode { get; set; }
        public String Term { get; set; }
        public String Remarks { get; set; }
        public Decimal Amount { get; set; }
        public String CreatedBy { get; set; }
        public List<InnosoftPOSIntegrationTrnSalesInvoiceItem> ListSalesInvoiceItem { get; set; }
    }
}