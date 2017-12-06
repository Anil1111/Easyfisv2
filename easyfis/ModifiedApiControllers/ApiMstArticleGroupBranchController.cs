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
    public class ApiMstArticleGroupBranchBranchController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =========================
        // List Article Group Branch
        // =========================
        [Authorize, HttpGet, Route("api/articleGroupBranch/list/{articleGroupId}")]
        public List<Entities.MstArticleGroupBranch> ListArticleGroupBranch(String articleGroupId)
        {
            var articleGroupBranches = from d in db.MstArticleGroupBranches.OrderBy(d => d.MstBranch.Branch)
                                       where d.ArticleGroupId == Convert.ToInt32(articleGroupId)
                                       select new Entities.MstArticleGroupBranch
                                       {
                                           Id = d.Id,
                                           ArticleGroupId = d.ArticleGroupId,
                                           CompanyId = d.MstBranch.CompanyId,
                                           BranchId = d.BranchId,
                                           Branch = d.MstBranch.Branch,
                                           AccountId = d.AccountId,
                                           SalesAccountId = d.SalesAccountId,
                                           CostAccountId = d.CostAccountId,
                                           AssetAccountId = d.AssetAccountId,
                                           ExpenseAccountId = d.ExpenseAccountId,
                                       };

            return articleGroupBranches.ToList();
        }

        // ===============================
        // Dropdown List - Company (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/articleGroupBranch/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListArticleGroupBranchCompany()
        {
            var companies = from d in db.MstCompanies.OrderBy(d => d.Company)
                            select new Entities.MstCompany
                            {
                                Id = d.Id,
                                Company = d.Company
                            };

            return companies.ToList();
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/articleGroupBranch/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListArticleGroupBranchListBranch(String companyId)
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           where d.CompanyId == Convert.ToInt32(companyId)
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
        [Authorize, HttpGet, Route("api/articleGroupBranch/dropdown/list/account")]
        public List<Entities.MstAccount> DropdownListArticleGroupBranchAccount()
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

        // ========================
        // Add Article Group Branch
        // ========================
        [Authorize, HttpPost, Route("api/articleGroupBranch/add")]
        public HttpResponseMessage AddArticleGroupBranch(Entities.MstArticleGroupBranch objArticleGroupBranch)
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
                                    && d.SysForm.FormName.Equals("SystemTables")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var accounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                                           where d.IsLocked == true
                                           select d;

                            if (accounts.Any())
                            {
                                Data.MstArticleGroupBranch newArticleGroupBranch = new Data.MstArticleGroupBranch
                                {
                                    ArticleGroupId = objArticleGroupBranch.ArticleGroupId,
                                    BranchId = objArticleGroupBranch.BranchId,
                                    AccountId = objArticleGroupBranch.AccountId,
                                    SalesAccountId = objArticleGroupBranch.SalesAccountId,
                                    CostAccountId = objArticleGroupBranch.CostAccountId,
                                    AssetAccountId = objArticleGroupBranch.AssetAccountId,
                                    ExpenseAccountId = objArticleGroupBranch.ExpenseAccountId
                                };

                                db.MstArticleGroupBranches.InsertOnSubmit(newArticleGroupBranch);
                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK, newArticleGroupBranch.Id);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No account found. Please setup at least one account for article group branches.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add article group branch.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this system table page.");
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
        // Update Article Group Branch
        // ===========================
        [Authorize, HttpPut, Route("api/articleGroupBranch/update/{id}")]
        public HttpResponseMessage UpdateArticleGroupBranch(Entities.MstArticleGroupBranch objArticleGroupBranch, String id)
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
                                    && d.SysForm.FormName.Equals("SystemTables")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var articleGroupBranch = from d in db.MstArticleGroupBranches
                                                     where d.Id == Convert.ToInt32(id)
                                                     select d;

                            if (articleGroupBranch.Any())
                            {
                                var updateArticleGroupBranch = articleGroupBranch.FirstOrDefault();
                                updateArticleGroupBranch.ArticleGroupId = objArticleGroupBranch.ArticleGroupId;
                                updateArticleGroupBranch.BranchId = objArticleGroupBranch.BranchId;
                                updateArticleGroupBranch.AccountId = objArticleGroupBranch.AccountId;
                                updateArticleGroupBranch.SalesAccountId = objArticleGroupBranch.SalesAccountId;
                                updateArticleGroupBranch.CostAccountId = objArticleGroupBranch.CostAccountId;
                                updateArticleGroupBranch.AssetAccountId = objArticleGroupBranch.AssetAccountId;
                                updateArticleGroupBranch.ExpenseAccountId = objArticleGroupBranch.ExpenseAccountId;

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These article group branch details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update article group branch.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this system table page.");
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
        // Delete Article Group Branch
        // ===========================
        [Authorize, HttpDelete, Route("api/articleGroupBranch/delete/{id}")]
        public HttpResponseMessage DeleteArticleGroupBranch(String id)
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
                                    && d.SysForm.FormName.Equals("SystemTables")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var articleGroupBranch = from d in db.MstArticleGroupBranches
                                                     where d.Id == Convert.ToInt32(id)
                                                     select d;

                            if (articleGroupBranch.Any())
                            {
                                db.MstArticleGroupBranches.DeleteOnSubmit(articleGroupBranch.First());
                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected article group branch is not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete article group branch.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this system table page.");
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
