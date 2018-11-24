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
    public class ApiTrnJournalVoucherController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========
        // Business
        // ========
        private Business.AccountsPayable accountsPayable = new Business.AccountsPayable();
        private Business.AccountsReceivable accountsReceivable = new Business.AccountsReceivable();
        private Business.Journal journal = new Business.Journal();
        private Business.AuditTrail auditTrail = new Business.AuditTrail();

        // ====================
        // List Journal Voucher
        // ====================
        [Authorize, HttpGet, Route("api/journalVoucher/list/{startDate}/{endDate}")]
        public List<Entities.TrnJournalVoucher> ListJournalVoucher(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var journalVouchers = from d in db.TrnJournalVouchers.OrderByDescending(d => d.Id)
                                  where d.BranchId == branchId
                                  && d.JVDate >= Convert.ToDateTime(startDate)
                                  && d.JVDate <= Convert.ToDateTime(endDate)
                                  select new Entities.TrnJournalVoucher
                                  {
                                      Id = d.Id,
                                      JVNumber = d.JVNumber,
                                      JVDate = d.JVDate.ToShortDateString(),
                                      ManualJVNumber = d.ManualJVNumber,
                                      Particulars = d.Particulars,
                                      IsLocked = d.IsLocked,
                                      CreatedById = d.CreatedById,
                                      CreatedBy = d.MstUser3.FullName,
                                      CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                      UpdatedById = d.UpdatedById,
                                      UpdatedBy = d.MstUser4.FullName,
                                      UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                  };

            return journalVouchers.ToList();
        }

        // ======================
        // Detail Journal Voucher
        // ======================
        [Authorize, HttpGet, Route("api/journalVoucher/detail/{id}")]
        public Entities.TrnJournalVoucher DetailJournalVoucher(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var journalVoucher = from d in db.TrnJournalVouchers.OrderByDescending(d => d.Id)
                                 where d.BranchId == branchId
                                 && d.Id == Convert.ToInt32(id)
                                 select new Entities.TrnJournalVoucher
                                 {
                                     Id = d.Id,
                                     BranchId = d.BranchId,
                                     JVNumber = d.JVNumber,
                                     JVDate = d.JVDate.ToShortDateString(),
                                     ManualJVNumber = d.ManualJVNumber,
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

            if (journalVoucher.Any())
            {
                return journalVoucher.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/journalVoucher/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListJournalVoucherBranch()
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
        [Authorize, HttpGet, Route("api/journalVoucher/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListJournalVoucherUsers()
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
        [Authorize, HttpGet, Route("api/journalVoucher/dropdown/list/status")]
        public List<Entities.MstStatus> DropdownListJournalVoucherStatus()
        {
            var statuses = from d in db.MstStatus.OrderBy(d => d.Status)
                           where d.IsLocked == true
                           && d.Category.Equals("JV")
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

        // ===================
        // Add Journal Voucher
        // ===================
        [Authorize, HttpPost, Route("api/journalVoucher/add")]
        public HttpResponseMessage AddJournalVoucher()
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
                                    && d.SysForm.FormName.Equals("JournalVoucherList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultJVNumber = "0000000001";
                            var lastJournalVoucher = from d in db.TrnJournalVouchers.OrderByDescending(d => d.Id)
                                                     where d.BranchId == currentBranchId
                                                     select d;

                            if (lastJournalVoucher.Any())
                            {
                                var JVNumber = Convert.ToInt32(lastJournalVoucher.FirstOrDefault().JVNumber) + 0000000001;
                                defaultJVNumber = FillLeadingZeroes(JVNumber, 10);
                            }

                            var users = from d in db.MstUsers.OrderBy(d => d.FullName)
                                        where d.IsLocked == true
                                        select d;

                            if (users.Any())
                            {
                                Data.TrnJournalVoucher newJournalVoucher = new Data.TrnJournalVoucher
                                {
                                    BranchId = currentBranchId,
                                    JVNumber = defaultJVNumber,
                                    JVDate = DateTime.Today,
                                    ManualJVNumber = "NA",
                                    Particulars = "NA",
                                    PreparedById = currentUserId,
                                    CheckedById = currentUserId,
                                    ApprovedById = currentUserId,
                                    Status = null,
                                    IsCancelled = false,
                                    IsPrinted = false,
                                    IsLocked = false,
                                    CreatedById = currentUserId,
                                    CreatedDateTime = DateTime.Now,
                                    UpdatedById = currentUserId,
                                    UpdatedDateTime = DateTime.Now
                                };

                                db.TrnJournalVouchers.InsertOnSubmit(newJournalVoucher);
                                db.SubmitChanges();

                                String newObject = auditTrail.GetObjectString(newJournalVoucher);
                                auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                return Request.CreateResponse(HttpStatusCode.OK, newJournalVoucher.Id);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No user found. Please setup more users for all transactions.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add journal voucher.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this journal voucher page.");
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
        // Save Journal Voucher
        // ====================
        [Authorize, HttpPut, Route("api/journalVoucher/save/{id}")]
        public HttpResponseMessage SaveJournalVoucher(Entities.TrnJournalVoucher objJournalVoucher, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var journalVoucher = from d in db.TrnJournalVouchers where d.Id == Convert.ToInt32(id) select d;
                    if (journalVoucher.Any())
                    {
                        if (!journalVoucher.FirstOrDefault().IsLocked)
                        {
                            String oldObject = auditTrail.GetObjectString(journalVoucher.FirstOrDefault());

                            var saveJournalVoucher = journalVoucher.FirstOrDefault();
                            saveJournalVoucher.JVDate = Convert.ToDateTime(objJournalVoucher.JVDate);
                            saveJournalVoucher.ManualJVNumber = objJournalVoucher.ManualJVNumber;
                            saveJournalVoucher.Particulars = objJournalVoucher.Particulars;
                            saveJournalVoucher.CheckedById = objJournalVoucher.CheckedById;
                            saveJournalVoucher.ApprovedById = objJournalVoucher.ApprovedById;
                            saveJournalVoucher.Status = objJournalVoucher.Status;
                            saveJournalVoucher.UpdatedById = currentUserId;
                            saveJournalVoucher.UpdatedDateTime = DateTime.Now;
                            db.SubmitChanges();

                            String newObject = auditTrail.GetObjectString(journalVoucher.FirstOrDefault());
                            auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Saving Error. These journal voucher details are already locked.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These journal voucher details are not found in the server.");
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
        // Lock Journal Voucher
        // ====================
        [Authorize, HttpPut, Route("api/journalVoucher/lock/{id}")]
        public HttpResponseMessage LockJournalVoucher(Entities.TrnJournalVoucher objJournalVoucher, String id)
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
                                    && d.SysForm.FormName.Equals("JournalVoucherDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var journalVoucher = from d in db.TrnJournalVouchers
                                                 where d.Id == Convert.ToInt32(id)
                                                 select d;

                            if (journalVoucher.Any())
                            {
                                if (!journalVoucher.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = auditTrail.GetObjectString(journalVoucher.FirstOrDefault());

                                    Decimal totalJournalVoucherLineDebitAmount = journalVoucher.FirstOrDefault().TrnJournalVoucherLines.Sum(d => d.DebitAmount);
                                    Decimal totalJournalVoucherLineCreditAmount = journalVoucher.FirstOrDefault().TrnJournalVoucherLines.Sum(d => d.CreditAmount);
                                    Decimal variance = totalJournalVoucherLineDebitAmount - totalJournalVoucherLineCreditAmount;

                                    if (variance == 0)
                                    {
                                        var lockJournalVoucher = journalVoucher.FirstOrDefault();
                                        lockJournalVoucher.JVDate = Convert.ToDateTime(objJournalVoucher.JVDate);
                                        lockJournalVoucher.ManualJVNumber = objJournalVoucher.ManualJVNumber;
                                        lockJournalVoucher.Particulars = objJournalVoucher.Particulars;
                                        lockJournalVoucher.CheckedById = objJournalVoucher.CheckedById;
                                        lockJournalVoucher.ApprovedById = objJournalVoucher.ApprovedById;
                                        lockJournalVoucher.Status = objJournalVoucher.Status;
                                        lockJournalVoucher.IsLocked = true;
                                        lockJournalVoucher.UpdatedById = currentUserId;
                                        lockJournalVoucher.UpdatedDateTime = DateTime.Now;
                                        db.SubmitChanges();

                                        if (lockJournalVoucher.IsLocked)
                                        {
                                            var journalVoucherLines = from d in db.TrnJournalVoucherLines where d.JVId == Convert.ToInt32(id) select d;
                                            if (journalVoucherLines.Any())
                                            {
                                                foreach (var journalVoucherLine in journalVoucherLines)
                                                {
                                                    if (journalVoucherLine.APRRId != null) { accountsPayable.UpdateAccountsPayable(Convert.ToInt32(journalVoucherLine.APRRId)); }
                                                    if (journalVoucherLine.ARSIId != null) { accountsReceivable.UpdateAccountsReceivable(Convert.ToInt32(journalVoucherLine.ARSIId)); }
                                                }
                                            }

                                            journal.InsertJournalVoucherJournal(Convert.ToInt32(id));
                                        }

                                        String newObject = auditTrail.GetObjectString(journalVoucher.FirstOrDefault());
                                        auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Not Balance.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These journal voucher details are already locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These journal voucher details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock journal voucher.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this journal voucher page.");
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
        // Unlock Journal Voucher
        // ======================
        [Authorize, HttpPut, Route("api/journalVoucher/unlock/{id}")]
        public HttpResponseMessage UnlockJournalVoucher(String id)
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
                                    && d.SysForm.FormName.Equals("JournalVoucherDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var journalVoucher = from d in db.TrnJournalVouchers
                                                 where d.Id == Convert.ToInt32(id)
                                                 select d;

                            if (journalVoucher.Any())
                            {
                                if (!journalVoucher.FirstOrDefault().IsCancelled)
                                {
                                    if (journalVoucher.FirstOrDefault().IsLocked)
                                    {
                                        String oldObject = auditTrail.GetObjectString(journalVoucher.FirstOrDefault());

                                        var unlockJournalVoucher = journalVoucher.FirstOrDefault();
                                        unlockJournalVoucher.IsLocked = false;
                                        unlockJournalVoucher.UpdatedById = currentUserId;
                                        unlockJournalVoucher.UpdatedDateTime = DateTime.Now;
                                        db.SubmitChanges();

                                        if (!unlockJournalVoucher.IsLocked)
                                        {
                                            var journalVoucherLines = from d in db.TrnJournalVoucherLines where d.JVId == Convert.ToInt32(id) select d;
                                            if (journalVoucherLines.Any())
                                            {
                                                foreach (var journalVoucherLine in journalVoucherLines)
                                                {
                                                    if (journalVoucherLine.APRRId != null) { accountsPayable.UpdateAccountsPayable(Convert.ToInt32(journalVoucherLine.APRRId)); }
                                                    if (journalVoucherLine.ARSIId != null) { accountsReceivable.UpdateAccountsReceivable(Convert.ToInt32(journalVoucherLine.ARSIId)); }
                                                }
                                            }

                                            journal.DeleteJournalVoucherJournal(Convert.ToInt32(id));
                                        }

                                        String newObject = auditTrail.GetObjectString(journalVoucher.FirstOrDefault());
                                        auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These journal voucher details are already unlocked.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These journal voucher details are already cancelled.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These journal voucher details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock journal voucher.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this journal voucher page.");
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
        // Cancel Journal Voucher
        // ======================
        [Authorize, HttpPut, Route("api/journalVoucher/cancel/{id}")]
        public HttpResponseMessage CancelJournalVoucher(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms where d.UserId == currentUserId && d.SysForm.FormName.Equals("JournalVoucherDetail") select d;
                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanCancel)
                        {
                            var journalVoucher = from d in db.TrnJournalVouchers where d.Id == Convert.ToInt32(id) select d;
                            if (journalVoucher.Any())
                            {
                                if (journalVoucher.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = auditTrail.GetObjectString(journalVoucher.FirstOrDefault());

                                    var cancelJournalVoucher = journalVoucher.FirstOrDefault();
                                    cancelJournalVoucher.IsCancelled = true;
                                    cancelJournalVoucher.UpdatedById = currentUserId;
                                    cancelJournalVoucher.UpdatedDateTime = DateTime.Now;
                                    db.SubmitChanges();

                                    if (cancelJournalVoucher.IsCancelled)
                                    {
                                        var journalVoucherLines = from d in db.TrnJournalVoucherLines where d.JVId == Convert.ToInt32(id) select d;
                                        if (journalVoucherLines.Any())
                                        {
                                            foreach (var journalVoucherLine in journalVoucherLines)
                                            {
                                                if (journalVoucherLine.APRRId != null) { accountsPayable.UpdateAccountsPayable(Convert.ToInt32(journalVoucherLine.APRRId)); }
                                                if (journalVoucherLine.ARSIId != null) { accountsReceivable.UpdateAccountsReceivable(Convert.ToInt32(journalVoucherLine.ARSIId)); }

                                                journalVoucherLine.DebitAmount = 0;
                                                journalVoucherLine.CreditAmount = 0;
                                            }

                                            db.SubmitChanges();
                                        }

                                        journal.CancelJournal(Convert.ToInt32(id), "JournalVoucher");
                                    }

                                    String newObject = auditTrail.GetObjectString(journalVoucher.FirstOrDefault());
                                    auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Cancel Error. Cannot cancel journal voucher detail if journal voucher detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These journal voucher details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to cancel journal voucher.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this journal voucher page.");
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
        // Delete Journal Voucher
        // ======================
        [Authorize, HttpDelete, Route("api/journalVoucher/delete/{id}")]
        public HttpResponseMessage DeleteJournalVoucher(String id)
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
                                    && d.SysForm.FormName.Equals("JournalVoucherList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var journalVoucher = from d in db.TrnJournalVouchers
                                                 where d.Id == Convert.ToInt32(id)
                                                 select d;

                            if (journalVoucher.Any())
                            {
                                if (!journalVoucher.FirstOrDefault().IsLocked)
                                {
                                    db.TrnJournalVouchers.DeleteOnSubmit(journalVoucher.First());

                                    String oldObject = auditTrail.GetObjectString(journalVoucher.FirstOrDefault());
                                    auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete journal voucher if the current journal voucher record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These journal voucher details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete journal voucher.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this journal voucher page.");
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
