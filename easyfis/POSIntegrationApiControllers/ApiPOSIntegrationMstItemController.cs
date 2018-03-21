using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.POSIntegrationApiControllers
{
    public class ApiPOSIntegrationMstItemController : ApiController
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
                            Category = d.Category,
                            Unit = d.MstUnit.Unit,
                            Price = d.Price,
                            Cost = d.Cost,
                            IsInventory = d.IsInventory,
                            Particulars = d.Particulars,
                            OutputTax = d.MstTaxType.TaxType
                        };

            return items.ToList();
        }
    }
}
