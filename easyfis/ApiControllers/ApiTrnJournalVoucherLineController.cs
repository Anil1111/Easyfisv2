using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;
using System.Reflection;

namespace easyfis.ModifiedApiControllers
{
    public class ApiTrnJournalVoucherLineController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

        // =========================
        // List Journal Voucher Line
        // =========================
        [Authorize, HttpGet, Route("api/journalVoucherLine/list/{JVId}")]
        public List<Entities.TrnJournalVoucherLine> ListJournalVoucherLine(String JVId)
        {
            var journalVoucherLines = from d in db.TrnJournalVoucherLines
                                      where d.JVId == Convert.ToInt32(JVId)
                                      select new Entities.TrnJournalVoucherLine
                                      {
                                          Id = d.Id,
                                          JVId = d.JVId,
                                          BranchId = d.BranchId,
                                          Branch = d.MstBranch.Branch,
                                          AccountId = d.AccountId,
                                          Account = d.MstAccount.Account,
                                          ArticleId = d.ArticleId,
                                          Article = d.MstArticle.Article,
                                          Particulars = d.Particulars,
                                          DebitAmount = d.DebitAmount,
                                          CreditAmount = d.CreditAmount,
                                          APRRId = d.APRRId,
                                          APRRNumber = d.APRRId != null ? d.TrnReceivingReceipt.RRNumber : "",
                                          ARSIId = d.ARSIId,
                                          ARSINumber = d.ARSIId != null ? d.TrnSalesInvoice.SINumber : "",
                                          IsClear = d.IsClear
                                      };

            return journalVoucherLines.ToList();
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/journalVoucherLine/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListJournalVoucherLineBranch()
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // ===============================
        // Dropdown List - Account (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/journalVoucherLine/dropdown/list/account")]
        public List<Entities.MstAccount> DropdownListJournalVoucherLineAccount()
        {
            var accounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                           where d.IsLocked == true
                           select new Entities.MstAccount
                           {
                               Id = d.Id,
                               AccountCode = d.AccountCode,
                               Account = d.Account
                           };

            return accounts.ToList();
        }

        // ===============================
        // Dropdown List - Article (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/journalVoucherLine/dropdown/list/article/{accountId}")]
        public List<Entities.MstArticle> DropdownListJournalVoucherLineArticle(String accountId)
        {
            List<Entities.MstArticle> listArticles = new List<Entities.MstArticle>();

            var accountArticleTypes = from d in db.MstAccountArticleTypes
                                      where d.AccountId == Convert.ToInt32(accountId)
                                      && d.MstAccount.IsLocked == true
                                      select d;

            if (accountArticleTypes.Any())
            {
                foreach (var accountArticleType in accountArticleTypes)
                {
                    var articles = from d in db.MstArticles
                                   where d.ArticleTypeId == accountArticleType.ArticleTypeId
                                   && d.IsLocked == true
                                   select d;

                    if (articles.Any())
                    {
                        foreach (var article in articles)
                        {
                            listArticles.Add(new Entities.MstArticle()
                            {
                                Id = article.Id,
                                Article = article.Article
                            });
                        }
                    }
                }
            }

            return listArticles.OrderBy(d => d.Article).ToList();
        }

        // ==========================================================
        // Dropdown List - Accounts Payable Receiving Receipt (Field)
        // ==========================================================
        [Authorize, HttpGet, Route("api/journalVoucherLine/dropdown/list/accountsPayableReceivingReceipt")]
        public List<Entities.TrnReceivingReceipt> DropdownListJournalVoucherLineAccountsPayableReceivingReceipt()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var receivingReceipts = from d in db.TrnReceivingReceipts.OrderByDescending(d => d.Id)
                                    where d.BranchId == branchId
                                    && d.BalanceAmount > 0
                                    && d.IsLocked == true
                                    select new Entities.TrnReceivingReceipt
                                    {
                                        Id = d.Id,
                                        RRNumber = d.RRNumber,
                                        RRDate = d.RRDate.ToShortDateString(),
                                        BalanceAmount = d.BalanceAmount
                                    };

            return receivingReceipts.ToList();
        }

        // =========================================================
        // Dropdown List - Accounts Receivable Sales Invoice (Field)
        // =========================================================
        [Authorize, HttpGet, Route("api/journalVoucherLine/dropdown/list/accountsReceivableSalesInvoice")]
        public List<Entities.TrnSalesInvoice> DropdownListJournalVoucherLineAccountsReceivableSalesInvoice()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var salesInvoices = from d in db.TrnSalesInvoices.OrderByDescending(d => d.Id)
                                where d.BranchId == branchId
                                && d.BalanceAmount > 0
                                && d.IsLocked == true
                                select new Entities.TrnSalesInvoice
                                {
                                    Id = d.Id,
                                    SINumber = d.SINumber,
                                    SIDate = d.SIDate.ToShortDateString(),
                                    BalanceAmount = d.BalanceAmount
                                };

            return salesInvoices.ToList();
        }

        // ========================
        // Add Journal Voucher Line
        // ========================
        [Authorize, HttpPost, Route("api/journalVoucherLine/add/{JVId}")]
        public HttpResponseMessage AddJournalVoucherLine(Entities.TrnJournalVoucherLine objJournalVoucherLine, String JVId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("JournalVoucherDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var journalVoucher = from d in db.TrnJournalVouchers
                                                 where d.Id == Convert.ToInt32(JVId)
                                                 select d;

                            if (journalVoucher.Any())
                            {
                                if (!journalVoucher.FirstOrDefault().IsLocked)
                                {
                                    var accounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                                                   where d.Id == objJournalVoucherLine.AccountId
                                                   && d.IsLocked == true
                                                   select d;

                                    if (accounts.Any())
                                    {
                                        var articles = from d in db.MstArticles
                                                       where d.Id == objJournalVoucherLine.ArticleId
                                                       && d.IsLocked == true
                                                       select d;

                                        if (articles.Any())
                                        {
                                            Data.TrnJournalVoucherLine newJournalVoucherLine = new Data.TrnJournalVoucherLine
                                            {
                                                JVId = Convert.ToInt32(JVId),
                                                BranchId = objJournalVoucherLine.BranchId,
                                                AccountId = objJournalVoucherLine.AccountId,
                                                ArticleId = objJournalVoucherLine.ArticleId,
                                                Particulars = objJournalVoucherLine.Particulars,
                                                DebitAmount = objJournalVoucherLine.DebitAmount,
                                                CreditAmount = objJournalVoucherLine.CreditAmount,
                                                APRRId = objJournalVoucherLine.APRRId,
                                                ARSIId = objJournalVoucherLine.ARSIId,
                                                IsClear = objJournalVoucherLine.IsClear
                                            };

                                            db.TrnJournalVoucherLines.InsertOnSubmit(newJournalVoucherLine);
                                            db.SubmitChanges();

                                            String newObject = at.GetObjectString(newJournalVoucherLine);
                                            at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                            return Request.CreateResponse(HttpStatusCode.OK);
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.BadRequest, "No Article.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No Account.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new journal voucher line if the current journal voucher detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current journal voucher details are not found in the server. Please add new journal voucher first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new journal voucher line in this journal voucher detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this journal voucher detail page.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // ===========================
        // Update Journal Voucher Line
        // ===========================
        [Authorize, HttpPut, Route("api/journalVoucherLine/update/{id}/{JVId}")]
        public HttpResponseMessage UpdateJournalVoucherLine(Entities.TrnJournalVoucherLine objJournalVoucherLine, String id, String JVId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("JournalVoucherDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var journalVoucher = from d in db.TrnJournalVouchers
                                                 where d.Id == Convert.ToInt32(JVId)
                                                 select d;

                            if (journalVoucher.Any())
                            {
                                if (!journalVoucher.FirstOrDefault().IsLocked)
                                {
                                    var journalVoucherLine = from d in db.TrnJournalVoucherLines
                                                             where d.Id == Convert.ToInt32(id)
                                                             select d;

                                    if (journalVoucherLine.Any())
                                    {
                                        String oldObject = at.GetObjectString(journalVoucherLine.FirstOrDefault());

                                        var accounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                                                       where d.Id == objJournalVoucherLine.AccountId
                                                       && d.IsLocked == true
                                                       select d;

                                        if (accounts.Any())
                                        {
                                            var articles = from d in db.MstArticles
                                                           where d.Id == objJournalVoucherLine.ArticleId
                                                           && d.IsLocked == true
                                                           select d;

                                            if (articles.Any())
                                            {
                                                var updateJournalVoucherLine = journalVoucherLine.FirstOrDefault();
                                                updateJournalVoucherLine.JVId = Convert.ToInt32(JVId);
                                                updateJournalVoucherLine.BranchId = objJournalVoucherLine.BranchId;
                                                updateJournalVoucherLine.AccountId = objJournalVoucherLine.AccountId;
                                                updateJournalVoucherLine.ArticleId = objJournalVoucherLine.ArticleId;
                                                updateJournalVoucherLine.Particulars = objJournalVoucherLine.Particulars;
                                                updateJournalVoucherLine.DebitAmount = objJournalVoucherLine.DebitAmount;
                                                updateJournalVoucherLine.CreditAmount = objJournalVoucherLine.CreditAmount;
                                                updateJournalVoucherLine.APRRId = objJournalVoucherLine.APRRId;
                                                updateJournalVoucherLine.ARSIId = objJournalVoucherLine.ARSIId;
                                                updateJournalVoucherLine.IsClear = objJournalVoucherLine.IsClear;

                                                db.SubmitChanges();

                                                String newObject = at.GetObjectString(journalVoucherLine.FirstOrDefault());
                                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                                return Request.CreateResponse(HttpStatusCode.OK);
                                            }
                                            else
                                            {
                                                return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item has no unit conversion.");
                                            }
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item was not found in the server.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This journal voucher line detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new journal voucher line if the current journal voucher detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current journal voucher details are not found in the server. Please add new journal voucher first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update journal voucher line in this journal voucher detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this journal voucher detail page.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // ===========================
        // Delete Journal Voucher Line
        // ===========================
        [Authorize, HttpDelete, Route("api/journalVoucherLine/delete/{id}/{JVId}")]
        public HttpResponseMessage DeleteJournalVoucherLine(String id, String JVId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("JournalVoucherDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var journalVoucher = from d in db.TrnJournalVouchers
                                                 where d.Id == Convert.ToInt32(JVId)
                                                 select d;

                            if (journalVoucher.Any())
                            {
                                if (!journalVoucher.FirstOrDefault().IsLocked)
                                {
                                    var journalVoucherLine = from d in db.TrnJournalVoucherLines
                                                             where d.Id == Convert.ToInt32(id)
                                                             select d;

                                    if (journalVoucherLine.Any())
                                    {
                                        db.TrnJournalVoucherLines.DeleteOnSubmit(journalVoucherLine.First());

                                        String oldObject = at.GetObjectString(journalVoucherLine.FirstOrDefault());
                                        at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This journal voucher line detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot apply purchase order items to journal voucher line if the current journal voucher detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current journal voucher details are not found in the server. Please add new journal voucher first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete journal voucher line in this journal voucher detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this journal voucher detail page.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }
    }
}
