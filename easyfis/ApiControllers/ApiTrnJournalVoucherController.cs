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

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

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
                                    IsLocked = false,
                                    CreatedById = currentUserId,
                                    CreatedDateTime = DateTime.Now,
                                    UpdatedById = currentUserId,
                                    UpdatedDateTime = DateTime.Now
                                };

                                db.TrnJournalVouchers.InsertOnSubmit(newJournalVoucher);
                                db.SubmitChanges();

                                String newObject = at.GetObjectString(newJournalVoucher);
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

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

        // =========================
        // Update AP and AR Balances
        // =========================
        public void UpdateBalances(Int32 JVId)
        {
            var journalVoucherLines = from d in db.TrnJournalVoucherLines
                                      where d.JVId == JVId
                                      select d;

            if (journalVoucherLines.Any())
            {
                foreach (var journalVoucherLine in journalVoucherLines)
                {
                    if (journalVoucherLine.APRRId != null)
                    {
                        Decimal paidAmount = 0;

                        var disbursementLines = from d in db.TrnDisbursementLines
                                                where d.RRId == journalVoucherLine.APRRId
                                                && d.TrnDisbursement.IsLocked == true
                                                select d;

                        if (disbursementLines.Any())
                        {
                            paidAmount = disbursementLines.Sum(d => d.Amount);
                        }

                        Decimal adjustmentAmount = 0;

                        var journalVoucherLinesAPAdjustments = from d in db.TrnJournalVoucherLines
                                                               where d.APRRId == journalVoucherLine.APRRId
                                                               && d.TrnJournalVoucher.IsLocked == true
                                                               select d;

                        if (journalVoucherLinesAPAdjustments.Any())
                        {
                            Decimal debitAmount = journalVoucherLinesAPAdjustments.Sum(d => d.DebitAmount);
                            Decimal creditAmount = journalVoucherLinesAPAdjustments.Sum(d => d.CreditAmount);

                            adjustmentAmount = creditAmount - debitAmount;
                        }

                        var receivingReceipt = from d in db.TrnReceivingReceipts
                                               where d.Id == journalVoucherLine.APRRId
                                               && d.IsLocked == true
                                               select d;

                        if (receivingReceipt.Any())
                        {
                            Decimal receivingReceiptAmount = receivingReceipt.FirstOrDefault().Amount;
                            Decimal receivingReceiptWTAXAmount = receivingReceipt.FirstOrDefault().WTaxAmount;
                            Decimal balanceAmount = (receivingReceiptAmount - receivingReceiptWTAXAmount - paidAmount) + adjustmentAmount;

                            var updateReceivingReceipt = receivingReceipt.FirstOrDefault();
                            updateReceivingReceipt.PaidAmount = paidAmount;
                            updateReceivingReceipt.AdjustmentAmount = adjustmentAmount;
                            updateReceivingReceipt.BalanceAmount = balanceAmount;
                            db.SubmitChanges();
                        }
                    }

                    if (journalVoucherLine.ARSIId != null)
                    {
                        Decimal paidAmount = 0;

                        var collectionLines = from d in db.TrnCollectionLines
                                              where d.SIId == journalVoucherLine.ARSIId
                                              && d.TrnCollection.IsLocked == true
                                              select d;

                        if (collectionLines.Any())
                        {
                            paidAmount = collectionLines.Sum(d => d.Amount);
                        }

                        Decimal adjustmentAmount = 0;

                        var journalVoucherLinesARAdjustments = from d in db.TrnJournalVoucherLines
                                                               where d.ARSIId == journalVoucherLine.ARSIId
                                                               && d.TrnJournalVoucher.IsLocked == true
                                                               select d;

                        if (journalVoucherLinesARAdjustments.Any())
                        {
                            Decimal debitAmount = journalVoucherLinesARAdjustments.Sum(d => d.DebitAmount);
                            Decimal creditAmount = journalVoucherLinesARAdjustments.Sum(d => d.CreditAmount);

                            adjustmentAmount = debitAmount - creditAmount;
                        }

                        var salesInvoice = from d in db.TrnSalesInvoices
                                           where d.Id == journalVoucherLine.ARSIId
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
                                    String oldObject = at.GetObjectString(journalVoucher.FirstOrDefault());

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
                                        lockJournalVoucher.IsLocked = true;
                                        lockJournalVoucher.UpdatedById = currentUserId;
                                        lockJournalVoucher.UpdatedDateTime = DateTime.Now;

                                        db.SubmitChanges();

                                        // =======
                                        // Journal
                                        // =======
                                        Business.Journal journal = new Business.Journal();

                                        if (lockJournalVoucher.IsLocked)
                                        {
                                            journal.InsertJournalVoucherJournal(Convert.ToInt32(id));
                                            UpdateBalances(Convert.ToInt32(id));
                                        }

                                        String newObject = at.GetObjectString(journalVoucher.FirstOrDefault());
                                        at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

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
                                if (journalVoucher.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = at.GetObjectString(journalVoucher.FirstOrDefault());

                                    var unlockJournalVoucher = journalVoucher.FirstOrDefault();
                                    unlockJournalVoucher.IsLocked = false;
                                    unlockJournalVoucher.UpdatedById = currentUserId;
                                    unlockJournalVoucher.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    // =======
                                    // Journal
                                    // =======
                                    Business.Journal journal = new Business.Journal();

                                    if (!unlockJournalVoucher.IsLocked)
                                    {
                                        journal.DeleteJournalVoucherJournal(Convert.ToInt32(id));
                                        UpdateBalances(Convert.ToInt32(id));
                                    }

                                    String newObject = at.GetObjectString(journalVoucher.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These journal voucher details are already unlocked.");
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

                                    String oldObject = at.GetObjectString(journalVoucher.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

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
