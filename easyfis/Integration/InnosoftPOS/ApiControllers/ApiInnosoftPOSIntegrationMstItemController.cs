using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.InnosoftPOS.ApiControllers
{
    public class ApiInnosoftPOSIntegrationMstItemController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ====================================
        // List Item (Innosoft POS Integration)
        // ====================================
        [HttpGet, Route("api/innosoftPOSIntegration/item/list/{updatedDateTime}")]
        public List<Entities.InnosoftPOSIntegrationMstItem> ListInnosoftPOSIntegrationItem(String updatedDateTime)
        {
            var items = from d in db.MstArticles
                        where d.ArticleTypeId == 1
                        && d.IsLocked == true
                        && d.UpdatedDateTime.Date == Convert.ToDateTime(updatedDateTime)
                        select new Entities.InnosoftPOSIntegrationMstItem
                        {
                            ManualItemCode = d.ManualArticleCode,
                            ItemDescription = d.Article,
                            Category = d.Category,
                            Unit = d.MstUnit.Unit,
                            Price = d.Price,
                            Cost = d.Cost,
                            IsInventory = d.IsInventory,
                            Particulars = d.Particulars,
                            OutputTax = d.MstTaxType.TaxType,
                            ListItemPrice = db.MstArticlePrices.Select(i => new Entities.InnosoftPOSIntegrationMstItemPrice
                            {
                                ItemId = i.ArticleId,
                                PriceDescription = i.PriceDescription,
                                Price = i.Price,
                                Remarks = i.Remarks
                            }).Where(i => i.ItemId == d.Id).ToList()
                        };

            return items.ToList();
        }
    }
}