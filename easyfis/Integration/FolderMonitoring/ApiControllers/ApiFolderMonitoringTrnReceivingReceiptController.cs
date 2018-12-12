using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.FolderMonitoring.ApiControllers
{
    public class ApiFolderMonitoringTrnReceivingReceiptController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========
        // Business
        // ========
        private Business.AccountsPayable accountsPayable = new Business.AccountsPayable();
        private Business.Inventory inventory = new Business.Inventory();
        private Business.Journal journal = new Business.Journal();

        // ===================
        // Fill Leading Zeroes
        // ===================
        public String FillLeadingZeroes(Int32 number, Int32 length)
        {
            var result = number.ToString();
            var pad = length - result.Length;
            while (pad > 0)
            {
                result = '0' + result;
                pad--;
            }

            return result;
        }

        // ==================
        // Compute VAT Amount
        // ==================
        public Decimal ComputeVATAmount(Boolean IsInclusive, Decimal Amount, Decimal VATRate)
        {
            if (IsInclusive)
            {
                return (Amount / (1 + VATRate / 100)) * (VATRate / 100);
            }
            else
            {
                return Amount * (VATRate / 100); ;
            }
        }

        // ===================
        // Compute WTAX Amount
        // ===================
        public Decimal ComputeWTAXAmount(Boolean IsInclusive, Decimal Amount, Decimal WTAXRate)
        {
            if (IsInclusive)
            {
                return (Amount / (1 + WTAXRate / 100)) * (WTAXRate / 100);
            }
            else
            {
                return Amount * (WTAXRate / 100);
            }
        }

        // =======================================
        // Add Folder Monitoring Receiving Receipt
        // =======================================
        [HttpPost, Route("api/folderMonitoring/receivingReceipt/add")]
        public HttpResponseMessage AddFolderMonitoringReceivingReceipt(List<Entities.FolderMonitoringTrnReceivingReceipt> folderMonitoringTrnReceivingReceiptObjects)
        {
            try
            {
                if (folderMonitoringTrnReceivingReceiptObjects.Any())
                {
                    foreach (var folderMonitoringTrnReceivingReceiptObject in folderMonitoringTrnReceivingReceiptObjects)
                    {
                        Boolean isBranchExist = false,
                                isSupplierExist = false,
                                isTermExist = false,
                                isUserExist = false,
                                isPurchaseOrderExist = false,
                                isItemExist = false,
                                isReceivedBranchExist = false;

                        IQueryable<Data.TrnPurchaseOrder> purchaseOrder = null;

                        var branch = from d in db.MstBranches where d.BranchCode.Equals(folderMonitoringTrnReceivingReceiptObject.BranchCode) select d;
                        if (branch.Any())
                        {
                            isBranchExist = true;

                            if (!folderMonitoringTrnReceivingReceiptObject.PONumber.Equals("") || !folderMonitoringTrnReceivingReceiptObject.PONumber.Equals("NA"))
                            {
                                purchaseOrder = from d in db.TrnPurchaseOrders where d.BranchId == branch.FirstOrDefault().Id && d.PONumber.Equals(folderMonitoringTrnReceivingReceiptObject.PONumber) && d.IsLocked == true select d;
                                if (purchaseOrder.Any()) { isPurchaseOrderExist = true; }
                            }
                        }

                        var supplier = from d in db.MstArticles where d.ArticleTypeId == 3 && d.ManualArticleCode.Equals(folderMonitoringTrnReceivingReceiptObject.SupplierCode) && d.IsLocked == true select d;
                        if (supplier.Any()) { isSupplierExist = true; }

                        var term = from d in db.MstTerms where d.Term.Equals(folderMonitoringTrnReceivingReceiptObject.Term) select d;
                        if (term.Any()) { isTermExist = true; }

                        var user = from d in db.MstUsers where d.UserName.Equals(folderMonitoringTrnReceivingReceiptObject.UserCode) select d;
                        if (user.Any()) { isUserExist = true; }

                        var item = from d in db.MstArticles where d.ArticleTypeId == 1 && d.ManualArticleCode.Equals(folderMonitoringTrnReceivingReceiptObject.ItemCode) && d.IsLocked == true select d;
                        if (item.Any()) { isItemExist = true; }

                        var receivedBranch = from d in db.MstBranches where d.BranchCode.Equals(folderMonitoringTrnReceivingReceiptObject.ReceivedBranchCode) select d;
                        if (receivedBranch.Any()) { isReceivedBranchExist = true; }

                        if (isBranchExist && isSupplierExist && isTermExist && isUserExist && isPurchaseOrderExist && isItemExist && isReceivedBranchExist)
                        {
                            Int32 RRId = 0;

                            var currentReceivingReceipt = from d in db.TrnReceivingReceipts where d.BranchId == branch.FirstOrDefault().Id && d.ManualRRNumber.Equals(folderMonitoringTrnReceivingReceiptObject.ManualRRNumber) && d.IsLocked == true select d;
                            if (currentReceivingReceipt.Any())
                            {
                                RRId = currentReceivingReceipt.FirstOrDefault().Id;

                                var unlockReceivingReceipt = currentReceivingReceipt.FirstOrDefault();
                                unlockReceivingReceipt.IsLocked = false;
                                unlockReceivingReceipt.UpdatedById = user.FirstOrDefault().Id;
                                unlockReceivingReceipt.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnReceivingReceiptObject.CreatedDateTime);
                                db.SubmitChanges();

                                inventory.DeleteReceivingReceiptInventory(RRId);
                                journal.DeleteReceivingReceiptJournal(RRId);
                            }
                            else
                            {
                                var defaultRRNumber = "0000000001";
                                var lastReceivingReceipt = from d in db.TrnReceivingReceipts.OrderByDescending(d => d.Id) where d.BranchId == branch.FirstOrDefault().Id select d;
                                if (lastReceivingReceipt.Any())
                                {
                                    var RRNumber = Convert.ToInt32(lastReceivingReceipt.FirstOrDefault().RRNumber) + 0000000001;
                                    defaultRRNumber = FillLeadingZeroes(RRNumber, 10);
                                }

                                Data.TrnReceivingReceipt newReceivingReceipt = new Data.TrnReceivingReceipt
                                {
                                    BranchId = branch.FirstOrDefault().Id,
                                    RRNumber = defaultRRNumber,
                                    RRDate = Convert.ToDateTime(folderMonitoringTrnReceivingReceiptObject.RRDate),
                                    DocumentReference = folderMonitoringTrnReceivingReceiptObject.DocumentReference,
                                    SupplierId = supplier.FirstOrDefault().Id,
                                    TermId = term.FirstOrDefault().Id,
                                    Remarks = folderMonitoringTrnReceivingReceiptObject.Remarks,
                                    ManualRRNumber = folderMonitoringTrnReceivingReceiptObject.ManualRRNumber,
                                    Amount = 0,
                                    WTaxAmount = 0,
                                    PaidAmount = 0,
                                    AdjustmentAmount = 0,
                                    BalanceAmount = 0,
                                    ReceivedById = user.FirstOrDefault().Id,
                                    PreparedById = user.FirstOrDefault().Id,
                                    CheckedById = user.FirstOrDefault().Id,
                                    ApprovedById = user.FirstOrDefault().Id,
                                    Status = null,
                                    IsCancelled = false,
                                    IsPrinted = false,
                                    IsLocked = true,
                                    CreatedById = user.FirstOrDefault().Id,
                                    CreatedDateTime = Convert.ToDateTime(folderMonitoringTrnReceivingReceiptObject.CreatedDateTime),
                                    UpdatedById = user.FirstOrDefault().Id,
                                    UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnReceivingReceiptObject.CreatedDateTime)
                                };

                                db.TrnReceivingReceipts.InsertOnSubmit(newReceivingReceipt);
                                db.SubmitChanges();

                                RRId = newReceivingReceipt.Id;
                            }

                            var unitConversion = from d in item.FirstOrDefault().MstArticleUnits where d.UnitId == item.FirstOrDefault().UnitId select d;
                            if (unitConversion.Any())
                            {
                                Decimal baseQuantity = folderMonitoringTrnReceivingReceiptObject.Quantity * 1;
                                if (unitConversion.FirstOrDefault().Multiplier > 0)
                                {
                                    baseQuantity = folderMonitoringTrnReceivingReceiptObject.Quantity * (1 / unitConversion.FirstOrDefault().Multiplier);
                                }

                                Decimal amount = folderMonitoringTrnReceivingReceiptObject.Quantity * folderMonitoringTrnReceivingReceiptObject.Cost;
                                Decimal VATAmount = ComputeVATAmount(item.FirstOrDefault().MstTaxType1.IsInclusive, folderMonitoringTrnReceivingReceiptObject.Quantity * folderMonitoringTrnReceivingReceiptObject.Cost, item.FirstOrDefault().MstTaxType1.TaxRate);
                                Decimal WTAXAmount = ComputeWTAXAmount(item.FirstOrDefault().MstTaxType2.IsInclusive, folderMonitoringTrnReceivingReceiptObject.Quantity * folderMonitoringTrnReceivingReceiptObject.Cost, item.FirstOrDefault().MstTaxType2.TaxRate);

                                Decimal baseCost = 0;
                                if (baseQuantity > 0)
                                {
                                    baseCost = (amount - VATAmount + WTAXAmount) / baseQuantity;
                                }

                                Data.TrnReceivingReceiptItem newReceivingReceiptItem = new Data.TrnReceivingReceiptItem
                                {
                                    RRId = RRId,
                                    POId = purchaseOrder.FirstOrDefault().Id,
                                    ItemId = item.FirstOrDefault().Id,
                                    Particulars = folderMonitoringTrnReceivingReceiptObject.Particulars,
                                    UnitId = item.FirstOrDefault().UnitId,
                                    Quantity = folderMonitoringTrnReceivingReceiptObject.Quantity,
                                    Cost = folderMonitoringTrnReceivingReceiptObject.Cost,
                                    Amount = folderMonitoringTrnReceivingReceiptObject.Amount,
                                    VATId = item.FirstOrDefault().InputTaxId,
                                    VATPercentage = item.FirstOrDefault().MstTaxType1.TaxRate,
                                    VATAmount = VATAmount,
                                    WTAXId = item.FirstOrDefault().WTaxTypeId,
                                    WTAXPercentage = item.FirstOrDefault().MstTaxType2.TaxRate,
                                    WTAXAmount = WTAXAmount,
                                    BranchId = receivedBranch.FirstOrDefault().Id,
                                    BaseUnitId = item.FirstOrDefault().UnitId,
                                    BaseQuantity = baseQuantity,
                                    BaseCost = baseCost
                                };

                                db.TrnReceivingReceiptItems.InsertOnSubmit(newReceivingReceiptItem);
                                db.SubmitChanges();

                                var receivingReceipt = from d in db.TrnReceivingReceipts where d.Id == RRId select d;
                                if (receivingReceipt.Any())
                                {
                                    Decimal receivingReceiptAmount = 0, receivingReceiptWTAXAmount = 0;
                                    var receivingReceiptItems = from d in db.TrnReceivingReceiptItems where d.RRId == RRId select d;
                                    if (receivingReceiptItems.Any()) { receivingReceiptAmount = receivingReceiptItems.Sum(d => d.Amount); receivingReceiptWTAXAmount = receivingReceiptItems.Sum(d => d.WTAXAmount); }

                                    var lockReceivingReceipt = receivingReceipt.FirstOrDefault();
                                    lockReceivingReceipt.Amount = receivingReceiptAmount;
                                    lockReceivingReceipt.WTaxAmount = receivingReceiptWTAXAmount;
                                    lockReceivingReceipt.IsLocked = true;
                                    lockReceivingReceipt.UpdatedById = user.FirstOrDefault().Id;
                                    lockReceivingReceipt.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnReceivingReceiptObject.CreatedDateTime);
                                    db.SubmitChanges();

                                    accountsPayable.UpdateAccountsPayable(RRId);
                                    inventory.InsertReceivingReceiptInventory(RRId);
                                    journal.InsertReceivingReceiptJournal(RRId);
                                }
                            }
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No data found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }
    }
}