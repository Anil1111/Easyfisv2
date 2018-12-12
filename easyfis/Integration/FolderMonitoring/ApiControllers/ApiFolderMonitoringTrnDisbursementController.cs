using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.FolderMonitoring.ApiControllers
{
    public class ApiFolderMonitoringTrnDisbursementController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========
        // Business
        // ========
        private Business.AccountsPayable accountsPayable = new Business.AccountsPayable();
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

        // ==================================
        // Add Folder Monitoring Disbursement
        // ==================================
        [HttpPost, Route("api/folderMonitoring/disbursement/add")]
        public HttpResponseMessage AddFolderMonitoringDisbursement(List<Entities.FolderMonitoringTrnDisbursement> folderMonitoringTrnDisbursementObjects)
        {
            try
            {
                if (folderMonitoringTrnDisbursementObjects.Any())
                {
                    foreach (var folderMonitoringTrnDisbursementObject in folderMonitoringTrnDisbursementObjects)
                    {
                        Boolean isBranchExist = false,
                                isSupplierExist = false,
                                isPayTypeExist = false,
                                isBankExist = false,
                                isUserExist = false,
                                isAccountExist = false,
                                isArticleExist = false,
                                isReceivingReceiptExist = false;

                        IQueryable<Data.TrnReceivingReceipt> receivingReceipt = null;

                        var branch = from d in db.MstBranches where d.BranchCode.Equals(folderMonitoringTrnDisbursementObject.BranchCode) select d;
                        if (branch.Any())
                        {
                            isBranchExist = true;

                            if (!folderMonitoringTrnDisbursementObject.RRNumber.Equals("") || !folderMonitoringTrnDisbursementObject.RRNumber.Equals("NA"))
                            {
                                receivingReceipt = from d in db.TrnReceivingReceipts where d.BranchId == branch.FirstOrDefault().Id && d.RRNumber.Equals(folderMonitoringTrnDisbursementObject.RRNumber) && d.IsLocked == true select d;
                                if (receivingReceipt.Any()) { isReceivingReceiptExist = true; }
                            }
                        }

                        var supplier = from d in db.MstArticles where d.ArticleTypeId == 3 && d.ManualArticleCode.Equals(folderMonitoringTrnDisbursementObject.SupplierCode) && d.IsLocked == true select d;
                        if (supplier.Any()) { isSupplierExist = true; }

                        var payType = from d in db.MstPayTypes where d.PayType.Equals(folderMonitoringTrnDisbursementObject.PayType) select d;
                        if (payType.Any()) { isPayTypeExist = true; }

                        var bank = from d in db.MstArticles where d.ArticleTypeId == 5 && d.ManualArticleCode.Equals(folderMonitoringTrnDisbursementObject.BankCode) select d;
                        if (bank.Any()) { isBankExist = true; }

                        var user = from d in db.MstUsers where d.UserName.Equals(folderMonitoringTrnDisbursementObject.UserCode) select d;
                        if (user.Any()) { isUserExist = true; }

                        List<easyfis.Entities.MstArticle> listArticles = new List<easyfis.Entities.MstArticle>();

                        var account = from d in db.MstAccounts where d.AccountCode.Equals(folderMonitoringTrnDisbursementObject.AccountCode) select d;
                        if (account.Any())
                        {
                            isAccountExist = true;

                            var accountArticleTypes = from d in db.MstAccountArticleTypes where d.AccountId == Convert.ToInt32(account.FirstOrDefault().Id) select d;
                            if (accountArticleTypes.Any())
                            {
                                foreach (var accountArticleType in accountArticleTypes)
                                {
                                    var articles = from d in db.MstArticles where d.ArticleTypeId == accountArticleType.ArticleTypeId && d.IsLocked == true select d;
                                    if (articles.Any())
                                    {
                                        foreach (var articleObject in articles)
                                        {
                                            listArticles.Add(new easyfis.Entities.MstArticle()
                                            {
                                                Id = articleObject.Id,
                                                ManualArticleCode = articleObject.ManualArticleCode,
                                                Article = articleObject.Article
                                            });
                                        }
                                    }
                                }
                            }
                        }

                        var article = from d in listArticles where d.ManualArticleCode.Equals(folderMonitoringTrnDisbursementObject.ArticleCode) select d;
                        if (article.Any()) { isArticleExist = true; }

                        if (isBranchExist && isSupplierExist && isPayTypeExist && isBankExist && isUserExist && isAccountExist && isArticleExist)
                        {
                            Int32 CVId = 0;

                            var currentDisbursement = from d in db.TrnDisbursements where d.BranchId == branch.FirstOrDefault().Id && d.ManualCVNumber.Equals(folderMonitoringTrnDisbursementObject.ManualCVNumber) && d.IsLocked == true select d;
                            if (currentDisbursement.Any())
                            {
                                CVId = currentDisbursement.FirstOrDefault().Id;

                                var unlockDisbursement = currentDisbursement.FirstOrDefault();
                                unlockDisbursement.IsLocked = false;
                                unlockDisbursement.UpdatedById = user.FirstOrDefault().Id;
                                unlockDisbursement.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnDisbursementObject.CreatedDateTime);
                                db.SubmitChanges();

                                journal.DeleteCashVoucherJournal(CVId);
                            }
                            else
                            {
                                var defaultCVNumber = "0000000001";
                                var lastDisbursement = from d in db.TrnDisbursements.OrderByDescending(d => d.Id) where d.BranchId == branch.FirstOrDefault().Id select d;
                                if (lastDisbursement.Any())
                                {
                                    var CVNumber = Convert.ToInt32(lastDisbursement.FirstOrDefault().CVNumber) + 0000000001;
                                    defaultCVNumber = FillLeadingZeroes(CVNumber, 10);
                                }

                                Data.TrnDisbursement newDisbursement = new Data.TrnDisbursement
                                {
                                    BranchId = branch.FirstOrDefault().Id,
                                    CVNumber = defaultCVNumber,
                                    CVDate = Convert.ToDateTime(folderMonitoringTrnDisbursementObject.CVDate),
                                    ManualCVNumber = folderMonitoringTrnDisbursementObject.ManualCVNumber,
                                    Payee = folderMonitoringTrnDisbursementObject.Payee,
                                    SupplierId = supplier.FirstOrDefault().Id,
                                    PayTypeId = payType.FirstOrDefault().Id,
                                    Particulars = folderMonitoringTrnDisbursementObject.Remarks,
                                    BankId = bank.FirstOrDefault().Id,
                                    CheckNumber = folderMonitoringTrnDisbursementObject.CheckNumber,
                                    CheckDate = Convert.ToDateTime(folderMonitoringTrnDisbursementObject.CheckDate),
                                    Amount = 0,
                                    IsCrossCheck = folderMonitoringTrnDisbursementObject.IsCrossCheck,
                                    PreparedById = user.FirstOrDefault().Id,
                                    CheckedById = user.FirstOrDefault().Id,
                                    ApprovedById = user.FirstOrDefault().Id,
                                    IsClear = folderMonitoringTrnDisbursementObject.IsClear,
                                    Status = null,
                                    IsCancelled = false,
                                    IsPrinted = false,
                                    IsLocked = true,
                                    CreatedById = user.FirstOrDefault().Id,
                                    CreatedDateTime = Convert.ToDateTime(folderMonitoringTrnDisbursementObject.CreatedDateTime),
                                    UpdatedById = user.FirstOrDefault().Id,
                                    UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnDisbursementObject.CreatedDateTime)
                                };

                                db.TrnDisbursements.InsertOnSubmit(newDisbursement);
                                db.SubmitChanges();

                                CVId = newDisbursement.Id;
                            }

                            Int32? RRId = null;
                            if (isReceivingReceiptExist) { RRId = receivingReceipt.FirstOrDefault().Id; }

                            Data.TrnDisbursementLine newDisbursementLine = new Data.TrnDisbursementLine
                            {
                                CVId = CVId,
                                BranchId = branch.FirstOrDefault().Id,
                                AccountId = account.FirstOrDefault().Id,
                                ArticleId = article.FirstOrDefault().Id,
                                RRId = RRId,
                                Particulars = folderMonitoringTrnDisbursementObject.Particulars,
                                Amount = folderMonitoringTrnDisbursementObject.Amount
                            };

                            db.TrnDisbursementLines.InsertOnSubmit(newDisbursementLine);
                            db.SubmitChanges();

                            var disbursement = from d in db.TrnDisbursements where d.Id == CVId && d.IsLocked == true select d;
                            if (disbursement.Any())
                            {
                                Decimal amount = 0;
                                var disbursementLines = from d in disbursement.FirstOrDefault().TrnDisbursementLines select d;
                                if (disbursementLines.Any()) { amount = disbursementLines.Sum(d => d.Amount); }

                                var lockDisbursement = disbursement.FirstOrDefault();
                                lockDisbursement.Amount = amount;
                                lockDisbursement.IsLocked = true;
                                lockDisbursement.UpdatedById = user.FirstOrDefault().Id;
                                lockDisbursement.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnDisbursementObject.CreatedDateTime);
                                db.SubmitChanges();

                                if (disbursementLines.Any())
                                {
                                    foreach (var disbursementLine in disbursementLines)
                                    {
                                        if (disbursementLine.RRId != null) { accountsPayable.UpdateAccountsPayable(Convert.ToInt32(disbursementLine.RRId)); }
                                    }
                                }

                                journal.InsertCashVoucherJournal(CVId);
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
