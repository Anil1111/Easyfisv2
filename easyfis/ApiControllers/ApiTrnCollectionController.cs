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
    public class ApiTrnCollectionController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========
        // Business
        // ========
        private Business.AccountsReceivable accountsReceivable = new Business.AccountsReceivable();
        private Business.Journal journal = new Business.Journal();
        private Business.AuditTrail auditTrail = new Business.AuditTrail();

        // ===============
        // List Collection
        // ===============
        [Authorize, HttpGet, Route("api/collection/list/{startDate}/{endDate}")]
        public List<Entities.TrnCollection> ListCollection(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var collections = from d in db.TrnCollections.OrderByDescending(d => d.Id)
                              where d.BranchId == branchId
                              && d.ORDate >= Convert.ToDateTime(startDate)
                              && d.ORDate <= Convert.ToDateTime(endDate)
                              select new Entities.TrnCollection
                              {
                                  Id = d.Id,
                                  ORNumber = d.ORNumber,
                                  ORDate = d.ORDate.ToShortDateString(),
                                  ManualORNumber = d.ManualORNumber,
                                  Customer = d.MstArticle.Article,
                                  Particulars = d.Particulars,
                                  Amount = d.TrnCollectionLines.Any() ? d.TrnCollectionLines.Sum(a => a.Amount) : 0,
                                  IsLocked = d.IsLocked,
                                  IsCancelled = d.IsCancelled,
                                  CreatedById = d.CreatedById,
                                  CreatedBy = d.MstUser2.FullName,
                                  CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                  UpdatedById = d.UpdatedById,
                                  UpdatedBy = d.MstUser4.FullName,
                                  UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                              };

            return collections.ToList();
        }

        // =================
        // Detail Collection
        // =================
        [Authorize, HttpGet, Route("api/collection/detail/{id}")]
        public Entities.TrnCollection DetailCollection(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var collection = from d in db.TrnCollections
                             where d.BranchId == branchId
                             && d.Id == Convert.ToInt32(id)
                             select new Entities.TrnCollection
                             {
                                 Id = d.Id,
                                 BranchId = d.BranchId,
                                 ORNumber = d.ORNumber,
                                 ORDate = d.ORDate.ToShortDateString(),
                                 ManualORNumber = d.ManualORNumber,
                                 CustomerId = d.CustomerId,
                                 Particulars = d.Particulars,
                                 PreparedById = d.PreparedById,
                                 CheckedById = d.CheckedById,
                                 ApprovedById = d.ApprovedById,
                                 Status = d.Status,
                                 IsCancelled = d.IsCancelled,
                                 IsLocked = d.IsLocked,
                                 CreatedBy = d.MstUser2.FullName,
                                 CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                 UpdatedBy = d.MstUser4.FullName,
                                 UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                             };

            if (collection.Any())
            {
                return collection.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/collection/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListCollectionBranch()
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
        // Dropdown List - Customer (Field)
        // ================================
        [Authorize, HttpGet, Route("api/collection/dropdown/list/customer")]
        public List<Entities.MstArticle> DropdownListCollectionCustomer()
        {
            var customers = from d in db.MstArticles.OrderBy(d => d.Article)
                            where d.ArticleTypeId == 2
                            && d.IsLocked == true
                            select new Entities.MstArticle
                            {
                                Id = d.Id,
                                Article = d.Article
                            };

            return customers.ToList();
        }

        // ============================
        // Dropdown List - User (Field)
        // ============================
        [Authorize, HttpGet, Route("api/collection/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListCollectionUsers()
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
        [Authorize, HttpGet, Route("api/collection/dropdown/list/status")]
        public List<Entities.MstStatus> DropdownListCollectionStatus()
        {
            var statuses = from d in db.MstStatus.OrderBy(d => d.Status)
                           where d.IsLocked == true
                           && d.Category.Equals("OR")
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

        // ==============
        // Add Collection
        // ==============
        [Authorize, HttpPost, Route("api/collection/add")]
        public HttpResponseMessage AddCollection()
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
                                    && d.SysForm.FormName.Equals("CollectionList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultORNumber = "0000000001";
                            var lastCollection = from d in db.TrnCollections.OrderByDescending(d => d.Id)
                                                 where d.BranchId == currentBranchId
                                                 select d;

                            if (lastCollection.Any())
                            {
                                var ORNumber = Convert.ToInt32(lastCollection.FirstOrDefault().ORNumber) + 0000000001;
                                defaultORNumber = FillLeadingZeroes(ORNumber, 10);
                            }

                            var customers = from d in db.MstArticles.OrderBy(d => d.Article)
                                            where d.ArticleTypeId == 2
                                            && d.IsLocked == true
                                            select d;

                            if (customers.Any())
                            {
                                var users = from d in db.MstUsers.OrderBy(d => d.FullName)
                                            where d.IsLocked == true
                                            select d;

                                if (users.Any())
                                {
                                    Data.TrnCollection newCollection = new Data.TrnCollection
                                    {
                                        BranchId = currentBranchId,
                                        ORNumber = defaultORNumber,
                                        ORDate = DateTime.Today,
                                        ManualORNumber = "NA",
                                        CustomerId = customers.FirstOrDefault().Id,
                                        Particulars = "NA",
                                        PreparedById = currentUserId,
                                        CheckedById = currentUserId,
                                        ApprovedById = currentUserId,
                                        Status = null,
                                        IsCancelled = false,
                                        IsLocked = false,
                                        CreatedById = currentUserId,
                                        CreatedDateTime = DateTime.Now,
                                        UpdatedById = currentUserId,
                                        UpdatedDateTime = DateTime.Now
                                    };

                                    db.TrnCollections.InsertOnSubmit(newCollection);
                                    db.SubmitChanges();

                                    String newObject = auditTrail.GetObjectString(newCollection);
                                    auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK, newCollection.Id);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.NotFound, "No user found. Please setup more users for all transactions.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No customer found. Please setup more customers for all transactions.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add collection.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this collection page.");
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
        // Save Collection
        // ===============
        [Authorize, HttpPut, Route("api/collection/save/{id}")]
        public HttpResponseMessage SaveCollection(Entities.TrnCollection objCollection, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var collection = from d in db.TrnCollections where d.Id == Convert.ToInt32(id) select d;
                    if (collection.Any())
                    {
                        if (!collection.FirstOrDefault().IsLocked)
                        {
                            String oldObject = auditTrail.GetObjectString(collection.FirstOrDefault());

                            var saveCollection = collection.FirstOrDefault();
                            saveCollection.ORDate = Convert.ToDateTime(objCollection.ORDate);
                            saveCollection.ManualORNumber = objCollection.ManualORNumber;
                            saveCollection.CustomerId = objCollection.CustomerId;
                            saveCollection.Particulars = objCollection.Particulars;
                            saveCollection.CheckedById = objCollection.CheckedById;
                            saveCollection.ApprovedById = objCollection.ApprovedById;
                            saveCollection.Status = objCollection.Status;
                            saveCollection.UpdatedById = currentUserId;
                            saveCollection.UpdatedDateTime = DateTime.Now;
                            db.SubmitChanges();

                            String newObject = auditTrail.GetObjectString(collection.FirstOrDefault());
                            auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Saving Error. These collection details are already locked.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These collection details are not found in the server.");
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
        // Lock Collection
        // ===============
        [Authorize, HttpPut, Route("api/collection/lock/{id}")]
        public HttpResponseMessage LockCollection(Entities.TrnCollection objCollection, String id)
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
                                    && d.SysForm.FormName.Equals("CollectionDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var collection = from d in db.TrnCollections
                                             where d.Id == Convert.ToInt32(id)
                                             select d;

                            if (collection.Any())
                            {
                                int countInvalidSI = 0;
                                var invalidSIs = from d in db.TrnCollectionLines
                                                 where d.ORId == collection.FirstOrDefault().Id
                                                 && d.TrnSalesInvoice.IsLocked == false
                                                 && d.TrnSalesInvoice.IsCancelled == true
                                                 select d;

                                if (invalidSIs.Any())
                                {
                                    countInvalidSI = invalidSIs.Count();
                                }

                                if (!collection.FirstOrDefault().IsLocked && countInvalidSI == 0)
                                {
                                    String oldObject = auditTrail.GetObjectString(collection.FirstOrDefault());

                                    var lockCollection = collection.FirstOrDefault();
                                    lockCollection.ORDate = Convert.ToDateTime(objCollection.ORDate);
                                    lockCollection.ManualORNumber = objCollection.ManualORNumber;
                                    lockCollection.CustomerId = objCollection.CustomerId;
                                    lockCollection.Particulars = objCollection.Particulars;
                                    lockCollection.CheckedById = objCollection.CheckedById;
                                    lockCollection.ApprovedById = objCollection.ApprovedById;
                                    lockCollection.Status = objCollection.Status;
                                    lockCollection.IsLocked = true;
                                    lockCollection.UpdatedById = currentUserId;
                                    lockCollection.UpdatedDateTime = DateTime.Now;
                                    db.SubmitChanges();

                                    if (lockCollection.IsLocked)
                                    {
                                        var collectionLines = from d in db.TrnCollectionLines where d.ORId == Convert.ToInt32(id) && d.SIId != null select d;
                                        if (collectionLines.Any())
                                        {
                                            foreach (var collectionLine in collectionLines) { accountsReceivable.UpdateAccountsReceivable(Convert.ToInt32(collectionLine.SIId)); }
                                        }

                                        journal.InsertOfficialReceiptJournal(Convert.ToInt32(id));
                                    }

                                    String newObject = auditTrail.GetObjectString(collection.FirstOrDefault());
                                    auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These collection details are already locked or SI is invalid.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These collection details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock collection.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this collection page.");
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
        // Unlock Collection
        // =================
        [Authorize, HttpPut, Route("api/collection/unlock/{id}")]
        public HttpResponseMessage UnlockCollection(String id)
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
                                    && d.SysForm.FormName.Equals("CollectionDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var collection = from d in db.TrnCollections
                                             where d.Id == Convert.ToInt32(id)
                                             select d;

                            if (collection.Any())
                            {
                                if (!collection.FirstOrDefault().IsCancelled)
                                {
                                    if (collection.FirstOrDefault().IsLocked)
                                    {
                                        String oldObject = auditTrail.GetObjectString(collection.FirstOrDefault());

                                        var unlockCollection = collection.FirstOrDefault();
                                        unlockCollection.IsLocked = false;
                                        unlockCollection.UpdatedById = currentUserId;
                                        unlockCollection.UpdatedDateTime = DateTime.Now;
                                        db.SubmitChanges();

                                        if (!unlockCollection.IsLocked)
                                        {
                                            var collectionLines = from d in db.TrnCollectionLines where d.ORId == Convert.ToInt32(id) && d.SIId != null select d;
                                            if (collectionLines.Any())
                                            {
                                                foreach (var collectionLine in collectionLines) { accountsReceivable.UpdateAccountsReceivable(Convert.ToInt32(collectionLine.SIId)); }
                                            }

                                            journal.DeleteOfficialReceiptJournal(Convert.ToInt32(id));
                                        }

                                        String newObject = auditTrail.GetObjectString(collection.FirstOrDefault());
                                        auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These collection details are already unlocked.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These collection details are already cancelled.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These collection details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock collection.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this collection page.");
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
        // Cancel Collection
        // =================
        [Authorize, HttpPut, Route("api/collection/cancel/{id}")]
        public HttpResponseMessage CancelCollection(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms where d.UserId == currentUserId && d.SysForm.FormName.Equals("CollectionDetail") select d;
                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanCancel)
                        {
                            var collection = from d in db.TrnCollections where d.Id == Convert.ToInt32(id) select d;
                            if (collection.Any())
                            {
                                if (collection.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = auditTrail.GetObjectString(collection.FirstOrDefault());

                                    var cancelCollection = collection.FirstOrDefault();
                                    cancelCollection.IsCancelled = true;
                                    cancelCollection.UpdatedById = currentUserId;
                                    cancelCollection.UpdatedDateTime = DateTime.Now;
                                    db.SubmitChanges();

                                    if (cancelCollection.IsCancelled)
                                    {
                                        var collectionLines = from d in db.TrnCollectionLines where d.ORId == Convert.ToInt32(id) select d;
                                        if (collectionLines.Any())
                                        {
                                            foreach (var collectionLine in collectionLines) { accountsReceivable.UpdateAccountsReceivable(Convert.ToInt32(collectionLine.SIId)); collectionLine.Amount = 0; }
                                            db.SubmitChanges();
                                        }

                                        journal.CancelJournal(Convert.ToInt32(id), "Collection");
                                    }

                                    String newObject = auditTrail.GetObjectString(collection.FirstOrDefault());
                                    auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Cancel Error. Cannot cancel collection detail if collection detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These collection details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to cancel collection.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this collection page.");
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
        // Delete Collection
        // =================
        [Authorize, HttpDelete, Route("api/collection/delete/{id}")]
        public HttpResponseMessage DeleteCollection(String id)
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
                                    && d.SysForm.FormName.Equals("CollectionList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var collection = from d in db.TrnCollections
                                             where d.Id == Convert.ToInt32(id)
                                             select d;

                            if (collection.Any())
                            {
                                if (!collection.FirstOrDefault().IsLocked)
                                {
                                    db.TrnCollections.DeleteOnSubmit(collection.First());

                                    String oldObject = auditTrail.GetObjectString(collection.FirstOrDefault());
                                    auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete collection if the current collection record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These collection details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete collection.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this collection page.");
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
