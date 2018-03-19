using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.POSIntegrationEntities
{
    public class POSIntegrationTrnArticlePrice
    {
        public String BranchCode { get; set; }
        public String IPNumber { get; set; }
        public String IPDate { get; set; }
        public String Particulars { get; set; }
        public String ManualIPNumber { get; set; }
        public Int32 ArticlePriceId { get; set; }
        public String ItemCode { get; set; }
        public String ItemDescription { get; set; }
        public Decimal Price { get; set; }
        public Decimal TriggerQuantity { get; set; }
    }
}