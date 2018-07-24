using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Integration.POSIntegration.Entities
{
    public class POSIntegrationTrnArticlePrice
    {
        public String BranchCode { get; set; }
        public String IPNumber { get; set; }
        public String IPDate { get; set; }
        public String ItemCode { get; set; }
        public String ItemDescription { get; set; }
        public Decimal Price { get; set; }
        public Decimal TriggerQuantity { get; set; }
    }
}