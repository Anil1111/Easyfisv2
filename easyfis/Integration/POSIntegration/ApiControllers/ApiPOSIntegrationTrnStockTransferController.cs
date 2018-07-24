using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.POSIntegration.ApiControllers
{
    public class ApiPOSIntegrationTrnStockTransferController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===============================================
        // GET Stock Transfer (POS Integration) - Stock In
        // ===============================================
        [HttpGet, Route("api/get/POSIntegration/stockTransferItems/IN/{stockTransferDate}/{toBranchCode}")]
        public List<Entities.POSIntegrationTrnStockTransfer> GetStockTransferItemsINPOSIntegration(String stockTransferDate, String toBranchCode, Entities.POSIntegrationTrnSalesInvoice POSIntegrationTrnSalesInvoiceObject)
        {
            var stockTransfer = from d in db.TrnStockTransfers.OrderByDescending(d => d.Id)
                                where d.STDate == Convert.ToDateTime(stockTransferDate)
                                && d.MstBranch1.BranchCode.Equals(toBranchCode)
                                && d.IsLocked == true
                                select new Entities.POSIntegrationTrnStockTransfer
                                {
                                    BranchCode = d.MstBranch.BranchCode,
                                    Branch = d.MstBranch.Branch,
                                    STNumber = d.STNumber,
                                    STDate = d.STDate.ToShortDateString(),
                                    ToBranchCode = d.MstBranch1.BranchCode,
                                    ToBranch = d.MstBranch1.Branch,
                                    ListPOSIntegrationTrnStockTransferItem = db.TrnStockTransferItems.Select(i => new Entities.POSIntegrationTrnStockTransferItem
                                    {
                                        STId = i.STId,
                                        ItemCode = i.MstArticle.ManualArticleCode,
                                        Item = i.MstArticle.Article,
                                        Unit = i.MstUnit.Unit,
                                        Quantity = i.Quantity,
                                        Cost = i.Cost,
                                        Amount = i.Amount
                                    }).Where(i => i.STId == d.Id).ToList(),
                                };

            return stockTransfer.ToList();
        }

        // ================================================
        // GET Stock Transfer (POS Integration) - Stock Out
        // ================================================
        [HttpGet, Route("api/get/POSIntegration/stockTransferItems/OT/{stockTransferDate}/{fromBranchCode}")]
        public List<Entities.POSIntegrationTrnStockTransfer> GetStockTransferItemsOTPOSIntegration(String stockTransferDate, String fromBranchCode, Entities.POSIntegrationTrnSalesInvoice POSIntegrationTrnSalesInvoiceObject)
        {
            var stockTransfer = from d in db.TrnStockTransfers.OrderByDescending(d => d.Id)
                                where d.STDate == Convert.ToDateTime(stockTransferDate)
                                && d.MstBranch.BranchCode.Equals(fromBranchCode)
                                && d.IsLocked == true
                                select new Entities.POSIntegrationTrnStockTransfer
                                {
                                    BranchCode = d.MstBranch.BranchCode,
                                    Branch = d.MstBranch.Branch,
                                    STNumber = d.STNumber,
                                    STDate = d.STDate.ToShortDateString(),
                                    ToBranchCode = d.MstBranch1.BranchCode,
                                    ToBranch = d.MstBranch1.Branch,
                                    ListPOSIntegrationTrnStockTransferItem = db.TrnStockTransferItems.Select(i => new Entities.POSIntegrationTrnStockTransferItem
                                    {
                                        STId = i.STId,
                                        ItemCode = i.MstArticle.ManualArticleCode,
                                        Item = i.MstArticle.Article,
                                        Unit = i.MstUnit.Unit,
                                        Quantity = i.Quantity,
                                        Cost = i.Cost,
                                        Amount = i.Amount
                                    }).Where(i => i.STId == d.Id).ToList(),
                                };

            return stockTransfer.ToList();
        }
    }
}
