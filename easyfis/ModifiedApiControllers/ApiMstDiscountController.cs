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
    public class ApiMstDiscountController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =============
        // List Discount
        // =============
        [Authorize, HttpGet, Route("api/discount/list")]
        public List<Entities.MstDiscount> ListDiscount()
        {
            var discounts = from d in db.MstDiscounts.OrderBy(d => d.Discount)
                            select new Entities.MstDiscount
                            {
                                Id = d.Id,
                                Discount = d.Discount,
                                DiscountRate = d.DiscountRate,
                                IsInclusive = d.IsInclusive,
                                AccountId = d.AccountId,
                                Account = d.MstAccount.Account,
                                IsLocked = d.IsLocked,
                                CreatedBy = d.MstUser.FullName,
                                CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                UpdatedBy = d.MstUser1.FullName,
                                UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                            };

            return discounts.ToList();
        }

        // ===============================
        // Dropdown List - Account (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/discount/dropdown/list/account")]
        public List<Entities.MstAccount> DropdownListDiscountAccount()
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
        // Add Discount
        // ============
        [Authorize, HttpPost, Route("api/discount/add")]
        public HttpResponseMessage AddDiscount(Entities.MstDiscount objDiscount)
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
                                Data.MstDiscount newDiscount = new Data.MstDiscount
                                {
                                    Discount = objDiscount.Discount,
                                    DiscountRate = objDiscount.DiscountRate,
                                    IsInclusive = objDiscount.IsInclusive,
                                    AccountId = objDiscount.AccountId,
                                    IsLocked = true,
                                    CreatedById = currentUserId,
                                    CreatedDateTime = DateTime.Now,
                                    UpdatedById = currentUserId,
                                    UpdatedDateTime = DateTime.Now
                                };

                                db.MstDiscounts.InsertOnSubmit(newDiscount);
                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK, newDiscount.Id);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No account found. Please setup at least one account for discounts.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add discount.");
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
        // Update Discount
        // ===============
        [Authorize, HttpPut, Route("api/discount/update/{id}")]
        public HttpResponseMessage UpdateDiscount(Entities.MstDiscount objDiscount, String id)
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
                            var discount = from d in db.MstDiscounts
                                           where d.Id == Convert.ToInt32(id)
                                           select d;

                            if (discount.Any())
                            {
                                var updateDiscount = discount.FirstOrDefault();
                                updateDiscount.Discount = objDiscount.Discount;
                                updateDiscount.DiscountRate = objDiscount.DiscountRate;
                                updateDiscount.IsInclusive = objDiscount.IsInclusive;
                                updateDiscount.AccountId = objDiscount.AccountId;
                                updateDiscount.IsLocked = true;
                                updateDiscount.UpdatedById = currentUserId;
                                updateDiscount.UpdatedDateTime = DateTime.Now;

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These discount details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update discount.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this discount detail page.");
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
        // Delete Discount
        // ===============
        [Authorize, HttpDelete, Route("api/discount/delete/{id}")]
        public HttpResponseMessage DeleteDiscount(String id)
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
                            var discount = from d in db.MstDiscounts
                                           where d.Id == Convert.ToInt32(id)
                                           select d;

                            if (discount.Any())
                            {
                                db.MstDiscounts.DeleteOnSubmit(discount.First());
                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected discount is not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete discount.");
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
