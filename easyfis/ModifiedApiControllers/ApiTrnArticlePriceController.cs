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
    public class ApiTrnArticlePriceController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===============
        // List Item Price
        // ===============
        [Authorize, HttpGet, Route("api/itemPrice/list/{startDate}/{endDate}")]
        public List<Entities.TrnArticlePrice> ListItemPrice(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var itemPrices = from d in db.TrnArticlePrices.OrderByDescending(d => d.Id)
                             where d.BranchId == branchId
                             && d.IPDate >= Convert.ToDateTime(startDate)
                             && d.IPDate <= Convert.ToDateTime(endDate)
                             select new Entities.TrnArticlePrice
                             {
                                 Id = d.Id,
                                 IPNumber = d.IPNumber,
                                 IPDate = d.IPDate.ToShortDateString(),
                                 ManualIPNumber = d.ManualIPNumber,
                                 Particulars = d.Particulars,
                                 PreparedById = d.PreparedById,
                                 CheckedById = d.CheckedById,
                                 ApprovedById = d.ApprovedById,
                                 IsLocked = d.IsLocked,
                                 CreatedById = d.CreatedById,
                                 CreatedBy = d.MstUser3.FullName,
                                 CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                 UpdatedById = d.UpdatedById,
                                 UpdatedBy = d.MstUser4.FullName,
                                 UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                             };

            return itemPrices.ToList();
        }

        // =================
        // Detail Item Price
        // =================
        [Authorize, HttpGet, Route("api/itemPrice/detail/{id}")]
        public Entities.TrnArticlePrice DetailItemPrice(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var itemPrice = from d in db.TrnArticlePrices.OrderByDescending(d => d.Id)
                            where d.BranchId == branchId
                            && d.Id == Convert.ToInt32(id)
                            select new Entities.TrnArticlePrice
                            {
                                Id = d.Id,
                                IPNumber = d.IPNumber,
                                IPDate = d.IPDate.ToShortDateString(),
                                ManualIPNumber = d.ManualIPNumber,
                                Particulars = d.Particulars,
                                PreparedById = d.PreparedById,
                                CheckedById = d.CheckedById,
                                ApprovedById = d.ApprovedById,
                                IsLocked = d.IsLocked,
                                CreatedById = d.CreatedById,
                                CreatedBy = d.MstUser3.FullName,
                                CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                UpdatedById = d.UpdatedById,
                                UpdatedBy = d.MstUser4.FullName,
                                UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                            };

            if (itemPrice.Any())
            {
                return itemPrice.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/itemPrice/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListItemPriceBranch()
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // ============================
        // Dropdown List - User (Field)
        // ============================
        [Authorize, HttpGet, Route("api/itemPrice/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListItemPriceUsers()
        {
            var users = from d in db.MstUsers.OrderBy(d => d.FullName)
                        where d.IsLocked == true
                        select new Entities.MstUser
                        {
                            Id = d.Id,
                            FullName = d.FullName
                        };

            return users.ToList();
        }

        // ===================
        // Fill Leading Zeroes
        // ===================
        public String FillLeadingZeroes(Int32 number, Int32 length)
        {
            var result = number.ToString();
            var pad = length - result.Length;
            while (pad > 0)
            {
                result = '0' + result;
                pad--;
            }

            return result;
        }

        // ==============
        // Add Item Price
        // ==============
        [Authorize, HttpPost, Route("api/itemPrice/add")]
        public HttpResponseMessage AddItemPrice()
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
                                    && d.SysForm.FormName.Equals("ItemPriceList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultIPNumber = "0000000001";
                            var lastItemPrice = from d in db.TrnArticlePrices.OrderByDescending(d => d.Id)
                                                where d.BranchId == currentBranchId
                                                select d;

                            if (lastItemPrice.Any())
                            {
                                var IPNumber = Convert.ToInt32(lastItemPrice.FirstOrDefault().IPNumber) + 0000000001;
                                defaultIPNumber = FillLeadingZeroes(IPNumber, 10);
                            }

                            var users = from d in db.MstUsers.OrderBy(d => d.FullName)
                                        where d.IsLocked == true
                                        select d;

                            if (users.Any())
                            {
                                Data.TrnArticlePrice newItemPrice = new Data.TrnArticlePrice
                                {
                                    BranchId = currentBranchId,
                                    IPNumber = defaultIPNumber,
                                    IPDate = DateTime.Today,
                                    ManualIPNumber = "NA",
                                    Particulars = "NA",
                                    PreparedById = currentUserId,
                                    CheckedById = currentUserId,
                                    ApprovedById = currentUserId,
                                    IsLocked = false,
                                    CreatedById = currentUserId,
                                    CreatedDateTime = DateTime.Now,
                                    UpdatedById = currentUserId,
                                    UpdatedDateTime = DateTime.Now
                                };

                                db.TrnArticlePrices.InsertOnSubmit(newItemPrice);
                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK, newItemPrice.Id);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No user found. Please setup more users for all transactions.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add item price.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this item price page.");
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
        // Lock Item Price
        // ===============
        [Authorize, HttpPut, Route("api/itemPrice/lock/{id}")]
        public HttpResponseMessage LockItemPrice(Entities.TrnArticlePrice objItemPrice, String id)
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
                                    && d.SysForm.FormName.Equals("ItemPriceDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var itemPrice = from d in db.TrnArticlePrices
                                            where d.Id == Convert.ToInt32(id)
                                            select d;

                            if (itemPrice.Any())
                            {
                                if (!itemPrice.FirstOrDefault().IsLocked)
                                {
                                    var lockItemPrice = itemPrice.FirstOrDefault();
                                    lockItemPrice.IPDate = Convert.ToDateTime(objItemPrice.IPDate);
                                    lockItemPrice.ManualIPNumber = objItemPrice.ManualIPNumber;
                                    lockItemPrice.Particulars = objItemPrice.Particulars;
                                    lockItemPrice.CheckedById = objItemPrice.CheckedById;
                                    lockItemPrice.ApprovedById = objItemPrice.ApprovedById;
                                    lockItemPrice.IsLocked = true;
                                    lockItemPrice.UpdatedById = currentUserId;
                                    lockItemPrice.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These item price details are already locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These item price details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock item price.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this item price page.");
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

        // =================
        // Unlock Item Price
        // =================
        [Authorize, HttpPut, Route("api/itemPrice/unlock/{id}")]
        public HttpResponseMessage UnlockItemPrice(String id)
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
                                    && d.SysForm.FormName.Equals("ItemPriceDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var itemPrice = from d in db.TrnArticlePrices
                                            where d.Id == Convert.ToInt32(id)
                                            select d;

                            if (itemPrice.Any())
                            {
                                if (itemPrice.FirstOrDefault().IsLocked)
                                {
                                    var unlockItemPrice = itemPrice.FirstOrDefault();
                                    unlockItemPrice.IsLocked = false;
                                    unlockItemPrice.UpdatedById = currentUserId;
                                    unlockItemPrice.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These item price details are already unlocked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These item price details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock item price.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this item price page.");
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

        // =================
        // Delete Item Price
        // =================
        [Authorize, HttpDelete, Route("api/itemPrice/delete/{id}")]
        public HttpResponseMessage DeleteItemPrice(String id)
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
                                    && d.SysForm.FormName.Equals("ItemPriceList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var itemPrice = from d in db.TrnArticlePrices
                                            where d.Id == Convert.ToInt32(id)
                                            select d;

                            if (itemPrice.Any())
                            {
                                if (!itemPrice.FirstOrDefault().IsLocked)
                                {
                                    db.TrnArticlePrices.DeleteOnSubmit(itemPrice.First());
                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete item price if the current item price record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These item price details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete item price.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this item price page.");
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
