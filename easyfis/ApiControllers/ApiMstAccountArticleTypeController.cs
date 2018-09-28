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
    public class ApiMstAccountArticleTypeController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

        // =============================================
        // List Account Article Type (Chart of Accounts)
        // =============================================
        [Authorize, HttpGet, Route("api/chartOfAccounts/accountArticleType/list/{accountId}")]
        public List<Entities.MstAccountArticleType> ListChartOfAccountArticleType(String accountId)
        {
            var accountArticleTypes = from d in db.MstAccountArticleTypes
                                      where d.AccountId == Convert.ToInt32(accountId)
                                      select new Entities.MstAccountArticleType
                                      {
                                          Id = d.Id,
                                          AccountId = d.AccountId,
                                          Account = d.MstAccount.Account,
                                          ArticleTypeId = d.ArticleTypeId,
                                          ArticleType = d.MstArticleType.ArticleType
                                      };

            return accountArticleTypes.ToList();
        }

        // ================================================
        // Dropdown List - Article Type (Chart of Accounts)
        // ================================================
        [Authorize, HttpGet, Route("api/chartOfAccounts/accountArticleType/dropdown/list/articleType")]
        public List<Entities.MstArticleType> DropdownListChartOfAccountsArticleType()
        {
            var articleTypes = from d in db.MstArticleTypes.OrderBy(d => d.ArticleType)
                               where d.IsLocked == true
                               select new Entities.MstArticleType
                               {
                                   Id = d.Id,
                                   ArticleType = d.ArticleType
                               };

            return articleTypes.ToList();
        }

        // ============================================
        // Add Account Article Type (Chart of Accounts)
        // ============================================
        [Authorize, HttpPost, Route("api/chartOfAccounts/accountArticleType/add")]
        public HttpResponseMessage AddAccountArticleType(Entities.MstAccountArticleType objAccountArticleType)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;
                    var currentUserLock = currentUser.FirstOrDefault().IsLocked;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("ChartOfAccounts")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var account = from d in db.MstAccounts
                                          where d.IsLocked == true
                                          && d.Id == objAccountArticleType.AccountId
                                          select d;

                            if (account.Any())
                            {
                                Data.MstAccountArticleType newAccountArticleType = new Data.MstAccountArticleType
                                {
                                    AccountId = objAccountArticleType.AccountId,
                                    ArticleTypeId = objAccountArticleType.ArticleTypeId
                                };

                                db.MstAccountArticleTypes.InsertOnSubmit(newAccountArticleType);
                                db.SubmitChanges();

                                String newObject = at.GetObjectString(newAccountArticleType);
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Account not exist.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound, "No rights.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "No rights.");
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

        // ===============================================
        // Update Account Article Type (Chart of Accounts)
        // ===============================================
        [Authorize, HttpPut, Route("api/chartOfAccounts/accountArticleType/update/{id}")]
        public HttpResponseMessage UpdateAccountArticleType(Entities.MstAccountArticleType objAccountArticleType, String id)
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
                                    && d.SysForm.FormName.Equals("ChartOfAccounts")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var accountArticleType = from d in db.MstAccountArticleTypes
                                                     where d.Id == Convert.ToInt32(id)
                                                     select d;

                            if (accountArticleType.Any())
                            {
                                String oldObject = at.GetObjectString(accountArticleType.FirstOrDefault());

                                var account = from d in db.MstAccounts
                                              where d.IsLocked == true
                                              && d.Id == objAccountArticleType.AccountId
                                              select d;

                                if (account.Any())
                                {
                                    var updateAccountArticleType = accountArticleType.FirstOrDefault();
                                    updateAccountArticleType.AccountId = objAccountArticleType.AccountId;
                                    updateAccountArticleType.ArticleTypeId = objAccountArticleType.ArticleTypeId;

                                    db.SubmitChanges();

                                    String newObject = at.GetObjectString(accountArticleType.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.NotFound, "Account not exist.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "This account article detail is no longer available.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound, "No rights.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "No rights.");
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

        // ===============================================
        // Delete Account Article Type (Chart of Accounts)
        // ===============================================
        [Authorize, HttpDelete, Route("api/chartOfAccounts/accountArticleType/delete/{id}")]
        public HttpResponseMessage DeleteAccountArticleType(String id)
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
                                    && d.SysForm.FormName.Equals("ChartOfAccounts")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var accountArticleType = from d in db.MstAccountArticleTypes
                                                     where d.Id == Convert.ToInt32(id)
                                                     select d;

                            if (accountArticleType.Any())
                            {
                                db.MstAccountArticleTypes.DeleteOnSubmit(accountArticleType.First());

                                String oldObject = at.GetObjectString(accountArticleType.FirstOrDefault());
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "This account article type detail is no longer available.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound, "No rights.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "No rights.");
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
