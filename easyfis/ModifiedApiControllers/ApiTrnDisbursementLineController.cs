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
                                        Branch = d.MstBranch.Branch,
                                        Account = d.MstAccount.Account,
                                        Article = d.MstArticle.Article,
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

            return listArticles;
        }

        // =========================================
        // Dropdown List - Receiving Receipt (Field)
        // =========================================
        [Authorize, HttpGet, Route("api/disbursementLine/dropdown/list/receivingReceipt/{supplierId}")]
        public List<Entities.TrnReceivingReceipt> DropdownListDisbursementLineReceivingReceipt(String supplierId)
        {
            var receivingReceipts = from d in db.TrnReceivingReceipts
                                    where d.SupplierId == Convert.ToInt32(supplierId)
                                    && d.BalanceAmount > 0
                                    && d.IsLocked == true
                                    select new Entities.TrnReceivingReceipt
                                    {
                                        Id = d.Id,
                                        RRNumber = d.RRNumber,
                                        RRDate = d.RRDate.ToShortDateString(),
                                        Amount = d.Amount
                                    };

            return receivingReceipts.ToList();
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
                                                updateDisbursementLine.CVId = objDisbursementLine.CVId;
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
