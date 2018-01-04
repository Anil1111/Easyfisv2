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
        public List<Models.TrnInventory> ListStockCard(String startDate, String endDate, String companyId, String branchId, String itemId)
        {
            var stockCardBeginningBalance = from d in db.TrnInventories
                                            where d.InventoryDate < Convert.ToDateTime(startDate)
                                            && d.MstArticleInventory.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                            && d.MstArticleInventory.BranchId == Convert.ToInt32(branchId)
                                            && d.MstArticleInventory.MstArticle.IsInventory == true
                                            && d.ArticleId == Convert.ToInt32(itemId)
                                            select new
                                            {
                                                Document = "Beginning Balance",
                                                InventoryDate = Convert.ToDateTime(startDate),
                                                Quantity = d.Quantity,
                                                InQuantity = d.QuantityIn,
                                                OutQuantity = d.QuantityOut,
                                                BalanceQuantity = d.Quantity,
                                                Unit = d.MstArticleInventory.MstArticle.MstUnit.Unit,
                                                Amount = d.MstArticleInventory != null ? d.MstArticleInventory.Amount : 0
                                            };

            var groupedStockCardBeginningBalance = from d in stockCardBeginningBalance
                                                   group d by new
                                                   {
                                                       Document = d.Document,
                                                       InventoryDate = d.InventoryDate,
                                                       Unit = d.Unit
                                                   } into g
                                                   select new
                                                   {
                                                       Document = g.Key.Document,
                                                       InventoryDate = g.Key.InventoryDate,
                                                       InQuantity = g.Sum(d => d.InQuantity),
                                                       OutQuantity = g.Sum(d => d.OutQuantity),
                                                       BalanceQuantity = g.Sum(d => d.BalanceQuantity),
                                                       Unit = g.Key.Unit,
                                                       Cost = g.Sum(d => d.InQuantity) > 0 ? g.Sum(d => d.Amount) / g.Sum(d => d.InQuantity) : 0,
                                                       Amount = g.Sum(d => d.InQuantity) > 0 ? g.Sum(d => d.Amount) : 0
                                                   };

            var stockCardCurrentBalance = from d in db.TrnInventories
                                          where d.InventoryDate >= Convert.ToDateTime(startDate)
                                          && d.InventoryDate <= Convert.ToDateTime(endDate)
                                          && d.MstArticleInventory.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                          && d.MstArticleInventory.BranchId == Convert.ToInt32(branchId)
                                          && d.MstArticleInventory.MstArticle.IsInventory == true
                                          && d.ArticleId == Convert.ToInt32(itemId)
                                          select new
                                          {
                                              Document = "Current",
                                              BranchCode = d.MstBranch.BranchCode,
                                              InventoryDate = d.InventoryDate,
                                              Quantity = d.Quantity,
                                              InQuantity = d.QuantityIn,
                                              OutQuantity = d.QuantityOut,
                                              BalanceQuantity = d.Quantity,
                                              Unit = d.MstArticleInventory.MstArticle.MstUnit.Unit,
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
                                              STNumber = d.TrnStockTransfer.STNumber
                                          };

            List<Models.TrnInventory> listStockCardInventory = new List<Models.TrnInventory>();

            Decimal beginningQuantity = 0;

            if (groupedStockCardBeginningBalance.Any())
            {
                var begBalance = groupedStockCardBeginningBalance.FirstOrDefault();

                beginningQuantity = begBalance.InQuantity > 0 ? begBalance.BalanceQuantity : 0;

                listStockCardInventory.Add(new Models.TrnInventory()
                {
                    Document = begBalance.Document,
                    BranchCode = " ",
                    InventoryDate = begBalance.InventoryDate.ToShortDateString(),
                    InQuantity = begBalance.InQuantity,
                    OutQuantity = begBalance.OutQuantity,
                    BalanceQuantity = begBalance.BalanceQuantity,
                    RunQuantity = begBalance.BalanceQuantity,
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
                    STNumber = "Beginning Balance"
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
                    amount = curBalance.InQuantity > 0 ? curBalance.Amount : 0;

                    listStockCardInventory.Add(new Models.TrnInventory()
                    {
                        Document = curBalance.Document,
                        BranchCode = curBalance.BranchCode,
                        InventoryDate = curBalance.InventoryDate.ToShortDateString(),
                        InQuantity = curBalance.InQuantity,
                        OutQuantity = curBalance.OutQuantity,
                        BalanceQuantity = curBalance.BalanceQuantity,
                        RunQuantity = runningQuantity,
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
                        STNumber = curBalance.STNumber
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
    }
}
