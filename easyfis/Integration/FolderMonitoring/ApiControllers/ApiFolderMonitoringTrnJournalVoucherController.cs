using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.FolderMonitoring.ApiControllers
{
    public class ApiFolderMonitoringTrnJournalVoucherController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========
        // Business
        // ========
        private Business.AccountsPayable accountsPayable = new Business.AccountsPayable();
        private Business.AccountsReceivable accountsReceivable = new Business.AccountsReceivable();
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

        // =====================================
        // Add Folder Monitoring Journal Voucher
        // =====================================
        [HttpPost, Route("api/folderMonitoring/journalVoucher/add")]
        public HttpResponseMessage AddFolderMonitoringJournalVoucher(List<Entities.FolderMonitoringTrnJournalVoucher> folderMonitoringTrnJournalVoucherObjects)
        {
            try
            {
                if (folderMonitoringTrnJournalVoucherObjects.Any())
                {
                    foreach (var folderMonitoringTrnJournalVoucherObject in folderMonitoringTrnJournalVoucherObjects)
                    {
                        Boolean isBranchExist = false,
                                isUserExist = false,
                                isEntryBranchCodeExist = false,
                                isAccountExist = false,
                                isArticleExist = false,
                                isReceivingReceiptExist = false,
                                isSalesInvoiceExist = false;

                        IQueryable<Data.TrnReceivingReceipt> receivingReceipt = null;
                        IQueryable<Data.TrnSalesInvoice> salesInvoice = null;

                        var branch = from d in db.MstBranches where d.BranchCode.Equals(folderMonitoringTrnJournalVoucherObject.BranchCode) select d;
                        if (branch.Any())
                        {
                            isBranchExist = true;

                            if (!folderMonitoringTrnJournalVoucherObject.APRRNumber.Equals("") || !folderMonitoringTrnJournalVoucherObject.APRRNumber.Equals("NA"))
                            {
                                receivingReceipt = from d in db.TrnReceivingReceipts where d.BranchId == branch.FirstOrDefault().Id && d.RRNumber.Equals(folderMonitoringTrnJournalVoucherObject.APRRNumber) && d.IsLocked == true select d;
                                if (receivingReceipt.Any()) { isReceivingReceiptExist = true; }
                            }

                            if (!folderMonitoringTrnJournalVoucherObject.ARSINumber.Equals("") || !folderMonitoringTrnJournalVoucherObject.ARSINumber.Equals("NA"))
                            {
                                salesInvoice = from d in db.TrnSalesInvoices where d.BranchId == branch.FirstOrDefault().Id && d.SINumber.Equals(folderMonitoringTrnJournalVoucherObject.ARSINumber) && d.IsLocked == true select d;
                                if (salesInvoice.Any()) { isSalesInvoiceExist = true; }
                            }
                        }

                        var user = from d in db.MstUsers where d.UserName.Equals(folderMonitoringTrnJournalVoucherObject.UserCode) select d;
                        if (user.Any()) { isUserExist = true; }

                        var entryBranch = from d in db.MstBranches where d.BranchCode.Equals(folderMonitoringTrnJournalVoucherObject.EntryBranchCode) select d;
                        if (entryBranch.Any()) { isEntryBranchCodeExist = true; }

                        List<easyfis.Entities.MstArticle> listArticles = new List<easyfis.Entities.MstArticle>();

                        var account = from d in db.MstAccounts where d.AccountCode.Equals(folderMonitoringTrnJournalVoucherObject.AccountCode) select d;
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

                        var article = from d in listArticles where d.ManualArticleCode.Equals(folderMonitoringTrnJournalVoucherObject.ArticleCode) select d;
                        if (article.Any()) { isArticleExist = true; }

                        if (isBranchExist && isUserExist && isEntryBranchCodeExist && isAccountExist && isArticleExist)
                        {
                            Int32 JVId = 0;

                            var currentJournalVoucher = from d in db.TrnJournalVouchers where d.BranchId == branch.FirstOrDefault().Id && d.ManualJVNumber.Equals(folderMonitoringTrnJournalVoucherObject.ManualJVNumber) && d.IsLocked == true select d;
                            if (currentJournalVoucher.Any())
                            {
                                JVId = currentJournalVoucher.FirstOrDefault().Id;

                                var unlockJournalVoucher = currentJournalVoucher.FirstOrDefault();
                                unlockJournalVoucher.IsLocked = false;
                                unlockJournalVoucher.UpdatedById = user.FirstOrDefault().Id;
                                unlockJournalVoucher.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnJournalVoucherObject.CreatedDateTime);
                                db.SubmitChanges();

                                journal.DeleteJournalVoucherJournal(JVId);
                            }
                            else
                            {
                                var defaultJVNumber = "0000000001";
                                var lastJournalVoucher = from d in db.TrnJournalVouchers.OrderByDescending(d => d.Id) where d.BranchId == branch.FirstOrDefault().Id select d;
                                if (lastJournalVoucher.Any())
                                {
                                    var JVNumber = Convert.ToInt32(lastJournalVoucher.FirstOrDefault().JVNumber) + 0000000001;
                                    defaultJVNumber = FillLeadingZeroes(JVNumber, 10);
                                }

                                Data.TrnJournalVoucher newJournalVoucher = new Data.TrnJournalVoucher
                                {
                                    BranchId = branch.FirstOrDefault().Id,
                                    JVNumber = defaultJVNumber,
                                    JVDate = Convert.ToDateTime(folderMonitoringTrnJournalVoucherObject.JVDate),
                                    ManualJVNumber = folderMonitoringTrnJournalVoucherObject.ManualJVNumber,
                                    Particulars = folderMonitoringTrnJournalVoucherObject.Remarks,
                                    PreparedById = user.FirstOrDefault().Id,
                                    CheckedById = user.FirstOrDefault().Id,
                                    ApprovedById = user.FirstOrDefault().Id,
                                    Status = null,
                                    IsCancelled = false,
                                    IsPrinted = false,
                                    IsLocked = false,
                                    CreatedById = user.FirstOrDefault().Id,
                                    CreatedDateTime = Convert.ToDateTime(folderMonitoringTrnJournalVoucherObject.CreatedDateTime),
                                    UpdatedById = user.FirstOrDefault().Id,
                                    UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnJournalVoucherObject.CreatedDateTime)
                                };

                                db.TrnJournalVouchers.InsertOnSubmit(newJournalVoucher);
                                db.SubmitChanges();

                                JVId = newJournalVoucher.Id;
                            }

                            Int32? APRRId = null;
                            if (isReceivingReceiptExist) { APRRId = receivingReceipt.FirstOrDefault().Id; }

                            Int32? ARSIId = null;
                            if (isSalesInvoiceExist) { ARSIId = salesInvoice.FirstOrDefault().Id; }

                            Data.TrnJournalVoucherLine newJournalVoucherLine = new Data.TrnJournalVoucherLine
                            {
                                JVId = JVId,
                                BranchId = entryBranch.FirstOrDefault().Id,
                                AccountId = account.FirstOrDefault().Id,
                                ArticleId = article.FirstOrDefault().Id,
                                Particulars = folderMonitoringTrnJournalVoucherObject.Particulars,
                                DebitAmount = folderMonitoringTrnJournalVoucherObject.DebitAmount,
                                CreditAmount = folderMonitoringTrnJournalVoucherObject.CreditAmount,
                                APRRId = APRRId,
                                ARSIId = ARSIId,
                                IsClear = folderMonitoringTrnJournalVoucherObject.IsClear
                            };

                            db.TrnJournalVoucherLines.InsertOnSubmit(newJournalVoucherLine);
                            db.SubmitChanges();

                            var journalVoucher = from d in db.TrnJournalVouchers where d.Id == JVId && d.IsLocked == false select d;
                            if (journalVoucher.Any())
                            {
                                var lockJournalVoucher = journalVoucher.FirstOrDefault();
                                lockJournalVoucher.IsLocked = true;
                                lockJournalVoucher.UpdatedById = user.FirstOrDefault().Id;
                                lockJournalVoucher.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnJournalVoucherObject.CreatedDateTime);
                                db.SubmitChanges();

                                var journalVoucherLines = from d in db.TrnJournalVoucherLines where d.JVId == JVId select d;
                                if (journalVoucherLines.Any())
                                {
                                    foreach (var journalVoucherLine in journalVoucherLines)
                                    {
                                        if (journalVoucherLine.APRRId != null) { accountsPayable.UpdateAccountsPayable(Convert.ToInt32(journalVoucherLine.APRRId)); }
                                        if (journalVoucherLine.ARSIId != null) { accountsReceivable.UpdateAccountsReceivable(Convert.ToInt32(journalVoucherLine.ARSIId)); }
                                    }
                                }

                                journal.InsertJournalVoucherJournal(JVId);
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