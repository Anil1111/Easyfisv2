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
    public class ApiMstTaxTypeController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =============
        // List Tax Type
        // =============
        [Authorize, HttpGet, Route("api/taxType/list")]
        public List<Entities.MstTaxType> ListTaxType()
        {
            var taxTypes = from d in db.MstTaxTypes.OrderBy(d => d.TaxType)
                           select new Entities.MstTaxType
                           {
                               Id = d.Id,
                               TaxType = d.TaxType,
                               TaxRate = d.TaxRate,
                               IsInclusive = d.IsInclusive,
                               AccountId = d.AccountId,
                               Account = d.MstAccount.Account,
                               IsLocked = d.IsLocked,
                               CreatedById = d.CreatedById,
                               CreatedBy = d.MstUser.FullName,
                               CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                               UpdatedById = d.UpdatedById,
                               UpdatedBy = d.MstUser1.FullName,
                               UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                           };

            return taxTypes.ToList();
        }

        // ===============================
        // Dropdown List - Account (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/taxType/dropdown/list/account")]
        public List<Entities.MstAccount> DropdownListTaxTypeAccount()
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
        // Add Tax Type
        // ============
        [Authorize, HttpPost, Route("api/taxType/add")]
        public HttpResponseMessage AddTaxType(Entities.MstTaxType objTaxType)
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
                                Data.MstTaxType newTaxType = new Data.MstTaxType
                                {
                                    TaxType = objTaxType.TaxType,
                                    TaxRate = objTaxType.TaxRate,
                                    IsInclusive = objTaxType.IsInclusive,
                                    AccountId = objTaxType.AccountId,
                                    IsLocked = true,
                                    CreatedById = currentUserId,
                                    CreatedDateTime = DateTime.Now,
                                    UpdatedById = currentUserId,
                                    UpdatedDateTime = DateTime.Now
                                };

                                db.MstTaxTypes.InsertOnSubmit(newTaxType);
                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK, newTaxType.Id);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No account found. Please setup at least one account for tax types.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add tax type.");
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
        // Update Tax Type
        // ===============
        [Authorize, HttpPut, Route("api/taxType/update/{id}")]
        public HttpResponseMessage UpdateTaxType(Entities.MstTaxType objTaxType, String id)
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
                            var taxType = from d in db.MstTaxTypes
                                          where d.Id == Convert.ToInt32(id)
                                          select d;

                            if (taxType.Any())
                            {
                                var updateTaxType = taxType.FirstOrDefault();
                                updateTaxType.TaxType = objTaxType.TaxType;
                                updateTaxType.TaxRate = objTaxType.TaxRate;
                                updateTaxType.IsInclusive = objTaxType.IsInclusive;
                                updateTaxType.AccountId = objTaxType.AccountId;
                                updateTaxType.IsLocked = true;
                                updateTaxType.UpdatedById = currentUserId;
                                updateTaxType.UpdatedDateTime = DateTime.Now;

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These tax type details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update tax type.");
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
        // Delete Tax Type
        // ===============
        [Authorize, HttpDelete, Route("api/taxType/delete/{id}")]
        public HttpResponseMessage DeleteTaxType(String id)
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
                            var taxType = from d in db.MstTaxTypes
                                          where d.Id == Convert.ToInt32(id)
                                          select d;

                            if (taxType.Any())
                            {
                                db.MstTaxTypes.DeleteOnSubmit(taxType.First());
                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected tax type is not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete tax type.");
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
