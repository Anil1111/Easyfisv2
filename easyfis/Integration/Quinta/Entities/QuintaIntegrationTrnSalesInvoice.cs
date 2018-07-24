using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.Quinta.Entities
{
    public class ReturnedDocument
    {
        public String SINumber { get; set; }
        public String ManualSINumber { get; set; }
    }

    public class RootObject
    {
        public List<QuintaIntegrationTrnSalesInvoice> TRN { get; set; }
        public String DefaultTerm { get; set; }
        public String DefaultVatOutput { get; set; }
        public String DefaultVatInput { get; set; }
        public String DefaultWTax { get; set; }
        public String DefaultDiscount { get; set; }
    }

    public class QuintaIntegrationTrnSalesInvoice
    {
        public String FTN { get; set; }     // Folio Transaction #
        public String TDT { get; set; }     // Transaction Date
        public String SAI { get; set; }     // Item Code
        public String SAM { get; set; }     // Item Description
        public String ACI { get; set; }     // Customer Code
        public String ACC { get; set; }     // Customer Name
        public Decimal NAM { get; set; }    // Net Amount
    }
}