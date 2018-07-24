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
    public class ApiMstArticleGroupController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==================
        // List Article Group
        // ==================
        [Authorize, HttpGet, Route("api/articleGroup/list")]
        public List<Entities.MstArticleGroup> ListArticleGroup()
        {
            var articleGroups = from d in db.MstArticleGroups.OrderBy(d => d.ArticleGroup)
                                select new Entities.MstArticleGroup
                                {
                                    Id = d.Id,
                                    ArticleGroup = d.ArticleGroup,
                                    ArticleTypeId = d.ArticleTypeId,
                                    ArticleType = d.MstArticleType.ArticleType,
                                    AccountId = d.AccountId,
                                    Account = d.MstAccount.Account,
                                    SalesAccountId = d.SalesAccountId,
                                    SalesAccount = d.MstAccount1.Account,
                                    CostAccountId = d.CostAccountId,
                                    CostAccount = d.MstAccount2.Account,
                                    AssetAccountId = d.AssetAccountId,
                                    AssetAccount = d.MstAccount3.Account,
                                    ExpenseAccountId = d.ExpenseAccountId,
                                    ExpenseAccount = d.MstAccount4.Account,
                                    IsLocked = d.IsLocked,
                                    CreatedById = d.CreatedById,
                                    CreatedBy = d.MstUser.FullName,
                                    CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                    UpdatedById = d.UpdatedById,
                                    UpdatedBy = d.MstUser1.FullName,
                                    UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                };

            return articleGroups.ToList();
        }

        // ====================================
        // Dropdown List - Article Type (Field)
        // ====================================
        [Authorize, HttpGet, Route("api/articleGroup/dropdown/list/articleType")]
        public List<Entities.MstArticleType> DropdownListArticleGroupArticleType()
        {
            var articleTypes = from d in db.MstArticleTypes.OrderBy(d => d.Id)
                               where d.IsLocked == true
                               select new Entities.MstArticleType
                               {
                                   Id = d.Id,
                                   ArticleType = d.ArticleType
                               };

            return articleTypes.ToList();
        }

        // ===============================
        // Dropdown List - Account (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/articleGroup/dropdown/list/account")]
        public List<Entities.MstAccount> DropdownListArticleGroupAccount()
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

        // =================
        // Add Article Group
        // =================
        [Authorize, HttpPost, Route("api/articleGroup/add")]
        public HttpResponseMessage AddArticleGroup(Entities.MstArticleGroup objArticleGroup)
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
                                Data.MstArticleGroup newArticleGroup = new Data.MstArticleGroup
                                {
                                    ArticleGroup = objArticleGroup.ArticleGroup,
                                    ArticleTypeId = objArticleGroup.ArticleTypeId,
                                    AccountId = objArticleGroup.AccountId,
                                    SalesAccountId = objArticleGroup.SalesAccountId,
                                    CostAccountId = objArticleGroup.CostAccountId,
                                    AssetAccountId = objArticleGroup.AssetAccountId,
                                    ExpenseAccountId = objArticleGroup.ExpenseAccountId,
                                    IsLocked = true,
                                    CreatedById = currentUserId,
                                    CreatedDateTime = DateTime.Now,
                                    UpdatedById = currentUserId,
                                    UpdatedDateTime = DateTime.Now
                                };

                                db.MstArticleGroups.InsertOnSubmit(newArticleGroup);
                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK, newArticleGroup.Id);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No account found. Please setup at least one account for article groups.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add article group.");
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

        // ====================
        // Update Article Group
        // ====================
        [Authorize, HttpPut, Route("api/articleGroup/update/{id}")]
        public HttpResponseMessage UpdateArticleGroup(Entities.MstArticleGroup objArticleGroup, String id)
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
                            var articleGroup = from d in db.MstArticleGroups
                                               where d.Id == Convert.ToInt32(id)
                                               select d;

                            if (articleGroup.Any())
                            {
                                var updateArticleGroup = articleGroup.FirstOrDefault();
                                updateArticleGroup.ArticleGroup = objArticleGroup.ArticleGroup;
                                updateArticleGroup.ArticleTypeId = objArticleGroup.ArticleTypeId;
                                updateArticleGroup.AccountId = objArticleGroup.AccountId;
                                updateArticleGroup.SalesAccountId = objArticleGroup.SalesAccountId;
                                updateArticleGroup.CostAccountId = objArticleGroup.CostAccountId;
                                updateArticleGroup.AssetAccountId = objArticleGroup.AssetAccountId;
                                updateArticleGroup.ExpenseAccountId = objArticleGroup.ExpenseAccountId;
                                updateArticleGroup.IsLocked = true;
                                updateArticleGroup.UpdatedById = currentUserId;
                                updateArticleGroup.UpdatedDateTime = DateTime.Now;

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These article group details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update article group.");
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

        // ====================
        // Delete Article Group
        // ====================
        [Authorize, HttpDelete, Route("api/articleGroup/delete/{id}")]
        public HttpResponseMessage DeleteArticleGroup(String id)
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
                            var articleGroup = from d in db.MstArticleGroups
                                               where d.Id == Convert.ToInt32(id)
                                               select d;

                            if (articleGroup.Any())
                            {
                                db.MstArticleGroups.DeleteOnSubmit(articleGroup.First());
                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected article group is not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete article group.");
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
