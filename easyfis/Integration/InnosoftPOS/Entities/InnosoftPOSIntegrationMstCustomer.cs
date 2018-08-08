using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.InnosoftPOS.Entities
{
    public class InnosoftPOSIntegrationMstCustomer
    {
        public String ManualCustomerCode { get; set; }
        public String Customer { get; set; }
        public String Address { get; set; }
        public String ContactPerson { get; set; }
        public String ContactNumber { get; set; }
        public String Term { get; set; }
        public String TaxNumber { get; set; }
        public Decimal CreditLimit { get; set; }
    }
}