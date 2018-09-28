﻿using System;
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
    public class ApiMstAccountTypeController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

        // =====================================
        // List Account Type (Chart of Accounts)
        // =====================================
        [Authorize, HttpGet, Route("api/chartOfAccounts/accountType/list")]
        public List<Entities.MstAccountType> ListChartOfAccountType()
        {
            var accountTypes = from d in db.MstAccountTypes.OrderByDescending(d => d.Id)
                               select new Entities.MstAccountType
                               {
                                   Id = d.Id,
                                   AccountTypeCode = d.AccountTypeCode,
                                   AccountType = d.AccountType,
                                   AccountCategoryId = d.AccountCategoryId,
                                   AccountCategory = d.MstAccountCategory.AccountCategory,
                                   SubCategoryDescription = d.SubCategoryDescription,
                                   IsLocked = d.IsLocked,
                                   CreatedById = d.CreatedById,
                                   CreatedBy = d.MstUser.FullName,
                                   CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                   UpdatedById = d.UpdatedById,
                                   UpdatedBy = d.MstUser1.FullName,
                                   UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                               };

            return accountTypes.ToList();
        }

        // ====================================================
        // Dropdown List - Account Category (Chart of Accounts)
        // ====================================================
        [Authorize, HttpGet, Route("api/chartOfAccounts/accountType/dropdown/list/accountCategory")]
        public List<Entities.MstAccountCategory> DropdownListChartOfAccountsAccountCategory()
        {
            var accountCategories = from d in db.MstAccountCategories.OrderBy(d => d.AccountCategory)
                                    where d.IsLocked == true
                                    select new Entities.MstAccountCategory
                                    {
                                        Id = d.Id,
                                        AccountCategory = d.AccountCategory
                                    };

            return accountCategories.ToList();
        }

        // ====================================
        // Add Account Type (Chart of Accounts)
        // ====================================
        [Authorize, HttpPost, Route("api/chartOfAccounts/accountType/add")]
        public HttpResponseMessage AddAccountType(Entities.MstAccountType objAccountType)
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
                            var accountCategories = from d in db.MstAccountCategories
                                                    where d.IsLocked == true
                                                    && d.Id == objAccountType.AccountCategoryId
                                                    select d;

                            if (accountCategories.Any())
                            {
                                Data.MstAccountType newAccountType = new Data.MstAccountType
                                {
                                    AccountTypeCode = objAccountType.AccountTypeCode,
                                    AccountType = objAccountType.AccountType,
                                    AccountCategoryId = objAccountType.AccountCategoryId,
                                    SubCategoryDescription = objAccountType.SubCategoryDescription,
                                    IsLocked = true,
                                    CreatedById = currentUserId,
                                    CreatedDateTime = DateTime.Now,
                                    UpdatedById = currentUserId,
                                    UpdatedDateTime = DateTime.Now
                                };

                                db.MstAccountTypes.InsertOnSubmit(newAccountType);
                                db.SubmitChanges();

                                String newObject = at.GetObjectString(newAccountType);
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Account category not found.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "No rights.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No rights.");
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

        // =======================================
        // Update Account Type (Chart of Accounts)
        // =======================================
        [Authorize, HttpPut, Route("api/chartOfAccounts/accountType/update/{id}")]
        public HttpResponseMessage UpdateAccountType(Entities.MstAccountType objAccountType, String id)
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
                            var accountCategories = from d in db.MstAccountCategories
                                                    where d.IsLocked == true
                                                    && d.Id == objAccountType.AccountCategoryId
                                                    select d;

                            if (accountCategories.Any())
                            {
                                var accountType = from d in db.MstAccountTypes
                                                  where d.Id == Convert.ToUInt32(id)
                                                  select d;

                                if (accountType.Any())
                                {
                                    String oldObject = at.GetObjectString(accountType.FirstOrDefault());

                                    var updateAccountType = accountType.FirstOrDefault();
                                    updateAccountType.AccountTypeCode = objAccountType.AccountTypeCode;
                                    updateAccountType.AccountType = objAccountType.AccountType;
                                    updateAccountType.AccountCategoryId = objAccountType.AccountCategoryId;
                                    updateAccountType.SubCategoryDescription = objAccountType.SubCategoryDescription;
                                    updateAccountType.IsLocked = true;
                                    updateAccountType.UpdatedById = currentUserId;
                                    updateAccountType.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    String newObject = at.GetObjectString(accountType.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.NotFound, "This account type detail is no longer available.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Account category not found.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "No rights.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No rights.");
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

        // =======================================
        // Delete Account Type (Chart of Accounts)
        // =======================================
        [Authorize, HttpDelete, Route("api/chartOfAccounts/accountType/delete/{id}")]
        public HttpResponseMessage DeleteAccountType(String id)
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
                            var accountType = from d in db.MstAccountTypes
                                              where d.Id == Convert.ToUInt32(id)
                                              select d;

                            if (accountType.Any())
                            {
                                db.MstAccountTypes.DeleteOnSubmit(accountType.First());

                                String oldObject = at.GetObjectString(accountType.FirstOrDefault());
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "This account type detail is no longer available.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "No rights.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No rights.");
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
