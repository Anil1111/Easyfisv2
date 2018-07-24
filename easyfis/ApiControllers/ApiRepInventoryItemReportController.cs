using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ModifiedApiControllers
{
    public class ApiRepInventoryItemReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        [HttpGet, Route("api/inventoryReportItem/dropdown/item")]
        public List<Entities.MstArticle> DropdownListInventoryReportItem()
        {
            var items = from d in db.MstArticles.OrderBy(d => d.Article)
                        where d.ArticleTypeId == 1
                        && d.IsLocked == true
                        select new Entities.MstArticle
                        {
                            Id = d.Id,
                            ManualArticleCode = d.ManualArticleCode,
                            Article = d.Article
                        };

            return items.ToList();
        }

        [HttpGet, Route("api/inventoryReportItem/dropdown/company")]
        public List<Entities.MstCompany> DropdownListInventoryReportItemCompany()
        {
            var companies = from d in db.MstCompanies.OrderBy(d => d.Company)
                            select new Entities.MstCompany
                            {
                                Id = d.Id,
                                Company = d.Company
                            };

            return companies.ToList();
        }

        [HttpGet, Route("api/inventoryReportItem/list/{itemId}/{companyId}")]
        public List<Models.MstArticleInventory> ListInventoryReportItem(String itemId, String companyId)
        {
            try
            {
                var unionInventories = from d in db.TrnInventories
                                       where d.MstArticleInventory.ArticleId == Convert.ToInt32(itemId)
                                       && d.MstArticleInventory.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                       && d.MstArticleInventory.MstArticle.IsInventory == true
                                       select new Models.MstArticleInventory
                                       {
                                           Id = d.Id,
                                           BranchId = d.BranchId,
                                           Branch = d.MstBranch.Branch,
                                           InventoryCode = d.MstArticleInventory.InventoryCode,
                                           Quantity = d.MstArticleInventory.Quantity,
                                           Cost = d.MstArticleInventory.Cost,
                                           Amount = d.MstArticleInventory.Amount,
                                           UnitId = d.MstArticleInventory.MstArticle.MstUnit.Id,
                                           Unit = d.MstArticleInventory.MstArticle.MstUnit.Unit,
                                           BegQuantity = d.Quantity,
                                           InQuantity = d.QuantityIn,
                                           OutQuantity = d.QuantityOut,
                                           EndQuantity = d.Quantity,
                                           Category = d.MstArticle.Category,
                                           Price = d.MstArticle.Price
                                       };

                if (unionInventories.Any())
                {
                    var inventories = from d in unionInventories
                                      group d by new
                                      {
                                          d.BranchId,
                                          d.Branch,
                                          d.InventoryCode,
                                          d.Cost,
                                          d.UnitId,
                                          d.Unit,
                                          d.Category,
                                          d.Price
                                      } into g
                                      select new Models.MstArticleInventory
                                      {
                                          BranchId = g.Key.BranchId,
                                          Branch = g.Key.Branch,
                                          InventoryCode = g.Key.InventoryCode,
                                          Cost = g.Key.Cost,
                                          UnitId = g.Key.UnitId,
                                          Unit = g.Key.Unit,
                                          BegQuantity = g.Sum(d => d.BegQuantity),
                                          InQuantity = g.Sum(d => d.InQuantity),
                                          OutQuantity = g.Sum(d => d.OutQuantity),
                                          EndQuantity = g.Sum(d => d.EndQuantity),
                                          Amount = g.Sum(d => d.Quantity * d.Cost),
                                          Category = g.Key.Category,
                                          Price = g.Key.Price
                                      };

                    return inventories.ToList();
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
