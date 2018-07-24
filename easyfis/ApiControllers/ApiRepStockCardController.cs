using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepStockCardController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ======================
        // Stock Card Report List
        // ======================
        [Authorize, HttpGet, Route("api/stockCard/list/{startDate}/{endDate}/{companyId}/{branchId}/{itemId}")]
        public List<Entities.RepStockCardReport> ListStockCard(String startDate, String endDate, String companyId, String branchId, String itemId)
        {
            var stockCardBeginningBalance = from d in db.TrnInventories
                                            where d.InventoryDate < Convert.ToDateTime(startDate)
                                            && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                            && d.BranchId == Convert.ToInt32(branchId)
                                            && d.MstArticle.IsInventory == true
                                            && d.ArticleId == Convert.ToInt32(itemId)
                                            select new
                                            {
                                                Document = "Beginning Balance",
                                                Branch = d.MstBranch.Branch,
                                                InventoryDate = Convert.ToDateTime(startDate),
                                                Quantity = d.Quantity,
                                                InQuantity = d.QuantityIn,
                                                OutQuantity = d.QuantityOut,
                                                BalanceQuantity = d.Quantity,
                                                Unit = d.MstArticle.MstUnit.Unit,
                                                Amount = d.Amount
                                            };

            var groupedStockCardBeginningBalance = from d in stockCardBeginningBalance
                                                   group d by new
                                                   {
                                                       Document = d.Document,
                                                       Branch = d.Branch,
                                                       InventoryDate = d.InventoryDate,
                                                       Unit = d.Unit
                                                   } into g
                                                   select new
                                                   {
                                                       Document = g.Key.Document,
                                                       Branch = g.Key.Branch,
                                                       InventoryDate = g.Key.InventoryDate,
                                                       InQuantity = g.Sum(d => d.InQuantity),
                                                       OutQuantity = g.Sum(d => d.OutQuantity),
                                                       BalanceQuantity = g.Sum(d => d.BalanceQuantity),
                                                       Unit = g.Key.Unit,
                                                       //Cost = g.Sum(d => d.InQuantity) > 0 ? g.Sum(d => d.Amount) / g.Sum(d => d.InQuantity) : 0,
                                                       //Amount = g.Sum(d => d.InQuantity) > 0 ? g.Sum(d => d.Amount) : 0,
                                                       //Amount = g.Sum(d => d.BalanceQuantity) >= 0 ? g.Sum(d => d.Amount) : 0
                                                       Cost = g.Sum(d => d.BalanceQuantity) >= 0 ? g.Sum(d => d.Amount) / g.Sum(d => d.BalanceQuantity) : 0,
                                                       Amount = g.Sum(d => d.Amount)
                                                   };

            var stockCardCurrentBalance = from d in db.TrnInventories
                                          where d.InventoryDate >= Convert.ToDateTime(startDate)
                                          && d.InventoryDate <= Convert.ToDateTime(endDate)
                                          && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                          && d.BranchId == Convert.ToInt32(branchId)
                                          && d.MstArticle.IsInventory == true
                                          && d.ArticleId == Convert.ToInt32(itemId)
                                          select new
                                          {
                                              Document = "Current",
                                              BranchCode = d.MstBranch.BranchCode,
                                              Branch = d.MstBranch.Branch,
                                              InventoryDate = d.InventoryDate,
                                              Quantity = d.Quantity,
                                              InQuantity = d.QuantityIn,
                                              OutQuantity = d.QuantityOut,
                                              BalanceQuantity = d.Quantity,
                                              Unit = d.MstArticle.MstUnit.Unit,
                                              Amount = d.Amount,
                                              RRId = d.RRId,
                                              RRNumber = d.TrnReceivingReceipt.RRNumber,
                                              SIId = d.SIId,
                                              SINumber = d.TrnSalesInvoice.SINumber,
                                              INId = d.INId,
                                              INNumber = d.TrnStockIn.INNumber,
                                              OTId = d.OTId,
                                              OTNumber = d.TrnStockOut.OTNumber,
                                              STId = d.STId,
                                              STNumber = d.TrnStockTransfer.STNumber,
                                              ManualNo = d.RRId != null ? d.TrnReceivingReceipt.ManualRRNumber : d.SIId != null ? d.TrnSalesInvoice.ManualSINumber : d.INId != null ? d.TrnStockIn.ManualINNumber : d.OTId != null ? d.TrnStockOut.ManualOTNumber : d.STId != null ? d.TrnStockTransfer.ManualSTNumber : " "
                                          };

            List<Entities.RepStockCardReport> listStockCardInventory = new List<Entities.RepStockCardReport>();

            Decimal beginningQuantity = 0;

            if (groupedStockCardBeginningBalance.Any())
            {
                var begBalance = groupedStockCardBeginningBalance.FirstOrDefault();

                beginningQuantity = begBalance.InQuantity > 0 ? begBalance.BalanceQuantity : 0;

                listStockCardInventory.Add(new Entities.RepStockCardReport()
                {
                    Document = begBalance.Document,
                    BranchCode = " ",
                    Branch = begBalance.Branch,
                    InventoryDate = begBalance.InventoryDate.ToShortDateString(),
                    InQuantity = begBalance.InQuantity,
                    OutQuantity = begBalance.OutQuantity,
                    BalanceQuantity = begBalance.BalanceQuantity,
                    RunningQuantity = begBalance.BalanceQuantity,
                    Unit = begBalance.Unit,
                    Cost = begBalance.Cost,
                    Amount = begBalance.Amount,
                    RRId = null,
                    RRNumber = "Beginning Balance",
                    SIId = null,
                    SINumber = "Beginning Balance",
                    INId = null,
                    INNumber = "Beginning Balance",
                    OTId = null,
                    OTNumber = "Beginning Balance",
                    STId = null,
                    STNumber = "Beginning Balance",
                    ManualNumber = " "
                });

                beginningQuantity = begBalance.BalanceQuantity;
            }

            Decimal runningQuantity = 0, cost = 0, amount = 0;

            if (stockCardCurrentBalance.Any())
            {
                foreach (var curBalance in stockCardCurrentBalance)
                {
                    runningQuantity = (beginningQuantity + curBalance.InQuantity) - curBalance.OutQuantity;

                    cost = curBalance.Quantity != 0 ? curBalance.Amount / curBalance.Quantity : 0;
                    //amount = curBalance.InQuantity > 0 ? curBalance.Amount : 0;
                    amount = curBalance.Amount;

                    listStockCardInventory.Add(new Entities.RepStockCardReport()
                    {
                        Document = curBalance.Document,
                        BranchCode = curBalance.BranchCode,
                        Branch = curBalance.Branch,
                        InventoryDate = curBalance.InventoryDate.ToShortDateString(),
                        InQuantity = curBalance.InQuantity,
                        OutQuantity = curBalance.OutQuantity,
                        BalanceQuantity = curBalance.BalanceQuantity,
                        RunningQuantity = runningQuantity,
                        Unit = curBalance.Unit,
                        Cost = cost,
                        Amount = amount,
                        RRId = curBalance.RRId,
                        RRNumber = curBalance.RRNumber,
                        SIId = curBalance.SIId,
                        SINumber = curBalance.SINumber,
                        INId = curBalance.INId,
                        INNumber = curBalance.INNumber,
                        OTId = curBalance.OTId,
                        OTNumber = curBalance.OTNumber,
                        STId = curBalance.STId,
                        STNumber = curBalance.STNumber,
                        ManualNumber = curBalance.ManualNo
                    });

                    beginningQuantity = runningQuantity;
                }
            }

            if (listStockCardInventory.Any())
            {
                return listStockCardInventory.ToList();
            }
            else
            {
                return null;
            }
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/stockCard/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListStockCardListCompany()
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
        [Authorize, HttpGet, Route("api/stockCard/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListStockCardListBranch(String companyId)
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
        [Authorize, HttpGet, Route("api/stockCard/dropdown/list/item")]
        public List<Entities.MstArticle> DropdownListStockCardListItem()
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
    }
}
