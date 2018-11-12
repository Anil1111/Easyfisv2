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
    public class ApiTrnSalesInvoiceController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========
        // Business
        // ========
        private Business.AccountsReceivable accountsReceivable = new Business.AccountsReceivable();
        private Business.Inventory inventory = new Business.Inventory();
        private Business.Journal journal = new Business.Journal();
        private Business.AuditTrail auditTrail = new Business.AuditTrail();

        // ==================
        // List Sales Invoice
        // ==================
        [Authorize, HttpGet, Route("api/salesInvoice/list/{startDate}/{endDate}")]
        public List<Entities.TrnSalesInvoice> ListSalesInvoice(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var salesInvoices = from d in db.TrnSalesInvoices.OrderByDescending(d => d.Id)
                                where d.BranchId == branchId
                                && d.SIDate >= Convert.ToDateTime(startDate)
                                && d.SIDate <= Convert.ToDateTime(endDate)
                                select new Entities.TrnSalesInvoice
                                {
                                    Id = d.Id,
                                    SINumber = d.SINumber,
                                    SIDate = d.SIDate.ToShortDateString(),
                                    ManualSINumber = d.ManualSINumber,
                                    Customer = d.MstArticle.Article,
                                    Remarks = d.Remarks,
                                    DocumentReference = d.DocumentReference,
                                    Amount = d.Amount,
                                    IsLocked = d.IsLocked,
                                    IsCancelled = d.IsCancelled,
                                    CreatedBy = d.MstUser2.FullName,
                                    CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                    UpdatedBy = d.MstUser5.FullName,
                                    UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                };

            return salesInvoices.ToList();
        }

        // ====================
        // Detail Sales Invoice
        // ====================
        [Authorize, HttpGet, Route("api/salesInvoice/detail/{id}")]
        public Entities.TrnSalesInvoice DetailSalesInvoice(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var salesInvoice = from d in db.TrnSalesInvoices.OrderByDescending(d => d.Id)
                               where d.BranchId == branchId
                               && d.Id == Convert.ToInt32(id)
                               select new Entities.TrnSalesInvoice
                               {
                                   Id = d.Id,
                                   BranchId = d.BranchId,
                                   SINumber = d.SINumber,
                                   SIDate = d.SIDate.ToShortDateString(),
                                   DocumentReference = d.DocumentReference,
                                   CustomerId = d.CustomerId,
                                   TermId = d.TermId,
                                   Remarks = d.Remarks,
                                   ManualSINumber = d.ManualSINumber,
                                   SoldById = d.SoldById,
                                   PreparedById = d.PreparedById,
                                   CheckedById = d.CheckedById,
                                   ApprovedById = d.ApprovedById,
                                   Status = d.Status,
                                   IsCancelled = d.IsCancelled,
                                   IsLocked = d.IsLocked,
                                   CreatedBy = d.MstUser2.FullName,
                                   CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                   UpdatedBy = d.MstUser5.FullName,
                                   UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                               };

            if (salesInvoice.Any())
            {
                return salesInvoice.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/salesInvoice/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListSalesInvoiceBranch()
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
        [Authorize, HttpGet, Route("api/salesInvoice/dropdown/list/customer")]
        public List<Entities.MstArticle> DropdownListSalesInvoiceCustomer()
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
        // Dropdown List - Term (Field)
        // ============================
        [Authorize, HttpGet, Route("api/salesInvoice/dropdown/list/term")]
        public List<Entities.MstTerm> DropdownListSalesInvoiceTerm()
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
        [Authorize, HttpGet, Route("api/salesInvoice/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListSalesInvoiceUsers()
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
        [Authorize, HttpGet, Route("api/salesInvoice/dropdown/list/status")]
        public List<Entities.MstStatus> DropdownListSalesInvoiceStatus()
        {
            var statuses = from d in db.MstStatus.OrderBy(d => d.Status)
                           where d.IsLocked == true
                           && d.Category.Equals("SI")
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

        // =================
        // Add Sales Invoice
        // =================
        [Authorize, HttpPost, Route("api/salesInvoice/add")]
        public HttpResponseMessage AddSalesInvoice()
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

                    var currentSalesInvoiceCheckedById = currentUserId;
                    if (currentUser.FirstOrDefault().SalesInvoiceCheckedById != null)
                    {
                        currentSalesInvoiceCheckedById = Convert.ToInt32(currentUser.FirstOrDefault().SalesInvoiceCheckedById);
                    }

                    var currentSalesInvoiceApprovedById = currentUserId;
                    if (currentUser.FirstOrDefault().SalesInvoiceApprovedById != null)
                    {
                        currentSalesInvoiceApprovedById = Convert.ToInt32(currentUser.FirstOrDefault().SalesInvoiceApprovedById);
                    }

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("SalesInvoiceList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultSINumber = "0000000001";
                            var lastSalesInvoice = from d in db.TrnSalesInvoices.OrderByDescending(d => d.Id)
                                                   where d.BranchId == currentBranchId
                                                   select d;

                            if (lastSalesInvoice.Any())
                            {
                                var SINumber = Convert.ToInt32(lastSalesInvoice.FirstOrDefault().SINumber) + 0000000001;
                                defaultSINumber = FillLeadingZeroes(SINumber, 10);
                            }

                            var customers = from d in db.MstArticles.OrderBy(d => d.Article)
                                            where d.ArticleTypeId == 2
                                            && d.IsLocked == true
                                            select d;

                            if (customers.Any())
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
                                        Data.TrnSalesInvoice newSalesInvoice = new Data.TrnSalesInvoice
                                        {
                                            BranchId = currentBranchId,
                                            SINumber = defaultSINumber,
                                            SIDate = DateTime.Today,
                                            DocumentReference = "NA",
                                            CustomerId = customers.FirstOrDefault().Id,
                                            TermId = terms.FirstOrDefault().Id,
                                            Remarks = "NA",
                                            ManualSINumber = "NA",
                                            Amount = 0,
                                            PaidAmount = 0,
                                            AdjustmentAmount = 0,
                                            BalanceAmount = 0,
                                            SoldById = currentUserId,
                                            PreparedById = currentUserId,
                                            CheckedById = currentSalesInvoiceCheckedById,
                                            ApprovedById = currentSalesInvoiceApprovedById,
                                            Status = null,
                                            IsCancelled = false,
                                            IsLocked = false,
                                            CreatedById = currentUserId,
                                            CreatedDateTime = DateTime.Now,
                                            UpdatedById = currentUserId,
                                            UpdatedDateTime = DateTime.Now
                                        };

                                        db.TrnSalesInvoices.InsertOnSubmit(newSalesInvoice);
                                        db.SubmitChanges();

                                        String newObject = auditTrail.GetObjectString(newSalesInvoice);
                                        auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                        return Request.CreateResponse(HttpStatusCode.OK, newSalesInvoice.Id);
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
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add sales invoice.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this sales invoice page.");
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

        // ==================
        // Save Sales Invoice
        // ==================
        [Authorize, HttpPut, Route("api/salesInvoice/save/{id}")]
        public HttpResponseMessage SaveSalesInvoice(Entities.TrnSalesInvoice objSalesInvoice, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var salesInvoice = from d in db.TrnSalesInvoices where d.Id == Convert.ToInt32(id) select d;
                    if (salesInvoice.Any())
                    {
                        if (!salesInvoice.FirstOrDefault().IsLocked)
                        {
                            String oldObject = auditTrail.GetObjectString(salesInvoice.FirstOrDefault());

                            Decimal amount = 0;
                            var salesInvoiceItems = from d in db.TrnSalesInvoiceItems where d.SIId == Convert.ToInt32(id) select d;
                            if (salesInvoiceItems.Any()) { amount = salesInvoiceItems.Sum(d => d.Amount); }

                            var saveSalesInvoice = salesInvoice.FirstOrDefault();
                            saveSalesInvoice.SIDate = Convert.ToDateTime(objSalesInvoice.SIDate);
                            saveSalesInvoice.CustomerId = objSalesInvoice.CustomerId;
                            saveSalesInvoice.TermId = objSalesInvoice.TermId;
                            saveSalesInvoice.DocumentReference = objSalesInvoice.DocumentReference;
                            saveSalesInvoice.ManualSINumber = objSalesInvoice.ManualSINumber;
                            saveSalesInvoice.Remarks = objSalesInvoice.Remarks;
                            saveSalesInvoice.Amount = amount;
                            saveSalesInvoice.SoldById = objSalesInvoice.SoldById;
                            saveSalesInvoice.CheckedById = objSalesInvoice.CheckedById;
                            saveSalesInvoice.ApprovedById = objSalesInvoice.ApprovedById;
                            saveSalesInvoice.Status = objSalesInvoice.Status;
                            saveSalesInvoice.UpdatedById = currentUserId;
                            saveSalesInvoice.UpdatedDateTime = DateTime.Now;
                            db.SubmitChanges();

                            String newObject = auditTrail.GetObjectString(salesInvoice.FirstOrDefault());
                            auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Saving Error. These sales invoice details are already locked.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These sales invoice details are not found in the server.");
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

        // ==================
        // Lock Sales Invoice
        // ==================
        [Authorize, HttpPut, Route("api/salesInvoice/lock/{id}")]
        public HttpResponseMessage LockSalesInvoice(Entities.TrnSalesInvoice objSalesInvoice, String id)
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
                                    && d.SysForm.FormName.Equals("SalesInvoiceDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var salesInvoice = from d in db.TrnSalesInvoices
                                               where d.Id == Convert.ToInt32(id)
                                               select d;

                            if (salesInvoice.Any())
                            {
                                if (!salesInvoice.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = auditTrail.GetObjectString(salesInvoice.FirstOrDefault());

                                    Decimal amount = 0;
                                    var salesInvoiceItems = from d in db.TrnSalesInvoiceItems where d.SIId == Convert.ToInt32(id) select d;
                                    if (salesInvoiceItems.Any()) { amount = salesInvoiceItems.Sum(d => d.Amount); }

                                    var lockSalesInvoice = salesInvoice.FirstOrDefault();
                                    lockSalesInvoice.SIDate = Convert.ToDateTime(objSalesInvoice.SIDate);
                                    lockSalesInvoice.CustomerId = objSalesInvoice.CustomerId;
                                    lockSalesInvoice.TermId = objSalesInvoice.TermId;
                                    lockSalesInvoice.DocumentReference = objSalesInvoice.DocumentReference;
                                    lockSalesInvoice.ManualSINumber = objSalesInvoice.ManualSINumber;
                                    lockSalesInvoice.Remarks = objSalesInvoice.Remarks;
                                    lockSalesInvoice.Amount = amount;
                                    lockSalesInvoice.SoldById = objSalesInvoice.SoldById;
                                    lockSalesInvoice.CheckedById = objSalesInvoice.CheckedById;
                                    lockSalesInvoice.ApprovedById = objSalesInvoice.ApprovedById;
                                    lockSalesInvoice.Status = objSalesInvoice.Status;
                                    lockSalesInvoice.IsLocked = true;
                                    lockSalesInvoice.UpdatedById = currentUserId;
                                    lockSalesInvoice.UpdatedDateTime = DateTime.Now;
                                    db.SubmitChanges();

                                    if (lockSalesInvoice.IsLocked)
                                    {
                                        accountsReceivable.UpdateAccountsReceivable(Convert.ToInt32(id));
                                        inventory.InsertSalesInvoiceInventory(Convert.ToInt32(id));
                                        journal.InsertSalesInvoiceJournal(Convert.ToInt32(id));
                                    }

                                    String newObject = auditTrail.GetObjectString(salesInvoice.FirstOrDefault());
                                    auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These sales invoice details are already locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These sales invoice details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock sales invoice.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this sales invoice page.");
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

        // ====================
        // Unlock Sales Invoice
        // ====================
        [Authorize, HttpPut, Route("api/salesInvoice/unlock/{id}")]
        public HttpResponseMessage UnlockSalesInvoice(String id)
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
                                    && d.SysForm.FormName.Equals("SalesInvoiceDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var salesInvoice = from d in db.TrnSalesInvoices
                                               where d.Id == Convert.ToInt32(id)
                                               select d;

                            if (salesInvoice.Any())
                            {
                                if (salesInvoice.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = auditTrail.GetObjectString(salesInvoice.FirstOrDefault());

                                    var unlockSalesInvoice = salesInvoice.FirstOrDefault();
                                    unlockSalesInvoice.IsLocked = false;
                                    unlockSalesInvoice.UpdatedById = currentUserId;
                                    unlockSalesInvoice.UpdatedDateTime = DateTime.Now;
                                    db.SubmitChanges();

                                    if (!unlockSalesInvoice.IsLocked)
                                    {
                                        inventory.DeleteSalesInvoiceInventory(Convert.ToInt32(id));
                                        journal.DeleteSalesInvoiceJournal(Convert.ToInt32(id));
                                    }

                                    String newObject = auditTrail.GetObjectString(salesInvoice.FirstOrDefault());
                                    auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These sales invoice details are already unlocked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These sales invoice details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock sales invoice.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this sales invoice page.");
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

        // ====================
        // Cancel Sales Invoice
        // ====================
        [Authorize, HttpPut, Route("api/salesInvoice/cancel/{id}")]
        public HttpResponseMessage CancelSalesInvoice(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms where d.UserId == currentUserId && d.SysForm.FormName.Equals("SalesInvoiceDetail") select d;
                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanCancel)
                        {
                            var salesInvoice = from d in db.TrnSalesInvoices where d.Id == Convert.ToInt32(id) select d;
                            if (salesInvoice.Any())
                            {
                                if (salesInvoice.FirstOrDefault().IsLocked)
                                {
                                    if (salesInvoice.FirstOrDefault().PaidAmount + salesInvoice.FirstOrDefault().AdjustmentAmount == 0)
                                    {
                                        String oldObject = auditTrail.GetObjectString(salesInvoice.FirstOrDefault());

                                        var cancelSalesInvoice = salesInvoice.FirstOrDefault();
                                        cancelSalesInvoice.Amount = 0;
                                        cancelSalesInvoice.PaidAmount = 0;
                                        cancelSalesInvoice.AdjustmentAmount = 0;
                                        cancelSalesInvoice.BalanceAmount = 0;
                                        cancelSalesInvoice.IsCancelled = true;
                                        cancelSalesInvoice.UpdatedById = currentUserId;
                                        cancelSalesInvoice.UpdatedDateTime = DateTime.Now;
                                        db.SubmitChanges();

                                        if (cancelSalesInvoice.IsCancelled)
                                        {
                                            inventory.DeleteSalesInvoiceInventory(Convert.ToInt32(id));
                                            journal.CancelJournal(Convert.ToInt32(id), "SalesInvoice");
                                        }

                                        String newObject = auditTrail.GetObjectString(salesInvoice.FirstOrDefault());
                                        auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Cancel Error. Cannot cancel sales invoice detail if sales invoice detail is paid and adjusted.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Cancel Error. Cannot cancel sales invoice detail if sales invoice detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These sales invoice details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to cancel sales invoice.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this sales invoice page.");
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

        // ====================
        // Delete Sales Invoice
        // ====================
        [Authorize, HttpDelete, Route("api/salesInvoice/delete/{id}")]
        public HttpResponseMessage DeleteSalesInvoice(String id)
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
                                    && d.SysForm.FormName.Equals("SalesInvoiceList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var salesInvoice = from d in db.TrnSalesInvoices
                                               where d.Id == Convert.ToInt32(id)
                                               select d;

                            if (salesInvoice.Any())
                            {
                                if (!salesInvoice.FirstOrDefault().IsLocked)
                                {
                                    db.TrnSalesInvoices.DeleteOnSubmit(salesInvoice.First());

                                    String oldObject = auditTrail.GetObjectString(salesInvoice.FirstOrDefault());
                                    auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete sales invoice if the current sales invoice record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These sales invoice details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete sales invoice.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this sales invoice page.");
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
