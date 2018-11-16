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
    public class ApiTrnReceivingReceiptController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========
        // Business
        // ========
        private Business.AccountsPayable accountsPayable = new Business.AccountsPayable();
        private Business.Inventory inventory = new Business.Inventory();
        private Business.Journal journal = new Business.Journal();
        private Business.AuditTrail auditTrail = new Business.AuditTrail();

        // ======================
        // List Receiving Receipt
        // ======================
        [Authorize, HttpGet, Route("api/receivingReceipt/list/{startDate}/{endDate}")]
        public List<Entities.TrnReceivingReceipt> ListReceivingReceipt(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var receivingReceipts = from d in db.TrnReceivingReceipts.OrderByDescending(d => d.Id)
                                    where d.BranchId == branchId
                                    && d.RRDate >= Convert.ToDateTime(startDate)
                                    && d.RRDate <= Convert.ToDateTime(endDate)
                                    select new Entities.TrnReceivingReceipt
                                    {
                                        Id = d.Id,
                                        RRNumber = d.RRNumber,
                                        RRDate = d.RRDate.ToShortDateString(),
                                        ManualRRNumber = d.ManualRRNumber,
                                        DocumentReference = d.DocumentReference,
                                        Supplier = d.MstArticle.Article,
                                        Amount = d.Amount,
                                        PaidAmount = d.PaidAmount,
                                        IsLocked = d.IsLocked,
                                        IsCancelled = d.IsCancelled,
                                        CreatedById = d.CreatedById,
                                        CreatedBy = d.MstUser2.FullName,
                                        CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                        UpdatedById = d.UpdatedById,
                                        UpdatedBy = d.MstUser5.FullName,
                                        UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                    };

            return receivingReceipts.ToList();
        }

        // ========================
        // Detail Receiving Receipt
        // ========================
        [Authorize, HttpGet, Route("api/receivingReceipt/detail/{id}")]
        public Entities.TrnReceivingReceipt DetailReceivingReceipt(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var receivingReceipt = from d in db.TrnReceivingReceipts.OrderByDescending(d => d.Id)
                                   where d.BranchId == branchId
                                   && d.Id == Convert.ToInt32(id)
                                   select new Entities.TrnReceivingReceipt
                                   {
                                       Id = d.Id,
                                       BranchId = d.BranchId,
                                       RRNumber = d.RRNumber,
                                       RRDate = d.RRDate.ToShortDateString(),
                                       DocumentReference = d.DocumentReference,
                                       SupplierId = d.SupplierId,
                                       TermId = d.TermId,
                                       Remarks = d.Remarks,
                                       ManualRRNumber = d.ManualRRNumber,
                                       ReceivedById = d.ReceivedById,
                                       PreparedById = d.PreparedById,
                                       CheckedById = d.CheckedById,
                                       ApprovedById = d.ApprovedById,
                                       Status = d.Status,
                                       IsCancelled = d.IsCancelled,
                                       IsLocked = d.IsLocked,
                                       CreatedById = d.CreatedById,
                                       CreatedBy = d.MstUser2.FullName,
                                       CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                       UpdatedById = d.UpdatedById,
                                       UpdatedBy = d.MstUser5.FullName,
                                       UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                   };

            if (receivingReceipt.Any())
            {
                return receivingReceipt.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/receivingReceipt/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListReceivingReceiptBranch()
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
        [Authorize, HttpGet, Route("api/receivingReceipt/dropdown/list/supplier")]
        public List<Entities.MstArticle> DropdownListReceivingReceiptSupplier()
        {
            var suppliers = from d in db.MstArticles.OrderBy(d => d.Article)
                            where d.ArticleTypeId == 3
                            && d.IsLocked == true
                            select new Entities.MstArticle
                            {
                                Id = d.Id,
                                Article = d.Article
                            };

            return suppliers.ToList();
        }

        // ============================
        // Dropdown List - Term (Field)
        // ============================
        [Authorize, HttpGet, Route("api/receivingReceipt/dropdown/list/term")]
        public List<Entities.MstTerm> DropdownListReceivingReceiptTerm()
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

        // ============================
        // Dropdown List - User (Field)
        // ============================
        [Authorize, HttpGet, Route("api/receivingReceipt/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListReceivingReceiptUsers()
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
        [Authorize, HttpGet, Route("api/receivingReceipt/dropdown/list/status")]
        public List<Entities.MstStatus> DropdownListReceivingReceiptStatus()
        {
            var statuses = from d in db.MstStatus.OrderBy(d => d.Status)
                           where d.IsLocked == true
                           && d.Category.Equals("RR")
                           select new Entities.MstStatus
                           {
                               Id = d.Id,
                               Status = d.Status
                           };

            return statuses.ToList();
        }

        // ============================
        // Update Purchase Order Status
        // ============================
        public void UpdatePurchaseOrderStatus(Int32 RRId)
        {
            var receivedPOs = from d in db.TrnReceivingReceiptItems where d.RRId == RRId group d by d.POId into g select g;
            if (receivedPOs.Any())
            {
                foreach (var receivedPO in receivedPOs)
                {
                    Decimal balanceQuantity = 0;

                    var purchasedItems = from d in db.TrnPurchaseOrderItems where d.POId == receivedPO.Key group d by d.ItemId into g select g;
                    if (purchasedItems.Any())
                    {
                        Decimal purchasedItemsQuantity = 0;
                        Decimal receivedItemsQuantity = 0;

                        foreach (var purchasedItem in purchasedItems)
                        {
                            purchasedItemsQuantity = purchasedItem.Sum(d => d.Quantity);

                            var receivedItems = from d in db.TrnReceivingReceiptItems
                                                where d.POId == receivedPO.Key && d.ItemId == purchasedItem.Key && d.TrnReceivingReceipt.IsLocked == true
                                                group d by d.ItemId into g
                                                select g;

                            if (receivedItems.Any())
                            {
                                receivedItemsQuantity = receivedItems.FirstOrDefault().Sum(d => d.Quantity);
                            }

                            balanceQuantity += purchasedItemsQuantity - receivedItemsQuantity;
                        }
                    }

                    var currentPurchaseOrder = from d in db.TrnPurchaseOrders where d.Id == receivedPO.Key select d;
                    if (currentPurchaseOrder.Any())
                    {
                        Boolean isClose = false;
                        if (balanceQuantity <= 0)
                        {
                            isClose = true;
                        }

                        var updatePurchaseOrder = currentPurchaseOrder.FirstOrDefault();
                        updatePurchaseOrder.IsClose = isClose;
                        db.SubmitChanges();

                        balanceQuantity = 0;
                    }
                }
            }
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

        // =====================
        // Add Receiving Receipt
        // =====================
        [Authorize, HttpPost, Route("api/receivingReceipt/add")]
        public HttpResponseMessage AddReceivingReceipt()
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
                                    && d.SysForm.FormName.Equals("ReceivingReceiptList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultRRNumber = "0000000001";
                            var lastReceivingReceipt = from d in db.TrnReceivingReceipts.OrderByDescending(d => d.Id)
                                                       where d.BranchId == currentBranchId
                                                       select d;

                            if (lastReceivingReceipt.Any())
                            {
                                var RRNumber = Convert.ToInt32(lastReceivingReceipt.FirstOrDefault().RRNumber) + 0000000001;
                                defaultRRNumber = FillLeadingZeroes(RRNumber, 10);
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
                                        Data.TrnReceivingReceipt newReceivingReceipt = new Data.TrnReceivingReceipt
                                        {
                                            BranchId = currentBranchId,
                                            RRNumber = defaultRRNumber,
                                            RRDate = DateTime.Today,
                                            DocumentReference = "NA",
                                            SupplierId = suppliers.FirstOrDefault().Id,
                                            TermId = terms.FirstOrDefault().Id,
                                            Remarks = "NA",
                                            ManualRRNumber = "NA",
                                            Amount = 0,
                                            WTaxAmount = 0,
                                            PaidAmount = 0,
                                            AdjustmentAmount = 0,
                                            BalanceAmount = 0,
                                            ReceivedById = currentUserId,
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

                                        db.TrnReceivingReceipts.InsertOnSubmit(newReceivingReceipt);
                                        db.SubmitChanges();

                                        String newObject = auditTrail.GetObjectString(newReceivingReceipt);
                                        auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                        return Request.CreateResponse(HttpStatusCode.OK, newReceivingReceipt.Id);
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
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add receiving receipt.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this receiving receipt page.");
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

        // ======================
        // Save Receiving Receipt
        // ======================
        [Authorize, HttpPut, Route("api/receivingReceipt/save/{id}")]
        public HttpResponseMessage SaveReceivingReceipt(Entities.TrnReceivingReceipt objReceivingReceipt, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var receivingReceipt = from d in db.TrnReceivingReceipts where d.Id == Convert.ToInt32(id) select d;
                    if (receivingReceipt.Any())
                    {
                        if (!receivingReceipt.FirstOrDefault().IsLocked)
                        {
                            String oldObject = auditTrail.GetObjectString(receivingReceipt.FirstOrDefault());

                            Decimal amount = 0;
                            var receivingReceiptItems = from d in db.TrnReceivingReceiptItems where d.RRId == Convert.ToInt32(id) select d;
                            if (receivingReceiptItems.Any()) { amount = receivingReceiptItems.Sum(d => d.Amount); }

                            var saveReceivingReceipt = receivingReceipt.FirstOrDefault();
                            saveReceivingReceipt.RRDate = Convert.ToDateTime(objReceivingReceipt.RRDate);
                            saveReceivingReceipt.DocumentReference = objReceivingReceipt.DocumentReference;
                            saveReceivingReceipt.SupplierId = objReceivingReceipt.SupplierId;
                            saveReceivingReceipt.TermId = objReceivingReceipt.TermId;
                            saveReceivingReceipt.Remarks = objReceivingReceipt.Remarks;
                            saveReceivingReceipt.ManualRRNumber = objReceivingReceipt.ManualRRNumber;
                            saveReceivingReceipt.Amount = amount;
                            saveReceivingReceipt.ReceivedById = objReceivingReceipt.ReceivedById;
                            saveReceivingReceipt.CheckedById = objReceivingReceipt.CheckedById;
                            saveReceivingReceipt.ApprovedById = objReceivingReceipt.ApprovedById;
                            saveReceivingReceipt.Status = objReceivingReceipt.Status;
                            saveReceivingReceipt.UpdatedById = currentUserId;
                            saveReceivingReceipt.UpdatedDateTime = DateTime.Now;
                            db.SubmitChanges();

                            String newObject = auditTrail.GetObjectString(receivingReceipt.FirstOrDefault());
                            auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Saving Error. These receiving receipt details are already locked.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These receiving receipt details are not found in the server.");
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

        // ======================
        // Lock Receiving Receipt
        // ======================
        [Authorize, HttpPut, Route("api/receivingReceipt/lock/{id}")]
        public HttpResponseMessage LockReceivingReceipt(Entities.TrnReceivingReceipt objReceivingReceipt, String id)
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
                                    && d.SysForm.FormName.Equals("ReceivingReceiptDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var receivingReceipt = from d in db.TrnReceivingReceipts
                                                   where d.Id == Convert.ToInt32(id)
                                                   select d;

                            if (receivingReceipt.Any())
                            {
                                int countInvalidPO = 0;
                                var invalidPOs = from d in db.TrnReceivingReceiptItems
                                                 where d.RRId == receivingReceipt.FirstOrDefault().Id
                                                 && d.TrnPurchaseOrder.IsLocked == false
                                                 && d.TrnPurchaseOrder.IsCancelled == true
                                                 select d;

                                if (invalidPOs.Any())
                                {
                                    countInvalidPO = invalidPOs.Count();
                                }

                                if (!receivingReceipt.FirstOrDefault().IsLocked && countInvalidPO == 0)
                                {
                                    String oldObject = auditTrail.GetObjectString(receivingReceipt.FirstOrDefault());

                                    Decimal amount = 0;
                                    var receivingReceiptItems = from d in db.TrnReceivingReceiptItems where d.RRId == Convert.ToInt32(id) select d;
                                    if (receivingReceiptItems.Any()) { amount = receivingReceiptItems.Sum(d => d.Amount); }

                                    var lockReceivingReceipt = receivingReceipt.FirstOrDefault();
                                    lockReceivingReceipt.RRDate = Convert.ToDateTime(objReceivingReceipt.RRDate);
                                    lockReceivingReceipt.DocumentReference = objReceivingReceipt.DocumentReference;
                                    lockReceivingReceipt.SupplierId = objReceivingReceipt.SupplierId;
                                    lockReceivingReceipt.TermId = objReceivingReceipt.TermId;
                                    lockReceivingReceipt.Remarks = objReceivingReceipt.Remarks;
                                    lockReceivingReceipt.ManualRRNumber = objReceivingReceipt.ManualRRNumber;
                                    lockReceivingReceipt.Amount = amount;
                                    lockReceivingReceipt.ReceivedById = objReceivingReceipt.ReceivedById;
                                    lockReceivingReceipt.CheckedById = objReceivingReceipt.CheckedById;
                                    lockReceivingReceipt.ApprovedById = objReceivingReceipt.ApprovedById;
                                    lockReceivingReceipt.Status = objReceivingReceipt.Status;
                                    lockReceivingReceipt.IsLocked = true;
                                    lockReceivingReceipt.UpdatedById = currentUserId;
                                    lockReceivingReceipt.UpdatedDateTime = DateTime.Now;
                                    db.SubmitChanges();

                                    if (lockReceivingReceipt.IsLocked)
                                    {
                                        UpdatePurchaseOrderStatus(Convert.ToInt32(id));

                                        accountsPayable.UpdateAccountsPayable(Convert.ToInt32(id));
                                        inventory.InsertReceivingReceiptInventory(Convert.ToInt32(id));
                                        journal.InsertReceivingReceiptJournal(Convert.ToInt32(id));
                                    }

                                    String newObject = auditTrail.GetObjectString(receivingReceipt.FirstOrDefault());
                                    auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These receiving receipt details are already locked or PO is invalid..");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These receiving receipt details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock receiving receipt.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this receiving receipt page.");
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

        // ========================
        // Unlock Receiving Receipt
        // ========================
        [Authorize, HttpPut, Route("api/receivingReceipt/unlock/{id}")]
        public HttpResponseMessage UnlockReceivingReceipt(String id)
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
                                    && d.SysForm.FormName.Equals("ReceivingReceiptDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var receivingReceipt = from d in db.TrnReceivingReceipts
                                                   where d.Id == Convert.ToInt32(id)
                                                   select d;

                            if (receivingReceipt.Any())
                            {
                                if (!receivingReceipt.FirstOrDefault().IsCancelled)
                                {
                                    if (receivingReceipt.FirstOrDefault().IsLocked)
                                    {
                                        String oldObject = auditTrail.GetObjectString(receivingReceipt.FirstOrDefault());

                                        var unlockReceivingReceipt = receivingReceipt.FirstOrDefault();
                                        unlockReceivingReceipt.IsLocked = false;
                                        unlockReceivingReceipt.UpdatedById = currentUserId;
                                        unlockReceivingReceipt.UpdatedDateTime = DateTime.Now;
                                        db.SubmitChanges();

                                        if (!unlockReceivingReceipt.IsLocked)
                                        {
                                            UpdatePurchaseOrderStatus(Convert.ToInt32(id));

                                            inventory.DeleteReceivingReceiptInventory(Convert.ToInt32(id));
                                            journal.DeleteReceivingReceiptJournal(Convert.ToInt32(id));
                                        }

                                        String newObject = auditTrail.GetObjectString(receivingReceipt.FirstOrDefault());
                                        auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These receiving receipt details are already unlocked.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These receiving receipt details are already cancelled.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These receiving receipt details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock receiving receipt.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this receiving receipt page.");
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

        // ========================
        // Cancel Receiving Receipt
        // ========================
        [Authorize, HttpPut, Route("api/receivingReceipt/cancel/{id}")]
        public HttpResponseMessage CancelReceivingReceipt(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms where d.UserId == currentUserId && d.SysForm.FormName.Equals("ReceivingReceiptDetail") select d;
                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanCancel)
                        {
                            var receivingReceipt = from d in db.TrnReceivingReceipts where d.Id == Convert.ToInt32(id) select d;
                            if (receivingReceipt.Any())
                            {
                                if (receivingReceipt.FirstOrDefault().IsLocked)
                                {
                                    if (receivingReceipt.FirstOrDefault().PaidAmount + receivingReceipt.FirstOrDefault().AdjustmentAmount == 0)
                                    {
                                        String oldObject = auditTrail.GetObjectString(receivingReceipt.FirstOrDefault());

                                        var cancelReceivingReceipt = receivingReceipt.FirstOrDefault();
                                        cancelReceivingReceipt.Amount = 0;
                                        cancelReceivingReceipt.WTaxAmount = 0;
                                        cancelReceivingReceipt.PaidAmount = 0;
                                        cancelReceivingReceipt.AdjustmentAmount = 0;
                                        cancelReceivingReceipt.BalanceAmount = 0;
                                        cancelReceivingReceipt.IsCancelled = true;
                                        cancelReceivingReceipt.UpdatedById = currentUserId;
                                        cancelReceivingReceipt.UpdatedDateTime = DateTime.Now;
                                        db.SubmitChanges();

                                        if (cancelReceivingReceipt.IsCancelled)
                                        {
                                            inventory.DeleteReceivingReceiptInventory(Convert.ToInt32(id));
                                            journal.CancelJournal(Convert.ToInt32(id), "ReceivingReceipt");
                                        }

                                        String newObject = auditTrail.GetObjectString(receivingReceipt.FirstOrDefault());
                                        auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Cancel Error. Cannot cancel receiving receipt detail if receiving receipt detail is paid or adjusted.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Cancel Error. Cannot cancel receiving receipt detail if receiving receipt detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These receiving receipt details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to cancel receiving receipt.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this receiving receipt page.");
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

        // ========================
        // Delete Receiving Receipt
        // ========================
        [Authorize, HttpDelete, Route("api/receivingReceipt/delete/{id}")]
        public HttpResponseMessage DeleteReceivingReceipt(String id)
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
                                    && d.SysForm.FormName.Equals("ReceivingReceiptList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var receivingReceipt = from d in db.TrnReceivingReceipts
                                                   where d.Id == Convert.ToInt32(id)
                                                   select d;

                            if (receivingReceipt.Any())
                            {
                                if (!receivingReceipt.FirstOrDefault().IsLocked)
                                {
                                    db.TrnReceivingReceipts.DeleteOnSubmit(receivingReceipt.First());

                                    String oldObject = auditTrail.GetObjectString(receivingReceipt.FirstOrDefault());
                                    auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete receiving receipt if the current receiving receipt record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These receiving receipt details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete receiving receipt.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this receiving receipt page.");
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
