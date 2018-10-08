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
    public class ApiMstPayTypeController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

        // =============
        // List Pay Type
        // =============
        [Authorize, HttpGet, Route("api/payType/list")]
        public List<Entities.MstPayType> ListPayType()
        {
            var payTypes = from d in db.MstPayTypes.OrderBy(d => d.PayType)
                           select new Entities.MstPayType
                           {
                               Id = d.Id,
                               PayType = d.PayType,
                               AccountId = d.AccountId,
                               Account = d.MstAccount.Account,
                               IsLocked = d.IsLocked,
                               CreatedBy = d.MstUser.FullName,
                               CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                               UpdatedBy = d.MstUser1.FullName,
                               UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                           };

            return payTypes.ToList();
        }

        // ===============================
        // Dropdown List - Account (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/payType/dropdown/list/account")]
        public List<Entities.MstAccount> DropdownListPayTypeAccount()
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

        // ============
        // Add Pay Type
        // ============
        [Authorize, HttpPost, Route("api/payType/add")]
        public HttpResponseMessage AddPayType(Entities.MstPayType objPayType)
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
                                Data.MstPayType newPayType = new Data.MstPayType
                                {
                                    PayType = objPayType.PayType,
                                    AccountId = objPayType.AccountId,
                                    IsLocked = true,
                                    CreatedById = currentUserId,
                                    CreatedDateTime = DateTime.Now,
                                    UpdatedById = currentUserId,
                                    UpdatedDateTime = DateTime.Now
                                };

                                db.MstPayTypes.InsertOnSubmit(newPayType);
                                db.SubmitChanges();

                                String newObject = at.GetObjectString(newPayType);
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                return Request.CreateResponse(HttpStatusCode.OK, newPayType.Id);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No account found. Please setup at least one account for payTypes.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add payType.");
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

        // ===============
        // Update Pay Type
        // ===============
        [Authorize, HttpPut, Route("api/payType/update/{id}")]
        public HttpResponseMessage UpdatePayType(Entities.MstPayType objPayType, String id)
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
                            var payType = from d in db.MstPayTypes
                                          where d.Id == Convert.ToInt32(id)
                                          select d;

                            if (payType.Any())
                            {
                                String oldObject = at.GetObjectString(payType.FirstOrDefault());

                                var updatePayType = payType.FirstOrDefault();
                                updatePayType.PayType = objPayType.PayType;
                                updatePayType.AccountId = objPayType.AccountId;
                                updatePayType.IsLocked = true;
                                updatePayType.UpdatedById = currentUserId;
                                updatePayType.UpdatedDateTime = DateTime.Now;

                                db.SubmitChanges();

                                String newObject = at.GetObjectString(payType.FirstOrDefault());
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These pay type details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update pay type.");
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

        // ===============
        // Delete Pay Type
        // ===============
        [Authorize, HttpDelete, Route("api/payType/delete/{id}")]
        public HttpResponseMessage DeletePayType(String id)
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
                            var payType = from d in db.MstPayTypes
                                          where d.Id == Convert.ToInt32(id)
                                          select d;

                            if (payType.Any())
                            {
                                db.MstPayTypes.DeleteOnSubmit(payType.First());

                                String oldObject = at.GetObjectString(payType.FirstOrDefault());
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected pay type is not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete pay type.");
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
