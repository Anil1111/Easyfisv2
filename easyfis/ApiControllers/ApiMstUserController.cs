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
    public class ApiMstUserController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

        // ==================
        // Get User Full Name
        // ==================
        public String GetCurrentUserFullName(Int32? id)
        {
            if (id != null)
            {
                var mstUser = from d in db.MstUsers
                              where d.Id == id
                              select d;

                if (mstUser.Any())
                {
                    return mstUser.FirstOrDefault().FullName;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        // =========
        // List User
        // =========
        [Authorize, HttpGet, Route("api/user/list")]
        public List<Entities.MstUser> ListUser()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var currentUserName = currentUser.FirstOrDefault().UserName;

            if (currentUserName.Equals("admin"))
            {
                var users = from d in db.MstUsers
                            select new Entities.MstUser
                            {
                                Id = d.Id,
                                UserName = d.UserName,
                                FullName = d.FullName,
                                IsLocked = d.IsLocked,
                                CreatedBy = GetCurrentUserFullName(d.CreatedById),
                                CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                UpdatedBy = GetCurrentUserFullName(d.UpdatedById),
                                UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                            };

                return users.ToList();
            }
            else
            {
                var users = from d in db.MstUsers
                            where !d.UserName.Equals("admin")
                            select new Entities.MstUser
                            {
                                Id = d.Id,
                                UserName = d.UserName,
                                FullName = d.FullName,
                                IsLocked = d.IsLocked,
                                CreatedBy = GetCurrentUserFullName(d.CreatedById),
                                CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                UpdatedBy = GetCurrentUserFullName(d.UpdatedById),
                                UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                            };

                return users.ToList();
            }
        }

        // ===========
        // Detail User
        // ===========
        [Authorize, HttpGet, Route("api/user/detail/{id}")]
        public Entities.MstUser DetailUser(String id)
        {
            var user = from d in db.MstUsers
                       where d.Id == Convert.ToInt32(id)
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
                           InventoryType = d.InventoryType,
                           DefaultSalesInvoiceDiscountId = d.DefaultSalesInvoiceDiscountId,
                           SalesInvoiceName = d.SalesInvoiceName,
                           SalesInvoiceCheckedById = d.SalesInvoiceCheckedById,
                           SalesInvoiceApprovedById = d.SalesInvoiceApprovedById,
                           OfficialReceiptName = d.OfficialReceiptName,
                           IsIncludeCostStockReports = d.IsIncludeCostStockReports,
                           IsLocked = d.IsLocked,
                           CreatedBy = GetCurrentUserFullName(d.CreatedById),
                           CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                           UpdatedBy = GetCurrentUserFullName(d.UpdatedById),
                           UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
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
        [Authorize, HttpGet, Route("api/user/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListUserListCompany()
        {
            var companies = from d in db.MstCompanies.OrderBy(d => d.Company)
                            select new Entities.MstCompany
                            {
                                Id = d.Id,
                                Company = d.Company
                            };

            return companies.ToList();
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/user/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListUserBranch(String companyId)
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           where d.CompanyId == Convert.ToInt32(companyId)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // ===============================
        // Dropdown List - Account (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/user/dropdown/list/account")]
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
        [Authorize, HttpGet, Route("api/user/dropdown/list/inventoryType")]
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
        [Authorize, HttpGet, Route("api/user/dropdown/list/discount")]
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

        // =============================
        // Dropdown List - Users (Field)
        // =============================
        [Authorize, HttpGet, Route("api/user/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListUserListUsers()
        {
            var users = from d in db.MstUsers
                        select new Entities.MstUser
                        {
                            Id = d.Id,
                            FullName = d.FullName
                        };

            return users.ToList();
        }

        // =========
        // Lock User
        // =========
        [Authorize, HttpPut, Route("api/user/lock/{id}")]
        public HttpResponseMessage LockUser(Entities.MstUser objUser, String id)
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
                                    && d.SysForm.FormName.Equals("UserDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var user = from d in db.MstUsers
                                       where d.Id == Convert.ToInt32(id)
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
                                                String oldObject = at.GetObjectString(user.FirstOrDefault());

                                                var lockUser = user.FirstOrDefault();
                                                lockUser.FullName = objUser.FullName;
                                                lockUser.CompanyId = objUser.CompanyId;
                                                lockUser.BranchId = objUser.BranchId;
                                                lockUser.IncomeAccountId = objUser.IncomeAccountId;
                                                lockUser.SupplierAdvancesAccountId = objUser.SupplierAdvancesAccountId;
                                                lockUser.CustomerAdvancesAccountId = objUser.CustomerAdvancesAccountId;
                                                lockUser.InventoryType = objUser.InventoryType;
                                                lockUser.DefaultSalesInvoiceDiscountId = objUser.DefaultSalesInvoiceDiscountId;
                                                lockUser.SalesInvoiceName = objUser.SalesInvoiceName;
                                                lockUser.SalesInvoiceCheckedById = objUser.SalesInvoiceCheckedById;
                                                lockUser.SalesInvoiceApprovedById = objUser.SalesInvoiceApprovedById;
                                                lockUser.OfficialReceiptName = objUser.OfficialReceiptName;
                                                lockUser.IsIncludeCostStockReports = objUser.IsIncludeCostStockReports;
                                                lockUser.IsLocked = true;
                                                lockUser.UpdatedById = currentUserId;
                                                lockUser.UpdatedDateTime = DateTime.Now;
                                                db.SubmitChanges();

                                                String newObject = at.GetObjectString(user.FirstOrDefault());
                                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                                return Request.CreateResponse(HttpStatusCode.OK);
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
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock user.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this user page.");
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

        // ===========
        // Unlock User
        // ===========
        [Authorize, HttpPut, Route("api/user/unlock/{id}")]
        public HttpResponseMessage UnlockUser(String id)
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
                                    && d.SysForm.FormName.Equals("UserDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var user = from d in db.MstUsers
                                       where d.Id == Convert.ToInt32(id)
                                       select d;

                            if (user.Any())
                            {
                                if (user.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = at.GetObjectString(user.FirstOrDefault());

                                    var unlockUser = user.FirstOrDefault();
                                    unlockUser.IsLocked = false;
                                    unlockUser.UpdatedById = currentUserId;
                                    unlockUser.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    String newObject = at.GetObjectString(user.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These user details are already unlocked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These user details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock user.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this user page.");
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
