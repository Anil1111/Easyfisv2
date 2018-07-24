using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.POSIntegration.ApiControllers
{
    public class ApiPOSIntegrationTrnReceivingReceiptController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =======================================
        // GET Receiving Receipt - POS Integration
        // =======================================
        [HttpGet, Route("api/get/POSIntegration/receivingReceipt/{receivingReceiptDate}/{branchCode}")]
        public List<Entities.POSIntegrationTrnReceivingReceipt> GetReceivingReceiptPOSIntegration(String receivingReceiptDate, String branchCode)
        {
            var receivingReceipts = from d in db.TrnReceivingReceipts.OrderByDescending(d => d.Id)
                                    where d.RRDate == Convert.ToDateTime(receivingReceiptDate)
                                    && d.IsLocked == true
                                    select new Entities.POSIntegrationTrnReceivingReceipt
                                    {
                                        Branch = d.MstBranch.Branch,
                                        BranchCode = d.MstBranch.BranchCode,
                                        RRNumber = d.RRNumber,
                                        RRDate = d.RRDate.ToShortDateString(),
                                        ListPOSIntegrationTrnReceivingReceiptItem = db.TrnReceivingReceiptItems.Select(i => new Entities.POSIntegrationTrnReceivingReceiptItem
                                        {
                                            RRId = i.RRId,
                                            ItemCode = i.MstArticle.ManualArticleCode,
                                            Item = i.MstArticle.Article,
                                            BranchCode = i.MstBranch.BranchCode,
                                            Unit = i.MstUnit.Unit,
                                            Quantity = i.Quantity,
                                            Cost = i.Cost,
                                            Amount = i.Amount,
                                        }).Where(i => i.RRId == d.Id && i.BranchCode.Equals(branchCode)).ToList(),
                                    };

            return receivingReceipts.ToList();
        }
    }
}
