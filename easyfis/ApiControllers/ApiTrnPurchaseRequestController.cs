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
    public class ApiTrnPurchaseRequestController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

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

        // =====================
        // List Purchase Request
        // =====================
        [Authorize, HttpGet, Route("api/purchaseRequest/list/{startDate}/{endDate}")]
        public List<Entities.TrnPurchaseRequest> ListPurchaseRequest(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var purchaseRequests = from d in db.TrnPurchaseRequests.OrderByDescending(d => d.Id)
                                   where d.BranchId == branchId
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
                                       IsLocked = d.IsLocked,
                                       CreatedBy = d.MstUser2.FullName,
                                       CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                       UpdatedBy = d.MstUser5.FullName,
                                       UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                   };

            return purchaseRequests.ToList();
        }

        // =======================
        // Detail Purchase Request
        // =======================
        [Authorize, HttpGet, Route("api/purchaseRequest/detail/{id}")]
        public Entities.TrnPurchaseRequest DetailPurchaseRequest(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var purchaseRequest = from d in db.TrnPurchaseRequests
                                  where d.BranchId == branchId
                                  && d.Id == Convert.ToInt32(id)
                                  select new Entities.TrnPurchaseRequest
                                  {
                                      Id = d.Id,
                                      BranchId = d.BranchId,
                                      PRNumber = d.PRNumber,
                                      PRDate = d.PRDate.ToShortDateString(),
                                      SupplierId = d.SupplierId,
                                      TermId = d.TermId,
                                      ManualPRNumber = d.ManualPRNumber,
                                      DateNeeded = d.DateNeeded.ToShortDateString(),
                                      Remarks = d.Remarks,
                                      IsClose = d.IsClose,
                                      RequestedById = d.RequestedById,
                                      PreparedById = d.PreparedById,
                                      CheckedById = d.CheckedById,
                                      ApprovedById = d.ApprovedById,
                                      Status = d.Status,
                                      IsLocked = d.IsLocked,
                                      CreatedBy = d.MstUser2.FullName,
                                      CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                      UpdatedBy = d.MstUser5.FullName,
                                      UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                  };

            if (purchaseRequest.Any())
            {
                return purchaseRequest.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/purchaseRequest/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListPurchaseRequestBranch()
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
        [Authorize, HttpGet, Route("api/purchaseRequest/dropdown/list/supplier")]
        public List<Entities.MstArticle> DropdownListPurchaseRequestSupplier()
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
        [Authorize, HttpGet, Route("api/purchaseRequest/dropdown/list/term")]
        public List<Entities.MstTerm> DropdownListPurchaseRequestTerm()
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
        [Authorize, HttpGet, Route("api/purchaseRequest/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListPurchaseRequestUsers()
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

        // ==============================
        // Dropdown List - Status (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/purchaseRequest/dropdown/list/status")]
        public List<Entities.MstStatus> DropdownListPurchaseRequestStatus()
        {
            var statuses = from d in db.MstStatus.OrderBy(d => d.Status)
                           where d.IsLocked == true
                           && d.Category.Equals("PR")
                           select new Entities.MstStatus
                           {
                               Id = d.Id,
                               Status = d.Status
                           };

            return statuses.ToList();
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

        // ====================
        // Add Purchase Request
        // ====================
        [Authorize, HttpPost, Route("api/purchaseRequest/add")]
        public HttpResponseMessage AddPurchaseRequest()
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
                                    && d.SysForm.FormName.Equals("PurchaseRequestList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultPRNumber = "0000000001";
                            var lastPurchaseRequest = from d in db.TrnPurchaseRequests.OrderByDescending(d => d.Id)
                                                      where d.BranchId == currentBranchId
                                                      select d;

                            if (lastPurchaseRequest.Any())
                            {
                                var PRNumber = Convert.ToInt32(lastPurchaseRequest.FirstOrDefault().PRNumber) + 0000000001;
                                defaultPRNumber = FillLeadingZeroes(PRNumber, 10);
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
                                        Data.TrnPurchaseRequest newPurchaseRequest = new Data.TrnPurchaseRequest
                                        {
                                            BranchId = currentBranchId,
                                            PRNumber = defaultPRNumber,
                                            PRDate = DateTime.Today,
                                            SupplierId = suppliers.FirstOrDefault().Id,
                                            TermId = terms.FirstOrDefault().Id,
                                            ManualPRNumber = "NA",
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

                                        db.TrnPurchaseRequests.InsertOnSubmit(newPurchaseRequest);
                                        db.SubmitChanges();

                                        String newObject = at.GetObjectString(newPurchaseRequest);
                                        at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                        return Request.CreateResponse(HttpStatusCode.OK, newPurchaseRequest.Id);
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
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add purchase request.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this purchase request page.");
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
        // Lock Purchase Request
        // =====================
        [Authorize, HttpPut, Route("api/purchaseRequest/lock/{id}")]
        public HttpResponseMessage LockPurchaseRequest(Entities.TrnPurchaseRequest objPurchaseRequest, String id)
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
                                    && d.SysForm.FormName.Equals("PurchaseRequestDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var purchaseRequest = from d in db.TrnPurchaseRequests
                                                  where d.Id == Convert.ToInt32(id)
                                                  select d;

                            if (purchaseRequest.Any())
                            {
                                if (!purchaseRequest.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = at.GetObjectString(purchaseRequest.FirstOrDefault());

                                    var lockPurchaseRequest = purchaseRequest.FirstOrDefault();
                                    lockPurchaseRequest.PRDate = Convert.ToDateTime(objPurchaseRequest.PRDate);
                                    lockPurchaseRequest.SupplierId = objPurchaseRequest.SupplierId;
                                    lockPurchaseRequest.TermId = objPurchaseRequest.TermId;
                                    lockPurchaseRequest.ManualPRNumber = objPurchaseRequest.ManualPRNumber;
                                    lockPurchaseRequest.DateNeeded = Convert.ToDateTime(objPurchaseRequest.DateNeeded);
                                    lockPurchaseRequest.Remarks = objPurchaseRequest.Remarks;
                                    lockPurchaseRequest.IsClose = objPurchaseRequest.IsClose;
                                    lockPurchaseRequest.RequestedById = objPurchaseRequest.RequestedById;
                                    lockPurchaseRequest.CheckedById = objPurchaseRequest.CheckedById;
                                    lockPurchaseRequest.ApprovedById = objPurchaseRequest.ApprovedById;
                                    lockPurchaseRequest.Status = objPurchaseRequest.Status;
                                    lockPurchaseRequest.IsLocked = true;
                                    lockPurchaseRequest.UpdatedById = currentUserId;
                                    lockPurchaseRequest.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    if (purchaseRequest.FirstOrDefault().TrnPurchaseRequestItems.Any())
                                    {
                                        foreach (var purchaseRequestItem in purchaseRequest.FirstOrDefault().TrnPurchaseRequestItems.ToList())
                                        {
                                            var item = from d in db.MstArticles
                                                       where d.Id == purchaseRequestItem.ItemId
                                                       select d;

                                            if (item.Any())
                                            {
                                                var updateItem = item.FirstOrDefault();
                                                updateItem.Cost = purchaseRequestItem.Cost;
                                            }
                                        }

                                        db.SubmitChanges();
                                    }

                                    String newObject = at.GetObjectString(purchaseRequest.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These purchase request details are already locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These purchase request details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock purchase request.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this purchase request page.");
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

        // =======================
        // Unlock Purchase Request
        // =======================
        [Authorize, HttpPut, Route("api/purchaseRequest/unlock/{id}")]
        public HttpResponseMessage UnlockPurchaseRequest(String id)
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
                                    && d.SysForm.FormName.Equals("PurchaseRequestDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var purchaseRequest = from d in db.TrnPurchaseRequests
                                                  where d.Id == Convert.ToInt32(id)
                                                  select d;

                            if (purchaseRequest.Any())
                            {
                                if (purchaseRequest.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = at.GetObjectString(purchaseRequest.FirstOrDefault());

                                    var unlockPurchaseRequest = purchaseRequest.FirstOrDefault();
                                    unlockPurchaseRequest.IsLocked = false;
                                    unlockPurchaseRequest.UpdatedById = currentUserId;
                                    unlockPurchaseRequest.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    String newObject = at.GetObjectString(purchaseRequest.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These purchase request details are already unlocked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These purchase request details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock purchase request.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this purchase request page.");
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

        // =======================
        // Delete Purchase Request
        // =======================
        [Authorize, HttpDelete, Route("api/purchaseRequest/delete/{id}")]
        public HttpResponseMessage DeletePurchaseRequest(String id)
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
                                    && d.SysForm.FormName.Equals("PurchaseRequestList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var purchaseRequest = from d in db.TrnPurchaseRequests
                                                  where d.Id == Convert.ToInt32(id)
                                                  select d;

                            if (purchaseRequest.Any())
                            {
                                if (!purchaseRequest.FirstOrDefault().IsLocked)
                                {
                                    db.TrnPurchaseRequests.DeleteOnSubmit(purchaseRequest.First());

                                    String oldObject = at.GetObjectString(purchaseRequest.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete purchase request if the current purchase request record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These purchase request details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete purchase request.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this purchase request page.");
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
