using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.POSIntegrationApiControllers
{
    public class POSIntegrationApiReceivingReceiptController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =======================================
        // GET Receiving Receipt - POS Integration
        // =======================================
        [HttpGet, Route("api/get/POSIntegration/receivingReceipt/{receivingReceiptDate}/{branchCode}")]
        public List<POSIntegrationEntities.POSIntegrationTrnReceivingReceipt> GetReceivingReceiptPOSIntegration(String receivingReceiptDate, String branchCode)
        {
            var receivingReceipts = from d in db.TrnReceivingReceipts.OrderByDescending(d => d.Id)
                                    where d.RRDate == Convert.ToDateTime(receivingReceiptDate)
                                    && d.MstBranch.BranchCode.Equals(branchCode)
                                    && d.IsLocked == true
                                    select new POSIntegrationEntities.POSIntegrationTrnReceivingReceipt
                                    {
                                        Branch = d.MstBranch.Branch,
                                        BranchCode = d.MstBranch.BranchCode,
                                        RRNumber = d.RRNumber,
                                        RRDate = d.RRDate.ToShortDateString(),
                                        DocumentReference = d.DocumentReference,
                                        Supplier = d.MstArticle.Article,
                                        Term = d.MstTerm.Term,
                                        ManualRRNumber = d.ManualRRNumber,
                                        Remarks = d.Remarks,
                                        ReceivedBy = d.MstUser4.FullName,
                                        PreparedBy = d.MstUser3.FullName,
                                        CheckedBy = d.MstUser1.FullName,
                                        ApprovedBy = d.MstUser.FullName,
                                        IsLocked = d.IsLocked,
                                        CreatedBy = d.MstUser2.FullName,
                                        CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                        UpdatedBy = d.MstUser5.FullName,
                                        UpdatedDateTime = d.UpdatedDateTime.ToShortDateString(),
                                        ListPOSIntegrationTrnReceivingReceiptItem = db.TrnReceivingReceiptItems.Select(i => new POSIntegrationEntities.POSIntegrationTrnReceivingReceiptItem
                                        {
                                            RRId = i.RRId,
                                            ItemCode = i.MstArticle.ManualArticleCode,
                                            Item = i.MstArticle.Article,
                                            Particulars = i.Particulars,
                                            Unit = i.MstUnit.Unit,
                                            Quantity = i.Quantity,
                                            Cost = i.Cost,
                                            Amount = i.Amount,
                                            BaseUnit = i.MstUnit1.Unit,
                                            BaseQuantity = i.BaseQuantity,
                                            BaseCost = i.BaseCost
                                        }).Where(i => i.RRId == d.Id).ToList(),
                                    };

            return receivingReceipts.ToList();
        }
    }
}
