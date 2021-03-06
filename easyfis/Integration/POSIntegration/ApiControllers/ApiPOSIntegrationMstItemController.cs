﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.POSIntegration.ApiControllers
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
        public List<Entities.POSIntegrationMstItem> GetItemPOSIntegration(String updateDateTime)
        {
            var items = from d in db.MstArticles
                        where d.ArticleTypeId == 1
                        && d.IsLocked == true
                        && d.UpdatedDateTime.Date == Convert.ToDateTime(updateDateTime)
                        select new Entities.POSIntegrationMstItem
                        {
                            ManualArticleCode = d.ManualArticleCode,
                            Article = d.Article,
                            Category = d.Category,
                            Unit = d.MstUnit.Unit,
                            Price = d.Price,
                            Cost = d.Cost,
                            IsInventory = d.IsInventory,
                            Particulars = d.Particulars,
                            OutputTax = d.MstTaxType.TaxType,
                            ListItemPrice = db.MstArticlePrices.Select(i => new Entities.POSIntegrationMstItemPrice
                            {
                                ArticleId = i.ArticleId,
                                PriceDescription = i.PriceDescription,
                                Price = i.Price,
                                Remarks = i.Remarks
                            }).Where(i => i.ArticleId == d.Id).ToList(),
                        };

            return items.ToList();
        }
    }
}
