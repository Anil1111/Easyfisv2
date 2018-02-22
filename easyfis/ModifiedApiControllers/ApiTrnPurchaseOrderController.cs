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
    public class ApiTrnPurchaseOrderController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =========================
        // Get Purchase Order Amount
        // =========================
        public Decimal GetPurchaseOrderAmount(Int32 POId)
        {
            var purchaseOrderItems = from d in db.TrnPurchaseOrderItems
                                     where d.POId == POId
                                     select d;

            if (purchaseOrderItems.Any())
            {
                return purchaseOrderItems.Sum(d => d.Amount);
            }
            else
            {
                return 0;
            }
        }

        // ===================
        // List Purchase Order
        // ===================
        [Authorize, HttpGet, Route("api/purchaseOrder/list/{startDate}/{endDate}")]
        public List<Entities.TrnPurchaseOrder> ListPurchaseOrder(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var purchaseOrders = from d in db.TrnPurchaseOrders.OrderByDescending(d => d.Id)
                                 where d.BranchId == branchId
                                 && d.PODate >= Convert.ToDateTime(startDate)
                                 && d.PODate <= Convert.ToDateTime(endDate)
                                 select new Entities.TrnPurchaseOrder
                                 {
                                     Id = d.Id,
                                     PONumber = d.PONumber,
                                     PODate = d.PODate.ToShortDateString(),
                                     ManualPONumber = d.ManualPONumber,
                                     Supplier = d.MstArticle.Article,
                                     Remarks = d.Remarks,
                                     Amount = GetPurchaseOrderAmount(d.Id),
                                     IsClose = d.IsClose,
                                     IsLocked = d.IsLocked,
                                     CreatedBy = d.MstUser2.FullName,
                                     CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                     UpdatedBy = d.MstUser5.FullName,
                                     UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                 };

            return purchaseOrders.ToList();
        }

        // =====================
        // Detail Purchase Order
        // =====================
        [Authorize, HttpGet, Route("api/purchaseOrder/detail/{id}")]
        public Entities.TrnPurchaseOrder DetailPurchaseOrder(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var purchaseOrder = from d in db.TrnPurchaseOrders
                                where d.BranchId == branchId
                                && d.Id == Convert.ToInt32(id)
                                select new Entities.TrnPurchaseOrder
                                {
                                    Id = d.Id,
                                    BranchId = d.BranchId,
                                    PONumber = d.PONumber,
                                    PODate = d.PODate.ToShortDateString(),
                                    SupplierId = d.SupplierId,
                                    TermId = d.TermId,
                                    ManualRequestNumber = d.ManualRequestNumber,
                                    ManualPONumber = d.ManualPONumber,
                                    DateNeeded = d.DateNeeded.ToShortDateString(),
                                    Remarks = d.Remarks,
                                    IsClose = d.IsClose,
                                    RequestedById = d.RequestedById,
                                    PreparedById = d.PreparedById,
                                    CheckedById = d.CheckedById,
                                    ApprovedById = d.ApprovedById,
                                    IsLocked = d.IsLocked,
                                    CreatedBy = d.MstUser2.FullName,
                                    CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                    UpdatedBy = d.MstUser5.FullName,
                                    UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                };

            if (purchaseOrder.Any())
            {
                return purchaseOrder.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/purchaseOrder/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListPurchaseOrderBranch()
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // ================================
        // Dropdown List - Supplier (Field)
        // ================================
        [Authorize, HttpGet, Route("api/purchaseOrder/dropdown/list/supplier")]
        public List<Entities.MstArticle> DropdownListPurchaseOrderSupplier()
        {
            var suppliers = from d in db.MstArticles.OrderBy(d => d.Article)
                            where d.ArticleTypeId == 3
                            && d.IsLocked == true
                            select new Entities.MstArticle
                            {
                                Id = d.Id,
                                Article = d.Article,
                                TermId = d.TermId
                            };

            return suppliers.ToList();
        }

        // ============================
        // Dropdown List - Term (Field)
        // ============================
        [Authorize, HttpGet, Route("api/purchaseOrder/dropdown/list/term")]
        public List<Entities.MstTerm> DropdownListPurchaseOrderTerm()
        {
            var terms = from d in db.MstTerms.OrderBy(d => d.Term)
                        where d.IsLocked == true
                        select new Entities.MstTerm
                        {
                            Id = d.Id,
                            Term = d.Term
                        };

            return terms.ToList();
        }

        // =============================
        // Dropdown List - Users (Field)
        // =============================
        [Authorize, HttpGet, Route("api/purchaseOrder/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListPurchaseOrderUsers()
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

        // ==================
        // Add Purchase Order
        // ==================
        [Authorize, HttpPost, Route("api/purchaseOrder/add")]
        public HttpResponseMessage AddPurchaseOrder()
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
                                    && d.SysForm.FormName.Equals("PurchaseOrderList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultPONumber = "0000000001";
                            var lastPurchaseOrder = from d in db.TrnPurchaseOrders.OrderByDescending(d => d.Id)
                                                    where d.BranchId == currentBranchId
                                                    select d;

                            if (lastPurchaseOrder.Any())
                            {
                                var PONumber = Convert.ToInt32(lastPurchaseOrder.FirstOrDefault().PONumber) + 0000000001;
                                defaultPONumber = FillLeadingZeroes(PONumber, 10);
                            }

                            var suppliers = from d in db.MstArticles.OrderBy(d => d.Article)
                                            where d.ArticleTypeId == 3
                                            && d.IsLocked == true
                                            select d;

                            if (suppliers.Any())
                            {
                                var terms = from d in db.MstTerms.OrderBy(d => d.Term)
                                            where d.IsLocked == true
                                            select d;

                                if (terms.Any())
                                {
                                    var users = from d in db.MstUsers.OrderBy(d => d.FullName)
                                                where d.IsLocked == true
                                                select d;

                                    if (users.Any())
                                    {
                                        Data.TrnPurchaseOrder newPurchaseOrder = new Data.TrnPurchaseOrder
                                        {
                                            BranchId = currentBranchId,
                                            PONumber = defaultPONumber,
                                            PODate = DateTime.Today,
                                            SupplierId = suppliers.FirstOrDefault().Id,
                                            TermId = terms.FirstOrDefault().Id,
                                            ManualRequestNumber = "NA",
                                            ManualPONumber = "NA",
                                            DateNeeded = DateTime.Today,
                                            Remarks = "NA",
                                            IsClose = false,
                                            RequestedById = currentUserId,
                                            PreparedById = currentUserId,
                                            CheckedById = currentUserId,
                                            ApprovedById = currentUserId,
                                            IsLocked = false,
                                            CreatedById = currentUserId,
                                            CreatedDateTime = DateTime.Now,
                                            UpdatedById = currentUserId,
                                            UpdatedDateTime = DateTime.Now
                                        };

                                        db.TrnPurchaseOrders.InsertOnSubmit(newPurchaseOrder);
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK, newPurchaseOrder.Id);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "No user found. Please setup more users for all transactions.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.NotFound, "No term found. Please setup more terms for all transactions.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No supplier found. Please setup more suppliers for all transactions.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add purchase order.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this purchase order page.");
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

        // ===================
        // Lock Purchase Order
        // ===================
        [Authorize, HttpPut, Route("api/purchaseOrder/lock/{id}")]
        public HttpResponseMessage LockPurchaseOrder(Entities.TrnPurchaseOrder objPurchaseOrder, String id)
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
                                    && d.SysForm.FormName.Equals("PurchaseOrderDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var purchaseOrder = from d in db.TrnPurchaseOrders
                                                where d.Id == Convert.ToInt32(id)
                                                select d;

                            if (purchaseOrder.Any())
                            {
                                if (!purchaseOrder.FirstOrDefault().IsLocked)
                                {
                                    var lockPurchaseOrder = purchaseOrder.FirstOrDefault();
                                    lockPurchaseOrder.PODate = Convert.ToDateTime(objPurchaseOrder.PODate);
                                    lockPurchaseOrder.SupplierId = objPurchaseOrder.SupplierId;
                                    lockPurchaseOrder.TermId = objPurchaseOrder.TermId;
                                    lockPurchaseOrder.ManualRequestNumber = objPurchaseOrder.ManualRequestNumber;
                                    lockPurchaseOrder.ManualPONumber = objPurchaseOrder.ManualPONumber;
                                    lockPurchaseOrder.DateNeeded = Convert.ToDateTime(objPurchaseOrder.DateNeeded);
                                    lockPurchaseOrder.Remarks = objPurchaseOrder.Remarks;
                                    lockPurchaseOrder.IsClose = objPurchaseOrder.IsClose;
                                    lockPurchaseOrder.RequestedById = objPurchaseOrder.RequestedById;
                                    lockPurchaseOrder.CheckedById = objPurchaseOrder.CheckedById;
                                    lockPurchaseOrder.ApprovedById = objPurchaseOrder.ApprovedById;
                                    lockPurchaseOrder.IsLocked = true;
                                    lockPurchaseOrder.UpdatedById = currentUserId;
                                    lockPurchaseOrder.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    if (purchaseOrder.FirstOrDefault().TrnPurchaseOrderItems.Any())
                                    {
                                        foreach (var purchaseOrderItem in purchaseOrder.FirstOrDefault().TrnPurchaseOrderItems.ToList())
                                        {
                                            var item = from d in db.MstArticles
                                                       where d.Id == purchaseOrderItem.ItemId
                                                       select d;

                                            if (item.Any())
                                            {
                                                var updateItem = item.FirstOrDefault();
                                                updateItem.Cost = purchaseOrderItem.Cost;
                                            }
                                        }

                                        db.SubmitChanges();
                                    }

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These purchase order details are already locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These purchase order details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock purchase order.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this purchase order page.");
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

        // =====================
        // Unlock Purchase Order
        // =====================
        [Authorize, HttpPut, Route("api/purchaseOrder/unlock/{id}")]
        public HttpResponseMessage UnlockPurchaseOrder(String id)
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
                                    && d.SysForm.FormName.Equals("PurchaseOrderDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var purchaseOrder = from d in db.TrnPurchaseOrders
                                                where d.Id == Convert.ToInt32(id)
                                                select d;

                            if (purchaseOrder.Any())
                            {
                                if (purchaseOrder.FirstOrDefault().IsLocked)
                                {
                                    var unlockPurchaseOrder = purchaseOrder.FirstOrDefault();
                                    unlockPurchaseOrder.IsLocked = false;
                                    unlockPurchaseOrder.UpdatedById = currentUserId;
                                    unlockPurchaseOrder.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These purchase order details are already unlocked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These purchase order details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock purchase order.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this purchase order page.");
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

        // =====================
        // Delete Purchase Order
        // =====================
        [Authorize, HttpDelete, Route("api/purchaseOrder/delete/{id}")]
        public HttpResponseMessage DeletePurchaseOrder(String id)
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
                                    && d.SysForm.FormName.Equals("PurchaseOrderList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var purchaseOrder = from d in db.TrnPurchaseOrders
                                                where d.Id == Convert.ToInt32(id)
                                                select d;

                            if (purchaseOrder.Any())
                            {
                                if (!purchaseOrder.FirstOrDefault().IsLocked)
                                {
                                    db.TrnPurchaseOrders.DeleteOnSubmit(purchaseOrder.First());
                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete purchase order if the current purchase order record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These purchase order details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete purchase order.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this purchase order page.");
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

        // ==================================
        // Dropdown List - PR Branch (Filter)
        // ==================================
        [Authorize, HttpGet, Route("api/purchaseOrder/purchaseRequest/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListPurchaseOrderPurchaseRequestBranch()
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // ===========================
        // Get Purchase Request Amount
        // ===========================
        public Decimal GetPurchaseRequestAmount(Int32 PRId)
        {
            var purchaseRequestItems = from d in db.TrnPurchaseRequestItems
                                       where d.PRId == PRId
                                       select d;

            if (purchaseRequestItems.Any())
            {
                return purchaseRequestItems.Sum(d => d.Amount);
            }
            else
            {
                return 0;
            }
        }

        // ==============================================
        // List Purchase Request (Purchase Request Query)
        // ==============================================
        [Authorize, HttpGet, Route("api/purchaseOrder/purchaseRequest/list/{startDate}/{endDate}/{branchId}")]
        public List<Entities.TrnPurchaseRequest> ListPurchaseOrderPurchaseRequest(String startDate, String endDate, String branchId)
        {
            var purchaseRequests = from d in db.TrnPurchaseRequests.OrderByDescending(d => d.Id)
                                   where d.BranchId == Convert.ToInt32(branchId)
                                   && d.PRDate >= Convert.ToDateTime(startDate)
                                   && d.PRDate <= Convert.ToDateTime(endDate)
                                   select new Entities.TrnPurchaseRequest
                                   {
                                       Id = d.Id,
                                       PRNumber = d.PRNumber,
                                       PRDate = d.PRDate.ToShortDateString(),
                                       ManualPRNumber = d.ManualPRNumber,
                                       Supplier = d.MstArticle.Article,
                                       Remarks = d.Remarks,
                                       Amount = GetPurchaseRequestAmount(d.Id),
                                       IsClose = d.IsClose,
                                   };

            return purchaseRequests.ToList();
        }

        // =====================
        // Load Purchase Request
        // =====================
        [Authorize, HttpPost, Route("api/purchaseOrder/load/purchaseRequest/{POId}/{PRId}")]
        public HttpResponseMessage LoadPurchaseOrderPurchaseRequest(String POId, String PRId)
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
                                    && d.SysForm.FormName.Equals("PurchaseOrderDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var purchaseOrder = from d in db.TrnPurchaseOrders
                                                where d.Id == Convert.ToInt32(POId)
                                                select d;

                            if (purchaseOrder.Any())
                            {
                                var purchaseRequest = from d in db.TrnPurchaseRequests
                                                      where d.Id == Convert.ToInt32(PRId)
                                                      && d.IsLocked == true
                                                      select d;

                                if (purchaseRequest.Any())
                                {
                                    var updatePurchaseOrder = purchaseOrder.FirstOrDefault();
                                    updatePurchaseOrder.PODate = purchaseRequest.FirstOrDefault().PRDate;
                                    updatePurchaseOrder.SupplierId = purchaseRequest.FirstOrDefault().SupplierId;
                                    updatePurchaseOrder.TermId = purchaseRequest.FirstOrDefault().TermId;
                                    updatePurchaseOrder.ManualRequestNumber = purchaseRequest.FirstOrDefault().ManualPRNumber;
                                    updatePurchaseOrder.ManualPONumber = purchaseRequest.FirstOrDefault().ManualPRNumber;
                                    updatePurchaseOrder.DateNeeded = purchaseRequest.FirstOrDefault().DateNeeded;
                                    updatePurchaseOrder.Remarks = purchaseRequest.FirstOrDefault().Remarks;
                                    updatePurchaseOrder.IsClose = purchaseRequest.FirstOrDefault().IsClose;
                                    updatePurchaseOrder.RequestedById = purchaseRequest.FirstOrDefault().RequestedById;
                                    updatePurchaseOrder.CheckedById = purchaseRequest.FirstOrDefault().CheckedById;
                                    updatePurchaseOrder.ApprovedById = purchaseRequest.FirstOrDefault().ApprovedById;
                                    updatePurchaseOrder.UpdatedById = currentUserId;
                                    updatePurchaseOrder.UpdatedDateTime = DateTime.Now;
                                    db.SubmitChanges();

                                    var purchaseOrderItems = from d in db.TrnPurchaseOrderItems
                                                             where d.POId == purchaseOrder.FirstOrDefault().Id
                                                             select d;

                                    if (purchaseOrderItems.Any())
                                    {
                                        db.TrnPurchaseOrderItems.DeleteAllOnSubmit(purchaseOrderItems);
                                        db.SubmitChanges();
                                    }

                                    var purchaseRequestItems = from d in db.TrnPurchaseRequestItems
                                                               where d.PRId == purchaseRequest.FirstOrDefault().Id
                                                               select d;

                                    if (purchaseRequestItems.Any())
                                    {
                                        foreach (var purchaseRequestItem in purchaseRequestItems)
                                        {
                                            Data.TrnPurchaseOrderItem newPurchaseOrderItem = new Data.TrnPurchaseOrderItem()
                                            {
                                                POId = purchaseOrder.FirstOrDefault().Id,
                                                ItemId = purchaseRequestItem.ItemId,
                                                Particulars = purchaseRequestItem.Particulars,
                                                UnitId = purchaseRequestItem.UnitId,
                                                Quantity = purchaseRequestItem.Quantity,
                                                Cost = purchaseRequestItem.Cost,
                                                Amount = purchaseRequestItem.Amount,
                                                BaseUnitId = purchaseRequestItem.BaseUnitId,
                                                BaseQuantity = purchaseRequestItem.BaseQuantity,
                                                BaseCost = purchaseRequestItem.BaseCost,
                                            };
                                            db.TrnPurchaseOrderItems.InsertOnSubmit(newPurchaseOrderItem);
                                        }

                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. There are no purchase request items found in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. There are no purchase requests found in the server.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These purchase order details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to load purchase request.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this purchase order page.");
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
