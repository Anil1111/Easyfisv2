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
    public class ApiTrnStockWithdrawalController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =====================
        // List Stock Withdrawal
        // =====================
        [Authorize, HttpGet, Route("api/stockWithdrawal/list/{startDate}/{endDate}")]
        public List<Entities.TrnStockWithdrawal> ListStockWithdrawa(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var stockWiithdrawals = from d in db.TrnStockWithdrawals.OrderByDescending(d => d.Id)
                                    where d.BranchId == branchId
                                    && d.SWDate >= Convert.ToDateTime(startDate)
                                    && d.SWDate <= Convert.ToDateTime(endDate)
                                    select new Entities.TrnStockWithdrawal
                                    {
                                        Id = d.Id,
                                        SWNumber = d.SWNumber,
                                        SWDate = d.SWDate.ToShortDateString(),
                                        DocumentReference = d.DocumentReference,
                                        Remarks = d.Remarks,
                                        IsLocked = d.IsLocked,
                                        CreatedBy = d.MstUser2.FullName,
                                        CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                        UpdatedBy = d.MstUser4.FullName,
                                        UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                    };

            return stockWiithdrawals.ToList();
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/stockWithdrawal/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListStockWithdrawalBranch()
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // =============================================
        // Dropdown List - Sales Invovice Branch (Field)
        // =============================================
        [Authorize, HttpGet, Route("api/stockWithdrawal/dropdown/list/salesInvoice/branch")]
        public List<Entities.MstBranch> DropdownListStockWithdrawalSalesInvoiceBranch()
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // ======================================
        // Dropdown List - Sales Invovice (Field)
        // ======================================
        [Authorize, HttpGet, Route("api/stockWithdrawal/dropdown/list/salesInvoice/{branchId}")]
        public List<Entities.TrnSalesInvoice> DropdownListStockWithdrawalSalesInvoice(String branchId)
        {
            var salesInvoices = from d in db.TrnSalesInvoices.OrderByDescending(d => d.SINumber)
                                where d.BranchId == Convert.ToInt32(branchId)
                                && d.BalanceAmount > 0
                                && d.IsLocked == true
                                select new Entities.TrnSalesInvoice
                                {
                                    Id = d.Id,
                                    SINumber = d.SINumber,
                                    SIDate = d.SIDate.ToShortDateString(),
                                    Remarks = d.Remarks
                                };

            return salesInvoices.ToList();
        }

        // ============================
        // Dropdown List - User (Field)
        // ============================
        [Authorize, HttpGet, Route("api/stockWithdrawal/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListStockWithdrawalUsers()
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

        // ====================
        // Add Stock Withdrawal
        // ====================
        [Authorize, HttpPost, Route("api/stockWithdrawal/add")]
        public HttpResponseMessage AddStockWithdrawal()
        {
            try
            {
                String returnMessage = "";

                Boolean currentUserExists = false;
                Int32 currentUserId = 0, currentBranchId = 0;
                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                if (currentUser.Any())
                {
                    currentUserExists = true;
                    currentUserId = currentUser.FirstOrDefault().Id;
                    currentBranchId = currentUser.FirstOrDefault().BranchId;
                }
                else
                {
                    returnMessage = "Theres no current user logged in";
                }

                Boolean userFormExists = false;
                Boolean canAdd = false;
                var userForms = from d in db.MstUserForms where d.UserId == currentUserId && d.SysForm.FormName.Equals("StockWithdrawalList") select d;
                if (userForms.Any())
                {
                    userFormExists = true;
                    if (userForms.FirstOrDefault().CanAdd)
                    {
                        canAdd = true;
                    }
                    else
                    {
                        returnMessage = "Sorry. You have no rights to add stock withdrawal.";
                    }
                }
                else
                {
                    returnMessage = "Sorry. You have no access for this stock in page..";
                }

                Boolean salesInvoiceExists = false;
                var salesInvoices = from d in db.TrnSalesInvoices.OrderByDescending(d => d.SINumber) where d.BranchId == Convert.ToInt32(currentBranchId) && d.BalanceAmount > 0 && d.IsLocked == true select d;
                if (salesInvoices.Any())
                {
                    salesInvoiceExists = true;
                }
                else
                {
                    returnMessage = "No sales invoice found. Please setup more sales invoices.";
                }

                Boolean userExists = false;
                var users = from d in db.MstUsers.OrderBy(d => d.FullName) where d.IsLocked == true select d;
                if (users.Any())
                {
                    userExists = true;
                }
                else
                {
                    returnMessage = "No user found. Please setup more users.";
                }

                Boolean isValid = false;

                if (currentUserExists)
                {
                    if (userFormExists)
                    {
                        if (canAdd)
                        {
                            if (salesInvoiceExists)
                            {
                                if (userExists)
                                {
                                    isValid = true;
                                }
                            }
                        }
                    }
                }

                if (isValid)
                {
                    var defaultSWNumber = "0000000001";
                    var lastStockWithdrawals = from d in db.TrnStockWithdrawals.OrderByDescending(d => d.Id) where d.BranchId == currentUser.FirstOrDefault().BranchId select d;
                    if (lastStockWithdrawals.Any())
                    {
                        var SWNumber = Convert.ToInt32(lastStockWithdrawals.FirstOrDefault().SWNumber) + 0000000001;
                        defaultSWNumber = FillLeadingZeroes(SWNumber, 10);
                    }

                    Data.TrnStockIn newStockIn = new Data.TrnStockIn
                    {
                        BranchId = currentBranchId,

                        PreparedById = currentUserId,
                        CheckedById = currentUserId,
                        ApprovedById = currentUserId,
                        IsLocked = false,
                        CreatedById = currentUserId,
                        CreatedDateTime = DateTime.Now,
                        UpdatedById = currentUserId,
                        UpdatedDateTime = DateTime.Now
                    };

                    db.TrnStockIns.InsertOnSubmit(newStockIn);
                    db.SubmitChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, newStockIn.Id);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, returnMessage);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

    }
}
