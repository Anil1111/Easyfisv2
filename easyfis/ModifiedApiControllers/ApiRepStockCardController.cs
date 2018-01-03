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
                                                BranchCode = d.MstBranch.BranchCode,
                                                InventoryDate = DateTime.Today,
                                                Quantity = d.Quantity,
                                                BegQuantity = d.Quantity,
                                                InQuantity = d.QuantityIn,
                                                OutQuantity = d.QuantityOut,
                                                EndQuantity = d.Quantity,
                                                Unit = d.MstArticle.MstUnit.Unit,
                                                Cost = d.MstArticleInventory != null ? d.MstArticleInventory.Cost : 0
                                            };

            var groupedStockCardBeginningBalance = from d in stockCardBeginningBalance
                                                   group d by new
                                                   {
                                                       Document = d.Document,
                                                       BranchCode = d.BranchCode,
                                                       InventoryDate = d.InventoryDate,
                                                       Unit = d.Unit,
                                                       Cost = d.Cost
                                                   } into g
                                                   select new
                                                   {
                                                       Document = g.Key.Document,
                                                       BranchCode = g.Key.BranchCode,
                                                       InventoryDate = g.Key.InventoryDate,
                                                       BegQuantity = g.Sum(d => d.BegQuantity),
                                                       InQuantity = g.Sum(d => d.InQuantity),
                                                       OutQuantity = g.Sum(d => d.OutQuantity),
                                                       EndQuantity = g.Sum(d => d.EndQuantity),
                                                       Unit = g.Key.Unit,
                                                       Cost = g.Key.Cost,
                                                       Amount = g.Sum(d => d.Quantity) * g.Key.Cost
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
                                              BegQuantity = d.Quantity,
                                              InQuantity = d.QuantityIn,
                                              OutQuantity = d.QuantityOut,
                                              EndQuantity = d.Quantity,
                                              Unit = d.MstArticle.MstUnit.Unit,
                                              Cost = d.MstArticleInventory != null ? d.MstArticleInventory.Cost : 0,
                                              Amount = d.MstArticleInventory != null ? d.MstArticleInventory.Amount : 0,
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

            if (groupedStockCardBeginningBalance.Any())
            {
                foreach (var begBalance in groupedStockCardBeginningBalance)
                {
                    listStockCardInventory.Add(new Models.TrnInventory()
                    {
                        Document = begBalance.Document,
                        BranchCode = begBalance.BranchCode,
                        InventoryDate = begBalance.InventoryDate.ToShortDateString(),
                        BegQuantity = begBalance.BegQuantity,
                        InQuantity = 0,
                        OutQuantity = 0,
                        EndQuantity = begBalance.EndQuantity,
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
                }
            }

            if (stockCardCurrentBalance.Any())
            {
                foreach (var curBalance in stockCardCurrentBalance)
                {
                    listStockCardInventory.Add(new Models.TrnInventory()
                    {
                        Document = curBalance.Document,
                        BranchCode = curBalance.BranchCode,
                        InventoryDate = curBalance.InventoryDate.ToShortDateString(),
                        BegQuantity = 0,
                        InQuantity = curBalance.InQuantity,
                        OutQuantity = curBalance.OutQuantity,
                        EndQuantity = curBalance.EndQuantity,
                        Unit = curBalance.Unit,
                        Cost = curBalance.Cost,
                        Amount = curBalance.Amount,
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
