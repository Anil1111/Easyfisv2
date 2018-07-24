using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.POSIntegration.Entities
{
    public class POSIntegrationMstSupplier
    {
        public String Article { get; set; }
        public String Address { get; set; }
        public String ContactNumber { get; set; }
        public String Term { get; set; }
        public String TaxNumber { get; set; }
    }
}