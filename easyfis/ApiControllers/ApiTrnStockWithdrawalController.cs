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
    public class ApiTrnStockWithdrawalController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

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
                                        ManualSWNumber = d.ManualSWNumber,
                                        DocumentReference = d.DocumentReference,
                                        SIBranch = d.TrnSalesInvoice.MstBranch.Branch,
                                        SINumber = d.TrnSalesInvoice.SINumber,
                                        Remarks = d.Remarks,
                                        IsLocked = d.IsLocked,
                                        CreatedBy = d.MstUser2.FullName,
                                        CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                        UpdatedBy = d.MstUser4.FullName,
                                        UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                    };

            return stockWiithdrawals.ToList();
        }

        // =======================
        // Detail Stock Withdrawal
        // =======================
        [Authorize, HttpGet, Route("api/stockWithdrawal/detail/{id}")]
        public Entities.TrnStockWithdrawal DetailStockWithdrawal(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var stockWithdrawal = from d in db.TrnStockWithdrawals
                                  where d.BranchId == branchId
                                  && d.Id == Convert.ToInt32(id)
                                  select new Entities.TrnStockWithdrawal
                                  {
                                      Id = d.Id,
                                      BranchId = d.BranchId,
                                      SWNumber = d.SWNumber,
                                      SWDate = d.SWDate.ToShortDateString(),
                                      DocumentReference = d.DocumentReference,
                                      ManualSWNumber = d.ManualSWNumber,
                                      CustomerId = d.CustomerId,
                                      SIId = d.SIId,
                                      SIBranchId = d.SIBranchId,
                                      SIBranch = d.MstBranch1.Branch,
                                      SINumber = d.TrnSalesInvoice.SINumber,
                                      SIDate = d.TrnSalesInvoice.SIDate.ToShortDateString(),
                                      Remarks = d.Remarks,
                                      ContactPerson = d.ContactPerson,
                                      ContactNumber = d.ContactNumber,
                                      Address = d.Address,
                                      ReceivedById = d.ReceivedById,
                                      PreparedById = d.PreparedById,
                                      CheckedById = d.CheckedById,
                                      ApprovedById = d.ApprovedById,
                                      Status = d.Status,
                                      IsLocked = d.IsLocked,
                                      CreatedBy = d.MstUser2.FullName,
                                      CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                      UpdatedBy = d.MstUser4.FullName,
                                      UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                  };

            if (stockWithdrawal.Any())
            {
                return stockWithdrawal.FirstOrDefault();
            }
            else
            {
                return null;
            }
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

        // ================================
        // Dropdown List - Customer (Field)
        // ================================
        [Authorize, HttpGet, Route("api/stockWithdrawal/dropdown/list/customer")]
        public List<Entities.MstArticle> DropdownListStockWithdrawalCustomer()
        {
            var customers = from d in db.MstArticles.OrderBy(d => d.Article)
                            where d.ArticleTypeId == 2
                            && d.IsLocked == true
                            select new Entities.MstArticle
                            {
                                Id = d.Id,
                                Article = d.Article,
                                ContactNumber = d.ContactNumber,
                                Address = d.Address
                            };

            return customers.ToList();
        }

        // =============================================
        // Dropdown List - Sales Invovice Branch (Field)
        // =============================================
        [Authorize, HttpGet, Route("api/stockWithdrawal/dropdown/list/salesInvoice/branch")]
        public List<Entities.MstBranch> DropdownListStockWithdrawalSalesInvoiceBranch()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var companyId = currentUser.FirstOrDefault().CompanyId;

            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           where d.CompanyId == companyId
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
        [Authorize, HttpGet, Route("api/stockWithdrawal/dropdown/list/salesInvoice/{branchId}/{customerId}")]
        public List<Entities.TrnSalesInvoice> DropdownListStockWithdrawalSalesInvoice(String branchId, String customerId)
        {
            var salesInvoices = from d in db.TrnSalesInvoices.OrderByDescending(d => d.SINumber)
                                where d.BranchId == Convert.ToInt32(branchId)
                                && d.CustomerId == Convert.ToInt32(customerId)
                                && d.IsLocked == true
                                select new Entities.TrnSalesInvoice
                                {
                                    Id = d.Id,
                                    SINumber = d.SINumber,
                                    SIDate = d.SIDate.ToShortDateString()
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

        // ==============================
        // Dropdown List - Status (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/stockWithdrawal/dropdown/list/status")]
        public List<Entities.MstStatus> DropdownListStockWithdrawalStatus()
        {
            var statuses = from d in db.MstStatus.OrderBy(d => d.Status)
                           where d.IsLocked == true
                           && d.Category.Equals("SW")
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
        // Add Stock Withdrawal
        // ====================
        [Authorize, HttpPost, Route("api/stockWithdrawal/add")]
        public HttpResponseMessage AddStockWithdrawal()
        {
            try
            {
                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                if (currentUser.Any())
                {
                    Int32 currentUserId = currentUser.FirstOrDefault().Id;
                    Int32 currentCompanyId = currentUser.FirstOrDefault().CompanyId;
                    Int32 currentBranchId = currentUser.FirstOrDefault().BranchId;

                    IQueryable<Data.MstUserForm> userForms = from d in db.MstUserForms where d.UserId == currentUserId && d.SysForm.FormName.Equals("StockWithdrawalList") select d;
                    IQueryable<Data.TrnSalesInvoice> salesInvoices = from d in db.TrnSalesInvoices.OrderByDescending(d => d.SINumber)
                                                                     where d.MstBranch.CompanyId == currentCompanyId
                                                                     && d.IsLocked == true
                                                                     select d;
                    IQueryable<Data.MstUser> users = from d in db.MstUsers.OrderBy(d => d.FullName) where d.IsLocked == true select d;

                    Boolean isValid = false;
                    String returnMessage = "";

                    if (!userForms.Any())
                    {
                        returnMessage = "Sorry. You have no access for this stock withdrawal page..";
                    }
                    else if (!userForms.FirstOrDefault().CanAdd)
                    {
                        returnMessage = "Sorry. You have no rights to add stock withdrawal.";
                    }
                    else if (!salesInvoices.Any())
                    {
                        returnMessage = "No sales invoice found. Please setup more sales invoices.";
                    }
                    else if (!users.Any())
                    {
                        returnMessage = "No user found. Please setup more users.";
                    }
                    else
                    {
                        isValid = true;
                    }

                    if (isValid)
                    {
                        var defaultSWNumber = "0000000001";
                        var lastStockWithdrawals = from d in db.TrnStockWithdrawals.OrderByDescending(d => d.Id)
                                                   where d.BranchId == currentBranchId
                                                   select d;

                        if (lastStockWithdrawals.Any())
                        {
                            var SWNumber = Convert.ToInt32(lastStockWithdrawals.FirstOrDefault().SWNumber) + 0000000001;
                            defaultSWNumber = FillLeadingZeroes(SWNumber, 10);
                        }

                        Data.TrnStockWithdrawal newStockWithdrawal = new Data.TrnStockWithdrawal
                        {
                            BranchId = currentBranchId,
                            SWNumber = defaultSWNumber,
                            SWDate = DateTime.Today,
                            SIBranchId = salesInvoices.FirstOrDefault().BranchId,
                            CustomerId = salesInvoices.FirstOrDefault().CustomerId,
                            SIId = salesInvoices.FirstOrDefault().Id,
                            Remarks = "NA",
                            DocumentReference = "NA",
                            ManualSWNumber = "NA",
                            ContactPerson = salesInvoices.FirstOrDefault().MstArticle.Article,
                            ContactNumber = salesInvoices.FirstOrDefault().MstArticle.ContactNumber,
                            Address = salesInvoices.FirstOrDefault().MstArticle.Address,
                            ReceivedById = currentUserId,
                            PreparedById = currentUserId,
                            CheckedById = currentUserId,
                            ApprovedById = currentUserId,
                            IsPrinted = false,
                            IsLocked = false,
                            CreatedById = currentUserId,
                            CreatedDateTime = DateTime.Now,
                            UpdatedById = currentUserId,
                            UpdatedDateTime = DateTime.Now
                        };

                        db.TrnStockWithdrawals.InsertOnSubmit(newStockWithdrawal);
                        db.SubmitChanges();

                        String newObject = at.GetObjectString(newStockWithdrawal);
                        at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                        return Request.CreateResponse(HttpStatusCode.OK, newStockWithdrawal.Id);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, returnMessage);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // =====================
        // Save Stock Withdrawal
        // =====================
        [Authorize, HttpPut, Route("api/stockWithdrawal/save/{id}")]
        public HttpResponseMessage SaveStockWithdrawal(Entities.TrnStockWithdrawal objStockWithdrawal, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                if (currentUser.Any())
                {
                    Int32 currentUserId = currentUser.FirstOrDefault().Id;
                    Int32 currentBranchId = currentUser.FirstOrDefault().BranchId;

                    IQueryable<Data.TrnSalesInvoice> salesInvoice = from d in db.TrnSalesInvoices where d.Id == objStockWithdrawal.SIId && d.BranchId == objStockWithdrawal.SIBranchId && d.IsLocked == true select d;
                    IQueryable<Data.MstUser> users = from d in db.MstUsers.OrderBy(d => d.FullName) where d.IsLocked == true select d;
                    IQueryable<Data.TrnStockWithdrawal> stockWithdrawal = from d in db.TrnStockWithdrawals where d.Id == Convert.ToInt32(id) select d;

                    Boolean isValid = false;
                    String returnMessage = "";

                    if (!salesInvoice.Any())
                    {
                        returnMessage = "No sales invoice found. Please setup more sales invoices.";
                    }
                    else if (!users.Any())
                    {
                        returnMessage = "No user found. Please setup more users.";
                    }
                    else if (!stockWithdrawal.Any())
                    {
                        returnMessage = "Data not found. These stock withdrawal details are not found in the server.";
                    }
                    else if (stockWithdrawal.FirstOrDefault().IsLocked)
                    {
                        returnMessage = "Saving Error. These stock withdrawal details are already locked.";
                    }
                    else
                    {
                        isValid = true;
                    }

                    if (isValid)
                    {
                        String oldObject = at.GetObjectString(stockWithdrawal.FirstOrDefault());

                        var saveStockWithdrawal = stockWithdrawal.FirstOrDefault();
                        saveStockWithdrawal.SWDate = Convert.ToDateTime(objStockWithdrawal.SWDate);
                        saveStockWithdrawal.CustomerId = objStockWithdrawal.CustomerId;
                        saveStockWithdrawal.SIBranchId = objStockWithdrawal.SIBranchId;
                        saveStockWithdrawal.SIId = objStockWithdrawal.SIId;
                        saveStockWithdrawal.Remarks = objStockWithdrawal.Remarks;
                        saveStockWithdrawal.DocumentReference = objStockWithdrawal.DocumentReference;
                        saveStockWithdrawal.ManualSWNumber = objStockWithdrawal.ManualSWNumber;
                        saveStockWithdrawal.ContactPerson = objStockWithdrawal.ContactPerson;
                        saveStockWithdrawal.ContactNumber = objStockWithdrawal.ContactNumber;
                        saveStockWithdrawal.Address = objStockWithdrawal.Address;
                        saveStockWithdrawal.ReceivedById = objStockWithdrawal.ReceivedById;
                        saveStockWithdrawal.PreparedById = objStockWithdrawal.PreparedById;
                        saveStockWithdrawal.CheckedById = objStockWithdrawal.CheckedById;
                        saveStockWithdrawal.ApprovedById = objStockWithdrawal.ApprovedById;
                        saveStockWithdrawal.Status = objStockWithdrawal.Status;
                        saveStockWithdrawal.UpdatedById = currentUserId;
                        saveStockWithdrawal.UpdatedDateTime = DateTime.Now;
                        db.SubmitChanges();

                        String newObject = at.GetObjectString(stockWithdrawal.FirstOrDefault());
                        at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, returnMessage);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // =====================
        // Lock Stock Withdrawal
        // =====================
        [Authorize, HttpPut, Route("api/stockWithdrawal/lock/{id}")]
        public HttpResponseMessage LockStockWithdrawal(Entities.TrnStockWithdrawal objStockWithdrawal, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                if (currentUser.Any())
                {
                    Int32 currentUserId = currentUser.FirstOrDefault().Id;
                    Int32 currentBranchId = currentUser.FirstOrDefault().BranchId;

                    IQueryable<Data.MstUserForm> userForms = from d in db.MstUserForms where d.UserId == currentUserId && d.SysForm.FormName.Equals("StockWithdrawalDetail") select d;
                    IQueryable<Data.TrnSalesInvoice> salesInvoice = from d in db.TrnSalesInvoices where d.Id == objStockWithdrawal.SIId && d.BranchId == objStockWithdrawal.SIBranchId && d.IsLocked == true select d;
                    IQueryable<Data.MstUser> users = from d in db.MstUsers.OrderBy(d => d.FullName) where d.IsLocked == true select d;
                    IQueryable<Data.TrnStockWithdrawal> stockWithdrawal = from d in db.TrnStockWithdrawals where d.Id == Convert.ToInt32(id) select d;

                    Boolean isValid = false;
                    String returnMessage = "";

                    if (!userForms.Any())
                    {
                        returnMessage = "Sorry. You have no access for this stock withdrawal page.";
                    }
                    else if (!userForms.FirstOrDefault().CanLock)
                    {
                        returnMessage = "Sorry. You have no rights to lock stock withdrawal.";
                    }
                    else if (!salesInvoice.Any())
                    {
                        returnMessage = "No sales invoice found. Please setup more sales invoices.";
                    }
                    else if (!users.Any())
                    {
                        returnMessage = "No user found. Please setup more users.";
                    }
                    else if (!stockWithdrawal.Any())
                    {
                        returnMessage = "Data not found. These stock withdrawal details are not found in the server.";
                    }
                    else if (stockWithdrawal.FirstOrDefault().IsLocked)
                    {
                        returnMessage = "Locking Error. These stock withdrawal details are already locked.";
                    }
                    else
                    {
                        isValid = true;
                    }

                    if (isValid)
                    {
                        String oldObject = at.GetObjectString(stockWithdrawal.FirstOrDefault());

                        var lockStockWithdrawal = stockWithdrawal.FirstOrDefault();
                        lockStockWithdrawal.SWDate = Convert.ToDateTime(objStockWithdrawal.SWDate);
                        lockStockWithdrawal.CustomerId = objStockWithdrawal.CustomerId;
                        lockStockWithdrawal.SIBranchId = objStockWithdrawal.SIBranchId;
                        lockStockWithdrawal.SIId = objStockWithdrawal.SIId;
                        lockStockWithdrawal.Remarks = objStockWithdrawal.Remarks;
                        lockStockWithdrawal.DocumentReference = objStockWithdrawal.DocumentReference;
                        lockStockWithdrawal.ManualSWNumber = objStockWithdrawal.ManualSWNumber;
                        lockStockWithdrawal.ContactPerson = objStockWithdrawal.ContactPerson;
                        lockStockWithdrawal.ContactNumber = objStockWithdrawal.ContactNumber;
                        lockStockWithdrawal.Address = objStockWithdrawal.Address;
                        lockStockWithdrawal.ReceivedById = objStockWithdrawal.ReceivedById;
                        lockStockWithdrawal.PreparedById = objStockWithdrawal.PreparedById;
                        lockStockWithdrawal.CheckedById = objStockWithdrawal.CheckedById;
                        lockStockWithdrawal.ApprovedById = objStockWithdrawal.ApprovedById;
                        lockStockWithdrawal.Status = objStockWithdrawal.Status;
                        lockStockWithdrawal.IsLocked = true;
                        lockStockWithdrawal.UpdatedById = currentUserId;
                        lockStockWithdrawal.UpdatedDateTime = DateTime.Now;
                        db.SubmitChanges();

                        if (currentBranchId != objStockWithdrawal.SIBranchId)
                        {
                            // =====================
                            // Journal and Inventory
                            // =====================
                            Business.Journal journal = new Business.Journal();
                            Business.Inventory inventory = new Business.Inventory();

                            if (lockStockWithdrawal.IsLocked)
                            {
                                journal.InsertStockWithdrawalJournal(Convert.ToInt32(id));
                                inventory.InsertStockWithdrawalInventory(Convert.ToInt32(id));
                            }
                        }

                        String newObject = at.GetObjectString(stockWithdrawal.FirstOrDefault());
                        at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, returnMessage);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // =======================
        // Unlock Stock Withdrawal
        // =======================
        [Authorize, HttpPut, Route("api/stockWithdrawal/unlock/{id}")]
        public HttpResponseMessage UnlockStockWithdrawal(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                if (currentUser.Any())
                {
                    Int32 currentUserId = currentUser.FirstOrDefault().Id;
                    Int32 currentBranchId = currentUser.FirstOrDefault().BranchId;

                    IQueryable<Data.MstUserForm> userForms = from d in db.MstUserForms where d.UserId == currentUserId && d.SysForm.FormName.Equals("StockWithdrawalDetail") select d;
                    IQueryable<Data.TrnStockWithdrawal> stockWithdrawal = from d in db.TrnStockWithdrawals where d.Id == Convert.ToInt32(id) select d;

                    Boolean isValid = false;
                    String returnMessage = "";

                    if (!userForms.Any())
                    {
                        returnMessage = "Sorry. You have no access for this stock withdrawal page..";
                    }
                    else if (!userForms.FirstOrDefault().CanUnlock)
                    {
                        returnMessage = "Sorry. You have no rights to unlock stock withdrawal.";
                    }
                    else if (!stockWithdrawal.Any())
                    {
                        returnMessage = "Data not found. These stock withdrawal details are not found in the server.";
                    }
                    else if (!stockWithdrawal.FirstOrDefault().IsLocked)
                    {
                        returnMessage = "Unlocking Error. These stock withdrawal details are already unlocked.";
                    }
                    else
                    {
                        isValid = true;
                    }

                    if (isValid)
                    {
                        String oldObject = at.GetObjectString(stockWithdrawal.FirstOrDefault());

                        var unlockStockWithdrawal = stockWithdrawal.FirstOrDefault();
                        unlockStockWithdrawal.IsLocked = false;
                        unlockStockWithdrawal.UpdatedById = currentUserId;
                        unlockStockWithdrawal.UpdatedDateTime = DateTime.Now;
                        db.SubmitChanges();

                        // =====================
                        // Journal and Inventory
                        // =====================
                        Business.Journal journal = new Business.Journal();
                        Business.Inventory inventory = new Business.Inventory();

                        if (!unlockStockWithdrawal.IsLocked)
                        {
                            journal.DeleteStockWithdrawalJournal(Convert.ToInt32(id));
                            inventory.DeleteStockWithdrawalInventory(Convert.ToInt32(id));
                        }

                        String newObject = at.GetObjectString(stockWithdrawal.FirstOrDefault());
                        at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, returnMessage);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // =======================
        // Delete Stock Withdrawal
        // =======================
        [Authorize, HttpDelete, Route("api/stockWithdrawal/delete/{id}")]
        public HttpResponseMessage DeleteStockWithdrawal(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                if (currentUser.Any())
                {
                    Int32 currentUserId = currentUser.FirstOrDefault().Id;
                    Int32 currentBranchId = currentUser.FirstOrDefault().BranchId;

                    IQueryable<Data.MstUserForm> userForms = from d in db.MstUserForms where d.UserId == currentUserId && d.SysForm.FormName.Equals("StockWithdrawalList") select d;
                    IQueryable<Data.TrnStockWithdrawal> stockWithdrawal = from d in db.TrnStockWithdrawals where d.Id == Convert.ToInt32(id) select d;

                    Boolean isValid = false;
                    String returnMessage = "";

                    if (!userForms.Any())
                    {
                        returnMessage = "Sorry. You have no access for this stock withdrawal page.";
                    }
                    else if (!userForms.FirstOrDefault().CanDelete)
                    {
                        returnMessage = "Sorry. You have no rights to delete stock withdrawal.";
                    }
                    else if (!stockWithdrawal.Any())
                    {
                        returnMessage = "Data not found. These stock withdrawal details are not found in the server.";
                    }
                    else if (stockWithdrawal.FirstOrDefault().IsLocked)
                    {
                        returnMessage = "Delete Error. You cannot delete stock withdrawal if the current stock withdrawal record is locked.";
                    }
                    else
                    {
                        isValid = true;
                    }

                    if (isValid)
                    {
                        db.TrnStockWithdrawals.DeleteOnSubmit(stockWithdrawal.First());

                        String oldObject = at.GetObjectString(stockWithdrawal.FirstOrDefault());
                        at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                        db.SubmitChanges();

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, returnMessage);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
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