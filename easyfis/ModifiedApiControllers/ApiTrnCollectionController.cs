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
    public class ApiTrnCollectionController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =====================
        // Get Collection Amount
        // =====================
        public Decimal GetCollectionAmount(Int32 ORId)
        {
            var collectionLines = from d in db.TrnCollectionLines
                                  where d.ORId == ORId
                                  select d;

            if (collectionLines.Any())
            {
                return collectionLines.Sum(d => d.Amount);
            }
            else
            {
                return 0;
            }
        }

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
                                  Customer = d.MstArticle.Article,
                                  Particulars = d.Particulars,
                                  Amount = GetCollectionAmount(d.Id),
                                  IsLocked = d.IsLocked,
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
                                        IsLocked = false,
                                        CreatedById = currentUserId,
                                        CreatedDateTime = DateTime.Now,
                                        UpdatedById = currentUserId,
                                        UpdatedDateTime = DateTime.Now
                                    };

                                    db.TrnCollections.InsertOnSubmit(newCollection);
                                    db.SubmitChanges();

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

        // ==========================
        // Update Accounts Receivable
        // ==========================
        public void UpdateAccountsReceivable(Int32 ORId)
        {
            var collectionLines = from d in db.TrnCollectionLines
                                  where d.ORId == ORId
                                  && d.TrnCollection.IsLocked == true
                                  select d;

            if (collectionLines.Any())
            {
                foreach (var collectionLine in collectionLines)
                {
                    if (collectionLine.SIId != null)
                    {
                        Decimal paidAmount = 0;

                        var collectionLinesAmount = from d in db.TrnCollectionLines
                                                    where d.SIId == collectionLine.SIId
                                                    && d.TrnCollection.IsLocked == true
                                                    select d;

                        if (collectionLinesAmount.Any())
                        {
                            paidAmount = collectionLinesAmount.Sum(d => d.Amount);
                        }

                        Decimal adjustmentAmount = 0;

                        var journalVoucherLines = from d in db.TrnJournalVoucherLines
                                                  where d.ARSIId == collectionLine.SIId
                                                  && d.TrnJournalVoucher.IsLocked == true
                                                  select d;

                        if (journalVoucherLines.Any())
                        {
                            Decimal debitAmount = journalVoucherLines.Sum(d => d.DebitAmount);
                            Decimal creditAmount = journalVoucherLines.Sum(d => d.CreditAmount);

                            adjustmentAmount = debitAmount - creditAmount;
                        }

                        var salesInvoice = from d in db.TrnSalesInvoices
                                           where d.Id == collectionLine.SIId
                                           && d.IsLocked == true
                                           select d;

                        if (salesInvoice.Any())
                        {
                            Decimal salesInvoiceAmount = salesInvoice.FirstOrDefault().Amount;
                            Decimal balanceAmount = (salesInvoiceAmount - paidAmount) + adjustmentAmount;

                            var updateSalesInvoice = salesInvoice.FirstOrDefault();
                            updateSalesInvoice.PaidAmount = paidAmount;
                            updateSalesInvoice.AdjustmentAmount = adjustmentAmount;
                            updateSalesInvoice.BalanceAmount = balanceAmount;
                            db.SubmitChanges();
                        }
                    }
                }
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
                                if (!collection.FirstOrDefault().IsLocked)
                                {
                                    var lockCollection = collection.FirstOrDefault();
                                    lockCollection.ORDate = Convert.ToDateTime(objCollection.ORDate);
                                    lockCollection.ManualORNumber = objCollection.ManualORNumber;
                                    lockCollection.CustomerId = objCollection.CustomerId;
                                    lockCollection.Particulars = objCollection.Particulars;
                                    lockCollection.CheckedById = objCollection.CheckedById;
                                    lockCollection.ApprovedById = objCollection.ApprovedById;
                                    lockCollection.IsLocked = true;
                                    lockCollection.UpdatedById = currentUserId;
                                    lockCollection.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    // ===============================
                                    // Journal and Accounts Receivable
                                    // ===============================
                                    Business.Journal journal = new Business.Journal();

                                    if (lockCollection.IsLocked)
                                    {
                                        journal.insertORJournal(Convert.ToInt32(id));
                                        UpdateAccountsReceivable(Convert.ToInt32(id));
                                    }

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These collection details are already locked.");
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
                                if (collection.FirstOrDefault().IsLocked)
                                {
                                    var unlockCollection = collection.FirstOrDefault();
                                    unlockCollection.IsLocked = false;
                                    unlockCollection.UpdatedById = currentUserId;
                                    unlockCollection.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    // ===============================
                                    // Journal and Accounts Receivable
                                    // ===============================
                                    Business.Journal journal = new Business.Journal();

                                    if (!unlockCollection.IsLocked)
                                    {
                                        journal.deleteORJournal(Convert.ToInt32(id));
                                        UpdateAccountsReceivable(Convert.ToInt32(id));
                                    }

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These collection details are already unlocked.");
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
