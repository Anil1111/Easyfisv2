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
    public class ApiSysManageController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===================
        // Current User Detail
        // ===================
        [Authorize, HttpGet, Route("api/manage/current/user/detail")]
        public Entities.MstUser ManageCurrentUserDetail()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var currentUserId = currentUser.FirstOrDefault().Id;

            var user = from d in db.MstUsers
                       where d.Id == currentUserId
                       select new Entities.MstUser
                       {
                           Id = d.Id,
                           FullName = d.FullName,
                           UserName = d.UserName,
                           UserId = d.UserId,
                           CompanyId = d.CompanyId,
                           BranchId = d.BranchId,
                           IncomeAccountId = d.IncomeAccountId,
                           SupplierAdvancesAccountId = d.SupplierAdvancesAccountId,
                           CustomerAdvancesAccountId = d.CustomerAdvancesAccountId,
                           OfficialReceiptName = d.OfficialReceiptName,
                           InventoryType = d.InventoryType,
                           DefaultSalesInvoiceDiscountId = d.DefaultSalesInvoiceDiscountId,
                           SalesInvoiceName = d.SalesInvoiceName,
                           IsIncludeCostStockReports = d.IsIncludeCostStockReports,
                           IsLocked = d.IsLocked,
                       };

            if (user.Any())
            {
                return user.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ===============================
        // Dropdown List - Company (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/manage/current/user/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListUserListCompany()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var currentUserId = currentUser.FirstOrDefault().Id;

            var companies = from d in db.MstUserBranches
                            where d.UserId == currentUserId
                            select new Entities.MstCompany
                            {
                                Id = d.Id,
                                Company = d.MstBranch.MstCompany.Company,
                            };

            return companies.ToList();
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/manage/current/user/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListUserBranch(String companyId)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var currentUserId = currentUser.FirstOrDefault().Id;

            var branches = from d in db.MstUserBranches
                           where d.UserId == currentUserId
                           && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.MstBranch.Branch
                           };

            return branches.ToList();
        }

        // ===============================
        // Dropdown List - Account (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/manage/current/user/dropdown/list/account")]
        public List<Entities.MstAccount> DropdownListUserAccount()
        {
            var accounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                           where d.IsLocked == true
                           select new Entities.MstAccount
                           {
                               Id = d.Id,
                               AccountCode = d.AccountCode,
                               Account = d.Account
                           };

            return accounts.ToList();
        }

        // ======================================
        // Dropdown List - Inventory Type (Field)
        // ======================================
        [Authorize, HttpGet, Route("api/manage/current/user/dropdown/list/inventoryType")]
        public List<Entities.SysInventoryType> DropdownListUserInventoryType()
        {
            String[] inventoryTypes = { "Specific Identification", "Moving Average" };
            Boolean isDisabled = false;

            var inventories = from d in db.TrnInventories
                              select d;

            if (inventories.Any())
            {
                isDisabled = true;
            }

            List<Entities.SysInventoryType> listInventoryType = new List<Entities.SysInventoryType>();
            for (Int32 i = 0; i < inventoryTypes.Length; i++)
            {
                listInventoryType.Add(new Entities.SysInventoryType()
                {
                    InventoryType = inventoryTypes[i],
                    IsDisabled = isDisabled
                });
            }

            return listInventoryType.ToList();
        }

        // ================================
        // Dropdown List - Discount (Field)
        // ================================
        [Authorize, HttpGet, Route("api/manage/current/user/dropdown/list/discount")]
        public List<Entities.MstDiscount> DropdownListUserDiscount()
        {
            var discounts = from d in db.MstDiscounts.OrderBy(d => d.Discount)
                            where d.IsLocked == true
                            select new Entities.MstDiscount
                            {
                                Id = d.Id,
                                Discount = d.Discount
                            };

            return discounts.ToList();
        }

        // ======================
        // List Current User Form
        // ======================
        [Authorize, HttpGet, Route("api/manage/current/user/userForm/list")]
        public List<Entities.MstUserForm> ManageCurrentUserFormList()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var currentUserId = currentUser.FirstOrDefault().Id;

            var userForms = from d in db.MstUserForms
                            where d.UserId == currentUserId
                            select new Entities.MstUserForm
                            {
                                Id = d.Id,
                                UserId = d.UserId,
                                FormId = d.FormId,
                                Form = d.SysForm.Particulars,
                                CanAdd = d.CanAdd,
                                CanEdit = d.CanEdit,
                                CanDelete = d.CanDelete,
                                CanLock = d.CanLock,
                                CanUnlock = d.CanUnlock,
                                CanPrint = d.CanPrint
                            };

            return userForms.ToList();
        }

        // ========================
        // List Current User Branch
        // ========================
        [Authorize, HttpGet, Route("api/manage/current/user/userBranch/list")]
        public List<Entities.MstUserBranch> ManageCurrentUserBranchList()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var currentUserId = currentUser.FirstOrDefault().Id;

            var userBranches = from d in db.MstUserBranches
                               where d.UserId == currentUserId
                               select new Entities.MstUserBranch
                               {
                                   Id = d.Id,
                                   UserId = d.UserId,
                                   CompanyId = d.MstBranch.CompanyId,
                                   Company = d.MstBranch.MstCompany.Company,
                                   BranchId = d.BranchId,
                                   Branch = d.MstBranch.Branch
                               };

            return userBranches.ToList();
        }

        // ===================
        // Update Current User
        // ===================
        [Authorize, HttpPut, Route("api/manage/current/user/update")]
        public HttpResponseMessage UpdateCurrentUser(Entities.MstUser objUser)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currenteAspNetUserId = User.Identity.GetUserId();
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var user = from d in db.MstUsers
                               where d.Id == currentUserId
                               select d;

                    if (user.Any())
                    {
                        if (!user.FirstOrDefault().IsLocked)
                        {
                            var branch = from d in db.MstBranches
                                         where d.Id == objUser.BranchId
                                         select d;

                            if (branch.Any())
                            {
                                var account = from d in db.MstAccounts
                                              select d;

                                if (account.Any())
                                {
                                    var discounts = from d in db.MstDiscounts
                                                    where d.Id == objUser.DefaultSalesInvoiceDiscountId
                                                    select d;

                                    if (discounts.Any())
                                    {
                                        var currentASPNetUser = from d in db.AspNetUsers
                                                                where d.Id == currenteAspNetUserId
                                                                select d;

                                        if (currentASPNetUser.Any())
                                        {
                                            var updateCurrentASPNetUser = currentASPNetUser.FirstOrDefault();
                                            updateCurrentASPNetUser.FullName = objUser.FullName;
                                            db.SubmitChanges();

                                            var updateCurrentUser = user.FirstOrDefault();
                                            updateCurrentUser.FullName = objUser.FullName;
                                            updateCurrentUser.CompanyId = objUser.CompanyId;
                                            updateCurrentUser.BranchId = objUser.BranchId;
                                            updateCurrentUser.IncomeAccountId = objUser.IncomeAccountId;
                                            updateCurrentUser.SupplierAdvancesAccountId = objUser.SupplierAdvancesAccountId;
                                            updateCurrentUser.CustomerAdvancesAccountId = objUser.CustomerAdvancesAccountId;
                                            updateCurrentUser.OfficialReceiptName = objUser.OfficialReceiptName;
                                            updateCurrentUser.InventoryType = objUser.InventoryType;
                                            updateCurrentUser.DefaultSalesInvoiceDiscountId = objUser.DefaultSalesInvoiceDiscountId;
                                            updateCurrentUser.SalesInvoiceName = objUser.SalesInvoiceName;
                                            updateCurrentUser.IsIncludeCostStockReports = objUser.IsIncludeCostStockReports;
                                            updateCurrentUser.UpdatedById = currentUserId;
                                            updateCurrentUser.UpdatedDateTime = DateTime.Now;
                                            db.SubmitChanges();

                                            return Request.CreateResponse(HttpStatusCode.OK);
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.NotFound, "Current user not found.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "Sales Invoice Discount not found.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.NotFound, "Some Account data not found.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Branch not found.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These user details are already locked.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These user details are not found in the server.");
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
