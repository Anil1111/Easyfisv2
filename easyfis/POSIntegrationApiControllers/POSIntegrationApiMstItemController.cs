using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.POSIntegrationApiControllers
{
    public class POSIntegrationApiMstItemController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========================
        // GET Items - POS Integration
        // ===========================
        [HttpGet, Route("api/get/POSIntegration/item/{updateDateTime}")]
        public List<POSIntegrationEntities.POSIntegrationMstItem> GetItemPOSIntegration(String updateDateTime)
        {
            var items = from d in db.MstArticles
                        where d.ArticleTypeId == 1
                        && d.IsLocked == true
                        && d.UpdatedDateTime.Date == Convert.ToDateTime(updateDateTime)
                        select new POSIntegrationEntities.POSIntegrationMstItem
                        {
                            ManualArticleCode = d.ManualArticleCode,
                            Article = d.Article,
                            ArticleTypeId = d.ArticleTypeId,
                            Category = d.Category,
                            Unit = d.MstUnit.Unit,
                            Price = d.Price,
                            Cost = d.Cost,
                            IsInventory = d.IsInventory,
                            Particulars = d.Particulars,
                            UpdatedDateTime = d.UpdatedDateTime.ToShortDateString(),
                            ListPOSIntegrationMstItemPrice = db.MstArticlePrices.Select(p => new POSIntegrationEntities.POSIntegrationMstItemPrice
                            {
                                ArticleId = p.ArticleId,
                                PriceDescription = p.PriceDescription,
                                Price = p.Price
                            }).Where(p => p.ArticleId == d.Id).ToList(),
                        };

            return items.ToList();
        }
    }
}
