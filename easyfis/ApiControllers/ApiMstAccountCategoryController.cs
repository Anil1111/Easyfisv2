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
    public class ApiMstAccountCategoryController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

        // =========================================
        // List Account Category (Chart of Accounts)
        // =========================================
        [Authorize, HttpGet, Route("api/chartOfAccounts/accountCategory/list")]
        public List<Entities.MstAccountCategory> ListChartOfAccountCategory()
        {
            var accountCategories = from d in db.MstAccountCategories
                                    select new Entities.MstAccountCategory
                                    {
                                        Id = d.Id,
                                        AccountCategoryCode = d.AccountCategoryCode,
                                        AccountCategory = d.AccountCategory,
                                        IsLocked = d.IsLocked,
                                        CreatedById = d.CreatedById,
                                        CreatedBy = d.MstUser.FullName,
                                        CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                        UpdatedById = d.UpdatedById,
                                        UpdatedBy = d.MstUser1.FullName,
                                        UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                    };

            return accountCategories.ToList();
        }

        // ========================================
        // Add Account Category (Chart of Accounts)
        // ========================================
        [Authorize, HttpPost, Route("api/chartOfAccounts/accountCategory/add")]
        public HttpResponseMessage AddAccountCategory(Entities.MstAccountCategory objAccountCategory)
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
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            Data.MstAccountCategory newAccountCategory = new Data.MstAccountCategory
                            {
                                AccountCategoryCode = objAccountCategory.AccountCategoryCode,
                                AccountCategory = objAccountCategory.AccountCategory,
                                IsLocked = true,
                                CreatedById = currentUserId,
                                CreatedDateTime = DateTime.Now,
                                UpdatedById = currentUserId,
                                UpdatedDateTime = DateTime.Now
                            };

                            db.MstAccountCategories.InsertOnSubmit(newAccountCategory);
                            db.SubmitChanges();

                            String newObject = at.GetObjectString(newAccountCategory);
                            at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                            return Request.CreateResponse(HttpStatusCode.OK);
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

        // ===========================================
        // Update Account Category (Chart of Accounts)
        // ===========================================
        [Authorize, HttpPut, Route("api/chartOfAccounts/accountCategory/update/{id}")]
        public HttpResponseMessage UpdateAccountCategory(Entities.MstAccountCategory objAccountCategory, String id)
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
                            var accountCategory = from d in db.MstAccountCategories
                                                  where d.Id == Convert.ToUInt32(id)
                                                  select d;

                            if (accountCategory.Any())
                            {
                                String oldObject = at.GetObjectString(accountCategory.FirstOrDefault());

                                var updateAccountCategory = accountCategory.FirstOrDefault();
                                updateAccountCategory.AccountCategoryCode = objAccountCategory.AccountCategoryCode;
                                updateAccountCategory.AccountCategory = objAccountCategory.AccountCategory;
                                updateAccountCategory.IsLocked = true;
                                updateAccountCategory.UpdatedById = currentUserId;
                                updateAccountCategory.UpdatedDateTime = DateTime.Now;

                                db.SubmitChanges();

                                String newObject = at.GetObjectString(accountCategory.FirstOrDefault());
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "This account cash flow detail is no longer available.");
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

        // ===========================================
        // Delete Account Category (Chart of Accounts)
        // ===========================================
        [Authorize, HttpDelete, Route("api/chartOfAccounts/accountCategory/delete/{id}")]
        public HttpResponseMessage DeleteAccountCategory(String id)
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
                            var accountCategory = from d in db.MstAccountCategories
                                                  where d.Id == Convert.ToUInt32(id)
                                                  select d;

                            if (accountCategory.Any())
                            {
                                db.MstAccountCategories.DeleteOnSubmit(accountCategory.First());

                                String oldObject = at.GetObjectString(accountCategory.FirstOrDefault());
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "This account category detail is no longer available.");
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
