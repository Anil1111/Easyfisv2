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

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/inventoryReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownLisInventoryReportListCompany()
        {
            var companies = from d in db.MstCompanies.OrderBy(d => d.Company)
                            select new Entities.MstCompany
                            {
                                Id = d.Id,
                                Company = d.Company
                            };

            return companies.ToList();
        }

        // ===============================
        // Dropdown List - Branch (Filter)
        // ===============================
        [Authorize, HttpGet, Route("api/inventoryReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListInventoryReportListBranch(String companyId)
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           where d.CompanyId == Convert.ToInt32(companyId)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // =============================
        // Dropdown List - Item (Filter)
        // =============================
        [Authorize, HttpGet, Route("api/inventoryReport/dropdown/list/item")]
        public List<Entities.MstArticle> DropdownListInventoryReportListItem()
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

        // ===================================
        // Dropdown List - Item Group (Filter)
        // ===================================
        [Authorize, HttpGet, Route("api/inventoryReport/dropdown/list/itemGroup")]
        public List<Entities.MstArticleGroup> DropdownListInventoryReportListItemGroup()
        {
            var itemGroups = from d in db.MstArticleGroups.OrderBy(d => d.ArticleGroup)
                             where d.ArticleTypeId == 1
                             select new Entities.MstArticleGroup
                             {
                                 Id = d.Id,
                                 ArticleGroup = d.ArticleGroup,
                             };

            return itemGroups.ToList();
        }

        // =================================
        // Dropdown List - Supplier (Filter)
        // =================================
        [Authorize, HttpGet, Route("api/inventoryReport/dropdown/list/supplier")]
        public List<Entities.MstArticle> DropdownListInventoryReportListSupplier()
        {
            var suppliers = from d in db.MstArticles.OrderBy(d => d.Article)
                            where d.ArticleTypeId == 3
                            && d.IsLocked == true
                            select new Entities.MstArticle
                            {
                                Id = d.Id,
                                Article = d.Article
                            };

            return suppliers.ToList();
        }

        // ====================================
        // Dropdown List - Stock Count (Filter)
        // ====================================
        [Authorize, HttpGet, Route("api/inventoryReport/dropdown/list/stockCount/{branchId}")]
        public List<Entities.TrnStockCount> DropdownListInventoryReportListStockCount(String branchId)
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
    }
}
