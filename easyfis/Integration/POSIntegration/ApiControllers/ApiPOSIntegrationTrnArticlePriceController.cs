using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.POSIntegration.ApiControllers
{
    public class ApiPOSIntegrationTrnArticlePriceController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =================================
        // GET Iten Prices - POS Integration
        // =================================
        [HttpGet, Route("api/get/POSIntegration/itemPrice/{branchCode}/{IPDate}")]
        public List<Entities.POSIntegrationTrnArticlePrice> GetItemPricePOSIntegration(String branchCode, String IPDate)
        {
            var itemPrices = from d in db.TrnArticlePriceItems
                             where d.TrnArticlePrice.IsLocked == true
                             && d.TrnArticlePrice.MstBranch.BranchCode.Equals(branchCode)
                             && d.TrnArticlePrice.IPDate.Date == Convert.ToDateTime(IPDate)
                             select new Entities.POSIntegrationTrnArticlePrice
                             {
                                 BranchCode = d.TrnArticlePrice.MstBranch.BranchCode,
                                 IPNumber = d.TrnArticlePrice.IPNumber,
                                 IPDate = d.TrnArticlePrice.IPDate.ToShortDateString(),
                                 ItemCode = d.MstArticle.ManualArticleCode,
                                 ItemDescription = d.MstArticle.Article,
                                 Price = d.Price,
                                 TriggerQuantity = d.TriggerQuantity
                             };

            return itemPrices.ToList();
        }
    }
}
