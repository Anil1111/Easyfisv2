using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;

namespace easyfis.ModifiedApiControllers
{
    public class ApiTrnDisbursementLineController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ======================
        // List Disbursement Line
        // ======================
        [Authorize, HttpGet, Route("api/disbursementLine/list/{CVId}")]
        public List<Entities.TrnDisbursementLine> ListDisbursementLine(String CVId)
        {
            var disbursementLines = from d in db.TrnDisbursementLines
                                    where d.CVId == Convert.ToInt32(CVId)
                                    select new Entities.TrnDisbursementLine
                                    {
                                        Id = d.Id,
                                        CVId = d.CVId,
                                        BranchId = d.BranchId,
                                        Branch = d.MstBranch.Branch,
                                        AccountId = d.AccountId,
                                        Account = d.MstAccount.Account,
                                        ArticleId = d.ArticleId,
                                        Article = d.MstArticle.Article,
                                        RRId = d.RRId,
                                        RRNumber = d.RRId != null ? d.TrnReceivingReceipt.RRNumber : "",
                                        Particulars = d.Particulars,
                                        Amount = d.Amount
                                    };

            return disbursementLines.ToList();
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/disbursementLine/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListDisbursementLineBranch()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var companyId = currentUser.FirstOrDefault().CompanyId;

            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           where d.CompanyId == companyId
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
        [Authorize, HttpGet, Route("api/disbursementLine/dropdown/list/account")]
        public List<Entities.MstAccount> DropdownListDisbursementLineAccount()
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
        [Authorize, HttpGet, Route("api/disbursementLine/dropdown/list/article/{accountId}")]
        public List<Entities.MstArticle> DropdownListDisbursementLineArticle(String accountId)
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

        // =========================================
        // Dropdown List - Receiving Receipt (Field)
        // =========================================
        [Authorize, HttpGet, Route("api/disbursementLine/dropdown/list/receivingReceipt/{supplierId}")]
        public List<Entities.TrnReceivingReceipt> DropdownListDisbursementLineReceivingReceipt(String supplierId)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var receivingReceipts = from d in db.TrnReceivingReceipts.OrderByDescending(d => d.Id)
                                    where d.SupplierId == Convert.ToInt32(supplierId)
                                    && d.BranchId == branchId
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

        // ======================================
        // Pop-Up List - Receiving Receipt Status
        // ======================================
        [Authorize, HttpGet, Route("api/disbursementLine/popUp/list/receivingReceiptStatus/{supplierId}/{startDate}/{endDate}")]
        public List<Entities.TrnReceivingReceipt> PopUpListDisbursementLineListReceivingReceiptStatus(String supplierId, String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var receivingReceipts = from d in db.TrnReceivingReceipts.OrderByDescending(d => d.Id)
                                    where d.SupplierId == Convert.ToInt32(supplierId)
                                    && d.RRDate >= Convert.ToDateTime(startDate)
                                    && d.RRDate <= Convert.ToDateTime(endDate)
                                    && d.BranchId == branchId
                                    && d.BalanceAmount > 0
                                    && d.IsLocked == true
                                    select new Entities.TrnReceivingReceipt
                                    {
                                        Id = d.Id,
                                        RRNumber = d.RRNumber,
                                        RRDate = d.RRDate.ToShortDateString(),
                                        DocumentReference = d.DocumentReference,
                                        Amount = d.Amount,
                                        PaidAmount = d.PaidAmount,
                                        AdjustmentAmount = d.AdjustmentAmount,
                                        BalanceAmount = d.BalanceAmount
                                    };

            return receivingReceipts.ToList();
        }

        // =============================================================
        // Apply (Download) Disbursement Line - Receiving Receipt Status
        // =============================================================
        [Authorize, HttpPost, Route("api/disbursementLine/popUp/apply/receivingReceiptStatus/{CVId}")]
        public HttpResponseMessage ApplyReceivingReceiptStatusDisbursementLine(List<Entities.TrnDisbursementLine> objDisbursementLines, String CVId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;
                    var currentBranchId = currentUser.FirstOrDefault().BranchId;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("DisbursementDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var disbursement = from d in db.TrnDisbursements
                                               where d.Id == Convert.ToInt32(CVId)
                                               select d;

                            if (disbursement.Any())
                            {
                                if (!disbursement.FirstOrDefault().IsLocked)
                                {
                                    foreach (var objDisbursementLine in objDisbursementLines)
                                    {
                                        var receivingReceipt = from d in db.TrnReceivingReceipts
                                                               where d.Id == objDisbursementLine.RRId
                                                               && d.BranchId == currentBranchId
                                                               && d.IsLocked == true
                                                               select d;

                                        if (receivingReceipt.Any())
                                        {
                                            Data.TrnDisbursementLine newDisbursementLine = new Data.TrnDisbursementLine
                                            {
                                                CVId = Convert.ToInt32(CVId),
                                                BranchId = receivingReceipt.FirstOrDefault().BranchId,
                                                AccountId = receivingReceipt.FirstOrDefault().MstArticle.AccountId,
                                                ArticleId = receivingReceipt.FirstOrDefault().SupplierId,
                                                RRId = receivingReceipt.FirstOrDefault().Id,
                                                Particulars = receivingReceipt.FirstOrDefault().Remarks,
                                                Amount = objDisbursementLine.Amount,
                                            };

                                            db.TrnDisbursementLines.InsertOnSubmit(newDisbursementLine);
                                            db.SubmitChanges();
                                        }
                                    }

                                    Decimal disbursementItemTotalAmount = 0;

                                    if (disbursement.FirstOrDefault().TrnDisbursementLines.Any())
                                    {
                                        disbursementItemTotalAmount = disbursement.FirstOrDefault().TrnDisbursementLines.Sum(d => d.Amount);
                                    }

                                    var updateDisbursement = disbursement.FirstOrDefault();
                                    updateDisbursement.Amount = disbursementItemTotalAmount;
                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new disbursement line if the current disbursement detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current disbursement details are not found in the server. Please add new disbursement first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new disbursement line in this disbursement detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this disbursement detail page.");
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

        // ===============================
        // Pop-Up List - Supplier Advances
        // ===============================
        [Authorize, HttpGet, Route("api/disbursementLine/popUp/list/supplierAdvances/{supplierId}")]
        public List<Entities.TrnJournal> PopUpListDisbursementLineListSupplierAdvances(String supplierId)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;
            var supplierAdvancesAccountId = currentUser.FirstOrDefault().SupplierAdvancesAccountId;

            var journals = from d in db.TrnJournals
                           where d.ArticleId == Convert.ToInt32(supplierId)
                           && d.AccountId == supplierAdvancesAccountId
                           && d.BranchId == branchId
                           group d by new
                           {
                               BranchId = d.BranchId,
                               Branch = d.MstBranch.Branch,
                               AccountId = d.AccountId,
                               Account = d.MstAccount.Account,
                               AccountCode = d.MstAccount.AccountCode,
                               ArticleId = d.ArticleId,
                               Article = d.MstArticle.Article
                           } into g
                           select new Entities.TrnJournal
                           {
                               BranchId = g.Key.BranchId,
                               Branch = g.Key.Branch,
                               AccountId = g.Key.AccountId,
                               Account = g.Key.Account,
                               AccountCode = g.Key.AccountCode,
                               ArticleId = g.Key.ArticleId,
                               Article = g.Key.Article,
                               DebitAmount = g.Sum(d => d.DebitAmount),
                               CreditAmount = g.Sum(d => d.CreditAmount),
                               Balance = g.Sum(d => d.DebitAmount) - g.Sum(d => d.CreditAmount)
                           };

            return journals.Where(d => d.Balance != 0).ToList();
        }

        // ======================================================
        // Apply (Download) Disbursement Line - Supplier Advances
        // ======================================================
        [Authorize, HttpPost, Route("api/disbursementLine/popUp/apply/supplierAdvances/{CVId}")]
        public HttpResponseMessage ApplySupplierAdvancesDisbursementLine(Entities.TrnDisbursementLine objDisbursementLine, String CVId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;
                    var currentBranchId = currentUser.FirstOrDefault().BranchId;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("DisbursementDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var disbursement = from d in db.TrnDisbursements
                                               where d.Id == Convert.ToInt32(CVId)
                                               select d;

                            if (disbursement.Any())
                            {
                                if (!disbursement.FirstOrDefault().IsLocked)
                                {
                                    var journals = from d in db.TrnJournals
                                                   where d.ArticleId == objDisbursementLine.ArticleId
                                                   && d.AccountId == objDisbursementLine.AccountId
                                                   && d.BranchId == currentBranchId
                                                   group d by new
                                                   {
                                                       BranchId = d.BranchId,
                                                       AccountId = d.AccountId,
                                                       ArticleId = d.ArticleId,
                                                   } into g
                                                   select new
                                                   {
                                                       BranchId = g.Key.BranchId,
                                                       AccountId = g.Key.AccountId,
                                                       ArticleId = g.Key.ArticleId,
                                                       DebitAmount = g.Sum(d => d.DebitAmount),
                                                       CreditAmount = g.Sum(d => d.CreditAmount),
                                                       BalanceAmount = g.Sum(d => d.DebitAmount) - g.Sum(d => d.CreditAmount)
                                                   };

                                    if (journals.Any())
                                    {
                                        var advances = from d in journals.ToList()
                                                       select new
                                                       {
                                                           BranchId = d.BranchId,
                                                           AccountId = d.AccountId,
                                                           ArticleId = d.ArticleId,
                                                           DebitAmount = d.DebitAmount,
                                                           CreditAmount = d.CreditAmount,
                                                           BalanceAmount = d.BalanceAmount
                                                       };

                                        if (advances.Any())
                                        {
                                            Data.TrnDisbursementLine newDisbursementLine = new Data.TrnDisbursementLine
                                            {
                                                CVId = Convert.ToInt32(CVId),
                                                BranchId = advances.FirstOrDefault().BranchId,
                                                AccountId = advances.FirstOrDefault().AccountId,
                                                ArticleId = advances.FirstOrDefault().ArticleId,
                                                RRId = null,
                                                Particulars = "Supplier Advances",
                                                Amount = advances.FirstOrDefault().BalanceAmount * -1,
                                            };

                                            db.TrnDisbursementLines.InsertOnSubmit(newDisbursementLine);
                                            db.SubmitChanges();

                                            Decimal disbursementItemTotalAmount = 0;

                                            if (disbursement.FirstOrDefault().TrnDisbursementLines.Any())
                                            {
                                                disbursementItemTotalAmount = disbursement.FirstOrDefault().TrnDisbursementLines.Sum(d => d.Amount);
                                            }

                                            var updateDisbursement = disbursement.FirstOrDefault();
                                            updateDisbursement.Amount = disbursementItemTotalAmount;
                                            db.SubmitChanges();

                                            return Request.CreateResponse(HttpStatusCode.OK);
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.BadRequest, "No advances found.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No journal data found.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new disbursement line if the current disbursement detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current disbursement details are not found in the server. Please add new disbursement first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new disbursement line in this disbursement detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this disbursement detail page.");
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

        // =====================
        // Add Disbursement Line
        // =====================
        [Authorize, HttpPost, Route("api/disbursementLine/add/{CVId}")]
        public HttpResponseMessage AddDisbursementLine(Entities.TrnDisbursementLine objDisbursementLine, String CVId)
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
                                    && d.SysForm.FormName.Equals("DisbursementDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var disbursement = from d in db.TrnDisbursements
                                               where d.Id == Convert.ToInt32(CVId)
                                               select d;

                            if (disbursement.Any())
                            {
                                if (!disbursement.FirstOrDefault().IsLocked)
                                {
                                    var accounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                                                   where d.Id == objDisbursementLine.AccountId
                                                   && d.IsLocked == true
                                                   select d;

                                    if (accounts.Any())
                                    {
                                        var articles = from d in db.MstArticles
                                                       where d.Id == objDisbursementLine.ArticleId
                                                       && d.IsLocked == true
                                                       select d;

                                        if (articles.Any())
                                        {
                                            Data.TrnDisbursementLine newDisbursementLine = new Data.TrnDisbursementLine
                                            {
                                                CVId = Convert.ToInt32(CVId),
                                                BranchId = objDisbursementLine.BranchId,
                                                AccountId = objDisbursementLine.AccountId,
                                                ArticleId = objDisbursementLine.ArticleId,
                                                RRId = objDisbursementLine.RRId,
                                                Particulars = objDisbursementLine.Particulars,
                                                Amount = objDisbursementLine.Amount,
                                            };

                                            db.TrnDisbursementLines.InsertOnSubmit(newDisbursementLine);
                                            db.SubmitChanges();

                                            Decimal disbursementItemTotalAmount = 0;

                                            if (disbursement.FirstOrDefault().TrnDisbursementLines.Any())
                                            {
                                                disbursementItemTotalAmount = disbursement.FirstOrDefault().TrnDisbursementLines.Sum(d => d.Amount);
                                            }

                                            var updateDisbursement = disbursement.FirstOrDefault();
                                            updateDisbursement.Amount = disbursementItemTotalAmount;
                                            db.SubmitChanges();

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
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new disbursement line if the current disbursement detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current disbursement details are not found in the server. Please add new disbursement first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new disbursement line in this disbursement detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this disbursement detail page.");
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

        // ========================
        // Update Disbursement Line
        // ========================
        [Authorize, HttpPut, Route("api/disbursementLine/update/{id}/{CVId}")]
        public HttpResponseMessage UpdateDisbursementLine(Entities.TrnDisbursementLine objDisbursementLine, String id, String CVId)
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
                                    && d.SysForm.FormName.Equals("DisbursementDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var disbursement = from d in db.TrnDisbursements
                                               where d.Id == Convert.ToInt32(CVId)
                                               select d;

                            if (disbursement.Any())
                            {
                                if (!disbursement.FirstOrDefault().IsLocked)
                                {
                                    var disbursementLine = from d in db.TrnDisbursementLines
                                                           where d.Id == Convert.ToInt32(id)
                                                           select d;

                                    if (disbursementLine.Any())
                                    {
                                        var accounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                                                       where d.Id == objDisbursementLine.AccountId
                                                       && d.IsLocked == true
                                                       select d;

                                        if (accounts.Any())
                                        {
                                            var articles = from d in db.MstArticles
                                                           where d.Id == objDisbursementLine.ArticleId
                                                           && d.IsLocked == true
                                                           select d;

                                            if (articles.Any())
                                            {
                                                var updateDisbursementLine = disbursementLine.FirstOrDefault();
                                                updateDisbursementLine.CVId = Convert.ToInt32(CVId);
                                                updateDisbursementLine.BranchId = objDisbursementLine.BranchId;
                                                updateDisbursementLine.AccountId = objDisbursementLine.AccountId;
                                                updateDisbursementLine.ArticleId = objDisbursementLine.ArticleId;
                                                updateDisbursementLine.RRId = objDisbursementLine.RRId;
                                                updateDisbursementLine.Particulars = objDisbursementLine.Particulars;
                                                updateDisbursementLine.Amount = objDisbursementLine.Amount;

                                                db.SubmitChanges();

                                                Decimal disbursementItemTotalAmount = 0;

                                                if (disbursement.FirstOrDefault().TrnDisbursementLines.Any())
                                                {
                                                    disbursementItemTotalAmount = disbursement.FirstOrDefault().TrnDisbursementLines.Sum(d => d.Amount);
                                                }

                                                var updateDisbursement = disbursement.FirstOrDefault();
                                                updateDisbursement.Amount = disbursementItemTotalAmount;
                                                db.SubmitChanges();

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
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This disbursement line detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new disbursement line if the current disbursement detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current disbursement details are not found in the server. Please add new disbursement first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update disbursement line in this disbursement detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this disbursement detail page.");
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

        // ========================
        // Delete Disbursement Line
        // ========================
        [Authorize, HttpDelete, Route("api/disbursementLine/delete/{id}/{CVId}")]
        public HttpResponseMessage DeleteDisbursementLine(String id, String CVId)
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
                                    && d.SysForm.FormName.Equals("DisbursementDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var disbursement = from d in db.TrnDisbursements
                                               where d.Id == Convert.ToInt32(CVId)
                                               select d;

                            if (disbursement.Any())
                            {
                                if (!disbursement.FirstOrDefault().IsLocked)
                                {
                                    var disbursementLine = from d in db.TrnDisbursementLines
                                                           where d.Id == Convert.ToInt32(id)
                                                           select d;

                                    if (disbursementLine.Any())
                                    {
                                        db.TrnDisbursementLines.DeleteOnSubmit(disbursementLine.First());
                                        db.SubmitChanges();

                                        Decimal disbursementItemTotalAmount = 0;

                                        if (disbursement.FirstOrDefault().TrnDisbursementLines.Any())
                                        {
                                            disbursementItemTotalAmount = disbursement.FirstOrDefault().TrnDisbursementLines.Sum(d => d.Amount);
                                        }

                                        var updateDisbursement = disbursement.FirstOrDefault();
                                        updateDisbursement.Amount = disbursementItemTotalAmount;
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This disbursement line detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot apply purchase order items to disbursement line if the current disbursement detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current disbursement details are not found in the server. Please add new disbursement first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete disbursement line in this disbursement detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this disbursement detail page.");
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
