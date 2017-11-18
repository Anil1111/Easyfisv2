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
    public class ApiTrnDisbursementController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =================
        // List Disbursement
        // =================
        [Authorize, HttpGet, Route("api/disbursement/list/{startDate}/{endDate}")]
        public List<Entities.TrnDisbursement> ListDisbursement(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var disbursements = from d in db.TrnDisbursements.OrderByDescending(d => d.Id)
                                where d.BranchId == branchId
                                && d.CVDate >= Convert.ToDateTime(startDate)
                                && d.CVDate <= Convert.ToDateTime(endDate)
                                select new Entities.TrnDisbursement
                                {
                                    Id = d.Id,
                                    CVNumber = d.CVNumber,
                                    CVDate = d.CVDate.ToShortDateString(),
                                    Supplier = d.MstArticle.Article,
                                    Particulars = d.Particulars,
                                    CheckNumber = d.CheckNumber,
                                    Amount = d.Amount,
                                    IsLocked = d.IsLocked,
                                    CreatedBy = d.MstUser2.FullName,
                                    CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                    UpdatedBy = d.MstUser4.FullName,
                                    UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                };

            return disbursements.ToList();
        }

        // ===================
        // Detail Disbursement
        // ===================
        [Authorize, HttpGet, Route("api/disbursement/detail/{id}")]
        public Entities.TrnDisbursement DetailDisbursement(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var disbursement = from d in db.TrnDisbursements.OrderByDescending(d => d.Id)
                               where d.BranchId == branchId
                               && d.Id == Convert.ToInt32(id)
                               select new Entities.TrnDisbursement
                               {
                                   Id = d.Id,
                                   BranchId = d.BranchId,
                                   CVNumber = d.CVNumber,
                                   CVDate = d.CVDate.ToShortDateString(),
                                   ManualCVNumber = d.ManualCVNumber,
                                   SupplierId = d.SupplierId,
                                   Payee = d.Payee,
                                   PayTypeId = d.PayTypeId,
                                   Particulars = d.Particulars,
                                   BankId = d.BankId,
                                   CheckNumber = d.CheckNumber,
                                   CheckDate = d.CheckDate.ToShortDateString(),
                                   Amount = d.Amount,
                                   IsCrossCheck = d.IsCrossCheck,
                                   PreparedById = d.PreparedById,
                                   CheckedById = d.CheckedById,
                                   ApprovedById = d.ApprovedById,
                                   IsClear = d.IsClear,
                                   IsLocked = d.IsLocked,
                                   CreatedBy = d.MstUser2.FullName,
                                   CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                   UpdatedBy = d.MstUser4.FullName,
                                   UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                               };

            if (disbursement.Any())
            {
                return disbursement.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/disbursement/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListDisbursementBranch()
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
        [Authorize, HttpGet, Route("api/disbursement/dropdown/list/supplier")]
        public List<Entities.MstArticle> DropdownListDisbursementSupplier()
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

        // ================================
        // Dropdown List - Pay Type (Field)
        // ================================
        [Authorize, HttpGet, Route("api/disbursement/dropdown/list/payType")]
        public List<Entities.MstPayType> DropdownListDisbursementPayType()
        {
            var payTypes = from d in db.MstPayTypes.OrderBy(d => d.PayType)
                           where d.IsLocked == true
                           select new Entities.MstPayType
                           {
                               Id = d.Id,
                               PayType = d.PayType
                           };

            return payTypes.ToList();
        }

        // ============================
        // Dropdown List - Bank (Field)
        // ============================
        [Authorize, HttpGet, Route("api/disbursement/dropdown/list/bank")]
        public List<Entities.MstArticle> DropdownListDisbursementBank()
        {
            var banks = from d in db.MstArticles.OrderBy(d => d.Article)
                        where d.ArticleTypeId == 5
                        && d.IsLocked == true
                        select new Entities.MstArticle
                        {
                            Id = d.Id,
                            Article = d.Article
                        };

            return banks.ToList();
        }

        // ============================
        // Dropdown List - User (Field)
        // ============================
        [Authorize, HttpGet, Route("api/disbursement/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListDisbursementUsers()
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

        // ================
        // Add Disbursement
        // ================
        [Authorize, HttpPost, Route("api/disbursement/add")]
        public HttpResponseMessage AddDisbursement()
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
                                    && d.SysForm.FormName.Equals("DisbursementList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultCVNumber = "0000000001";
                            var lastDisbursement = from d in db.TrnDisbursements.OrderByDescending(d => d.Id)
                                                   where d.BranchId == currentBranchId
                                                   select d;

                            if (lastDisbursement.Any())
                            {
                                var CVNumber = Convert.ToInt32(lastDisbursement.FirstOrDefault().CVNumber) + 0000000001;
                                defaultCVNumber = FillLeadingZeroes(CVNumber, 10);
                            }

                            var suppliers = from d in db.MstArticles.OrderBy(d => d.Article)
                                            where d.ArticleTypeId == 3
                                            && d.IsLocked == true
                                            select d;

                            if (suppliers.Any())
                            {
                                var payTypes = from d in db.MstPayTypes.OrderBy(d => d.PayType)
                                               where d.IsLocked == true
                                               select d;

                                if (payTypes.Any())
                                {
                                    var banks = from d in db.MstArticles.OrderBy(d => d.Article)
                                                where d.ArticleTypeId == 5
                                                && d.IsLocked == true
                                                select d;

                                    if (banks.Any())
                                    {
                                        var users = from d in db.MstUsers.OrderBy(d => d.FullName)
                                                    where d.IsLocked == true
                                                    select d;

                                        if (users.Any())
                                        {
                                            Data.TrnDisbursement newDisbursement = new Data.TrnDisbursement
                                            {
                                                BranchId = currentBranchId,
                                                CVNumber = defaultCVNumber,
                                                CVDate = DateTime.Today,
                                                ManualCVNumber = "NA",
                                                Payee = "NA",
                                                SupplierId = suppliers.FirstOrDefault().Id,
                                                PayTypeId = payTypes.FirstOrDefault().Id,
                                                Particulars = "NA",
                                                BankId = banks.FirstOrDefault().Id,
                                                CheckNumber = "NA",
                                                CheckDate = DateTime.Today,
                                                Amount = 0,
                                                IsCrossCheck = false,
                                                PreparedById = currentUserId,
                                                CheckedById = currentUserId,
                                                ApprovedById = currentUserId,
                                                IsClear = false,
                                                IsLocked = false,
                                                CreatedById = currentUserId,
                                                CreatedDateTime = DateTime.Now,
                                                UpdatedById = currentUserId,
                                                UpdatedDateTime = DateTime.Now
                                            };

                                            db.TrnDisbursements.InsertOnSubmit(newDisbursement);
                                            db.SubmitChanges();

                                            return Request.CreateResponse(HttpStatusCode.OK, newDisbursement.Id);
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.NotFound, "No user found. Please setup more users for all transactions.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "No bank found. Please setup more banks for all transactions.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.NotFound, "No pay type found. Please setup more pay types for all transactions.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No supplier found. Please setup more suppliers for all transactions.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add disbursement.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this disbursement page.");
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
        // Update Accounts Payable
        // =======================
        public void UpdateAccountsPayable(Int32 CVId)
        {
            var disbursementLines = from d in db.TrnDisbursementLines
                                    where d.CVId == CVId
                                    && d.TrnDisbursement.IsLocked == true
                                    select d;

            if (disbursementLines.Any())
            {
                foreach (var disbursementLine in disbursementLines)
                {
                    if (disbursementLine.RRId != null)
                    {
                        Decimal paidAmount = 0;

                        var disbursementLinesAmount = from d in db.TrnDisbursementLines
                                                      where d.RRId == disbursementLine.RRId
                                                      && d.TrnDisbursement.IsLocked == true
                                                      select d;

                        if (disbursementLinesAmount.Any())
                        {
                            paidAmount = disbursementLinesAmount.Sum(d => d.Amount);
                        }

                        Decimal adjustmentAmount = 0;

                        var journalVoucherLines = from d in db.TrnJournalVoucherLines
                                                  where d.APRRId == disbursementLine.RRId
                                                  && d.TrnJournalVoucher.IsLocked == true
                                                  select d;

                        if (journalVoucherLines.Any())
                        {
                            Decimal debitAmount = journalVoucherLines.Sum(d => d.DebitAmount);
                            Decimal creditAmount = journalVoucherLines.Sum(d => d.CreditAmount);

                            adjustmentAmount = creditAmount - debitAmount;
                        }

                        var receivingReceipt = from d in db.TrnReceivingReceipts
                                               where d.Id == disbursementLine.RRId
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
                }
            }
        }

        // =======================
        // Get Disbursement Amount
        // =======================
        public Decimal GetDisbursementAmount(Int32 CVId)
        {
            var disbursementLines = from d in db.TrnDisbursementLines
                                    where d.CVId == CVId
                                    select d;

            if (disbursementLines.Any())
            {
                return disbursementLines.Sum(d => d.Amount);
            }
            else
            {
                return 0;
            }
        }

        // =================
        // Lock Disbursement
        // =================
        [Authorize, HttpPut, Route("api/disbursement/lock/{id}")]
        public HttpResponseMessage LockDisbursement(Entities.TrnDisbursement objDisbursement, String id)
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
                                    && d.SysForm.FormName.Equals("DisbursementDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var disbursement = from d in db.TrnDisbursements
                                               where d.Id == Convert.ToInt32(id)
                                               select d;

                            if (disbursement.Any())
                            {
                                if (!disbursement.FirstOrDefault().IsLocked)
                                {
                                    var lockDisbursement = disbursement.FirstOrDefault();
                                    lockDisbursement.CVDate = Convert.ToDateTime(objDisbursement.CVDate);
                                    lockDisbursement.SupplierId = objDisbursement.SupplierId;
                                    lockDisbursement.ManualCVNumber = objDisbursement.ManualCVNumber;
                                    lockDisbursement.Payee = objDisbursement.Payee;
                                    lockDisbursement.PayTypeId = objDisbursement.PayTypeId;
                                    lockDisbursement.Particulars = objDisbursement.Particulars;
                                    lockDisbursement.BankId = objDisbursement.BankId;
                                    lockDisbursement.CheckNumber = objDisbursement.CheckNumber;
                                    lockDisbursement.CheckDate = Convert.ToDateTime(objDisbursement.CheckDate);
                                    lockDisbursement.Amount = GetDisbursementAmount(Convert.ToInt32(id));
                                    lockDisbursement.IsCrossCheck = objDisbursement.IsCrossCheck;
                                    lockDisbursement.CheckedById = objDisbursement.CheckedById;
                                    lockDisbursement.ApprovedById = objDisbursement.ApprovedById;
                                    lockDisbursement.IsClear = objDisbursement.IsClear;
                                    lockDisbursement.IsLocked = true;
                                    lockDisbursement.UpdatedById = currentUserId;
                                    lockDisbursement.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    // ============================
                                    // Journal and Accounts Payable
                                    // ============================
                                    Business.Journal journal = new Business.Journal();

                                    if (lockDisbursement.IsLocked)
                                    {
                                        journal.insertCVJournal(Convert.ToInt32(id));
                                        UpdateAccountsPayable(Convert.ToInt32(id));
                                    }

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These disbursement details are already locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These disbursement details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock disbursement.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this disbursement page.");
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
        // Unlock Disbursement
        // ===================
        [Authorize, HttpPut, Route("api/disbursement/unlock/{id}")]
        public HttpResponseMessage UnlockDisbursement(String id)
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
                                    && d.SysForm.FormName.Equals("DisbursementDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var disbursement = from d in db.TrnDisbursements
                                               where d.Id == Convert.ToInt32(id)
                                               select d;

                            if (disbursement.Any())
                            {
                                if (disbursement.FirstOrDefault().IsLocked)
                                {
                                    var unlockDisbursement = disbursement.FirstOrDefault();
                                    unlockDisbursement.IsLocked = false;
                                    unlockDisbursement.UpdatedById = currentUserId;
                                    unlockDisbursement.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    // ============================
                                    // Journal and Accounts Payable
                                    // ============================
                                    Business.Journal journal = new Business.Journal();

                                    if (!unlockDisbursement.IsLocked)
                                    {
                                        journal.deleteCVJournal(Convert.ToInt32(id));
                                        UpdateAccountsPayable(Convert.ToInt32(id));
                                    }

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These disbursement details are already unlocked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These disbursement details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock disbursement.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this disbursement page.");
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
        // Delete Disbursement
        // ===================
        [Authorize, HttpDelete, Route("api/disbursement/delete/{id}")]
        public HttpResponseMessage DeleteDisbursement(String id)
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
                                    && d.SysForm.FormName.Equals("DisbursementList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var disbursement = from d in db.TrnDisbursements
                                               where d.Id == Convert.ToInt32(id)
                                               select d;

                            if (disbursement.Any())
                            {
                                if (!disbursement.FirstOrDefault().IsLocked)
                                {
                                    db.TrnDisbursements.DeleteOnSubmit(disbursement.First());
                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete disbursement if the current disbursement record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These disbursement details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete disbursement.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this disbursement page.");
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
