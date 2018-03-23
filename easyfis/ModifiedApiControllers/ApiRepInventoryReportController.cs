using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepInventoryReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===================================
        // Dropdown List - Bank Group (Filter)
        // ===================================
        [Authorize, HttpGet, Route("api/inventoryReport/dropdown/list/stockCount/{branchId}")]
        public List<Entities.TrnStockCount> DropdownListStockCount(String branchId)
        {
            var stockCounts = from d in db.TrnStockCounts.OrderByDescending(d => d.Id)
                              where d.BranchId == Convert.ToInt32(branchId)
                              && d.IsLocked == true
                              select new Entities.TrnStockCount
                              {
                                  Id = d.Id,
                                  SCNumber = d.SCNumber,
                                  StockCountItem = d.TrnStockCountItems.Select(i => new Entities.TrnStockCountItem
                                  {
                                      ItemId = i.ItemId,
                                      Quantity = i.Quantity
                                  }).ToList(),
                              };

            return stockCounts.ToList();
        }

        // =====================
        // Inventory Report List
        // =====================
        [Authorize, HttpGet, Route("api/inventoryReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Models.MstArticleInventory> ListInventoryReport(String startDate, String endDate, String companyId, String branchId)
        {
            try
            {
                var unionInventories = (from d in db.TrnInventories
                                        where d.InventoryDate < Convert.ToDateTime(startDate)
                                        && d.MstArticleInventory.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                        && d.MstArticleInventory.BranchId == Convert.ToInt32(branchId)
                                        && d.MstArticleInventory.MstArticle.IsInventory == true
                                        select new Models.MstArticleInventory
                                        {
                                            Id = d.Id,
                                            Document = "Beginning Balance",
                                            BranchId = d.BranchId,
                                            Branch = d.MstBranch.Branch,
                                            ArticleId = d.MstArticleInventory.ArticleId,
                                            Article = d.MstArticleInventory.MstArticle.Article,
                                            ManualArticleCode = d.MstArticleInventory.MstArticle.ManualArticleCode,
                                            ManualArticleOldCode = d.MstArticleInventory.MstArticle.ManualArticleOldCode,
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
                                        }).Union(from d in db.TrnInventories
                                                 where d.InventoryDate >= Convert.ToDateTime(startDate)
                                                 && d.InventoryDate <= Convert.ToDateTime(endDate)
                                                 && d.MstArticleInventory.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                                 && d.MstArticleInventory.BranchId == Convert.ToInt32(branchId)
                                                 && d.MstArticleInventory.MstArticle.IsInventory == true
                                                 select new Models.MstArticleInventory
                                                 {
                                                     Id = d.Id,
                                                     Document = "Current",
                                                     BranchId = d.BranchId,
                                                     Branch = d.MstBranch.Branch,
                                                     ArticleId = d.MstArticleInventory.ArticleId,
                                                     Article = d.MstArticleInventory.MstArticle.Article,
                                                     ManualArticleCode = d.MstArticleInventory.MstArticle.ManualArticleCode,
                                                     ManualArticleOldCode = d.MstArticleInventory.MstArticle.ManualArticleOldCode,
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
                                                 });

                if (unionInventories.Any())
                {
                    var inventories = from d in unionInventories
                                      group d by new
                                      {
                                          d.BranchId,
                                          d.Branch,
                                          d.ArticleId,
                                          d.Article,
                                          d.ManualArticleCode,
                                          d.ManualArticleOldCode,
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
                                          ArticleId = g.Key.ArticleId,
                                          Article = g.Key.Article,
                                          ManualArticleCode = g.Key.ManualArticleCode,
                                          ManualArticleOldCode = g.Key.ManualArticleOldCode,
                                          InventoryCode = g.Key.InventoryCode,
                                          Cost = g.Key.Cost,
                                          UnitId = g.Key.UnitId,
                                          Unit = g.Key.Unit,
                                          BegQuantity = g.Sum(d => d.Document == "Current" ? 0 : d.BegQuantity),
                                          InQuantity = g.Sum(d => d.Document == "Beginning Balance" ? 0 : d.InQuantity),
                                          OutQuantity = g.Sum(d => d.Document == "Beginning Balance" ? 0 : d.OutQuantity),
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
