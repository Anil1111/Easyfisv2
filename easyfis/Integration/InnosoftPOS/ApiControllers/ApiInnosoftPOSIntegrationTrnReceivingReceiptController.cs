using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.InnosoftPOS.ApiControllers
{
    public class ApiInnosoftPOSIntegrationTrnReceivingReceiptController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =================================================
        // List Receiving Receipt (Innosoft POS Integration)
        // =================================================
        [HttpGet, Route("api/innosoftPOSIntegration/receivingReceipt/list/{branchCode}/{RRDate}")]
        public List<Entities.InnosoftPOSIntegrationTrnReceivingReceipt> ListInnosoftPOSIntegrationReceivingReceipt(String branchCode, String RRDate)
        {
            var receivingReceipts = from d in db.TrnReceivingReceipts.OrderByDescending(d => d.Id)
                                    where d.RRDate == Convert.ToDateTime(RRDate)
                                    && d.IsLocked == true
                                    select new Entities.InnosoftPOSIntegrationTrnReceivingReceipt
                                    {
                                        Branch = d.MstBranch.Branch,
                                        BranchCode = d.MstBranch.BranchCode,
                                        RRNumber = d.RRNumber,
                                        RRDate = d.RRDate.ToShortDateString(),
                                        ManualRRNumber = d.ManualRRNumber,
                                        ListReceivingReceiptItem = db.TrnReceivingReceiptItems.Select(i => new Entities.InnosoftPOSIntegrationTrnReceivingReceiptItem
                                        {
                                            RRId = i.RRId,
                                            BranchCode = i.MstBranch.BranchCode,
                                            Branch = i.MstBranch.Branch,
                                            ManualItemCode = i.MstArticle.ManualArticleCode,
                                            ItemDescription = i.MstArticle.Article,
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