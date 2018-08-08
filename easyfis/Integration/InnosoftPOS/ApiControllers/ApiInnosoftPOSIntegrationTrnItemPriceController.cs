using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.InnosoftPOS.ApiControllers
{
    public class ApiInnosoftPOSIntegrationTrnItemPriceController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==========================================
        // List Item Price (Innosoft POS Integration)
        // ==========================================
        [HttpGet, Route("api/innosoftPOSIntegration/itemPrice/list/{branchCode}/{IPDate}")]
        public List<Entities.InnosoftPOSIntegrationTrnItemPrice> ListInnosoftPOSIntegrationItemPrice(String branchCode, String IPDate)
        {
            var itemPrices = from d in db.TrnArticlePrices
                             where d.IsLocked == true
                             && d.MstBranch.BranchCode.Equals(branchCode)
                             && d.IPDate.Date == Convert.ToDateTime(IPDate)
                             select new Entities.InnosoftPOSIntegrationTrnItemPrice
                             {
                                 BranchCode = d.MstBranch.BranchCode,
                                 Branch = d.MstBranch.Branch,
                                 IPNumber = d.IPNumber,
                                 IPDate = d.IPDate.ToShortDateString(),
                                 ManualIPNumber = d.ManualIPNumber,
                                 ListItemPriceItem = db.TrnArticlePriceItems.Select(i => new Entities.InnosoftPOSIntegrationTrnItemPriceItem
                                 {
                                     IPId = i.ArticlePriceId,
                                     ManualItemCode = i.MstArticle.ManualArticleCode,
                                     ItemDescription = i.MstArticle.Article,
                                     Price = i.Price,
                                     TriggerQuantity = i.TriggerQuantity
                                 }).Where(i => i.IPId == d.Id).ToList()
                             };

            return itemPrices.ToList();
        }
    }
}