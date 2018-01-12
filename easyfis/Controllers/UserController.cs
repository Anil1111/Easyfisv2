using System.Linq;
using System.Web.Mvc;
using easyfis.Models;

namespace easyfis.Controllers
{
    public class UserAccountController : Controller
    {
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (User != null)
            {
                var context = new ApplicationDbContext();
                var username = User.Identity.Name;

                if (!string.IsNullOrEmpty(username))
                {
                    // ======================
                    // Current Logged-In User
                    // ======================
                    var user = context.Users.SingleOrDefault(u => u.UserName == username);

                    // ==========
                    // AspNetUser
                    // ==========
                    string aspNetUserId = user.Id;
                    string userName = user.UserName;
                    string fullName = user.FullName;

                    ViewData.Add("UserId", aspNetUserId);
                    ViewData.Add("UserName", userName);
                    ViewData.Add("FullName", fullName);

                    // =======
                    // MstUser
                    // =======
                    Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

                    var currentUser = from d in db.MstUsers
                                      where d.UserId == aspNetUserId
                                      select d;

                    if (currentUser.Any())
                    {
                        // ==========================
                        // Current Branch and Company
                        // ==========================
                        int branchId = currentUser.FirstOrDefault().BranchId;
                        string branch = currentUser.FirstOrDefault().MstBranch.Branch;
                        int companyId = currentUser.FirstOrDefault().CompanyId;
                        string company = currentUser.FirstOrDefault().MstCompany.Company;

                        ViewData.Add("BranchId", branchId);
                        ViewData.Add("Branch", branch);
                        ViewData.Add("CompanyId", companyId);
                        ViewData.Add("Company", company);

                        // ==================
                        // Defaults (Current)
                        // ==================
                        int mstUserId = currentUser.FirstOrDefault().Id;
                        string salesInvoiceName = currentUser.FirstOrDefault().SalesInvoiceName;
                        int defaultSalesInvoiceDiscountId = currentUser.FirstOrDefault().DefaultSalesInvoiceDiscountId;
                        string defaultSalesInvoiceDiscount = currentUser.FirstOrDefault().MstDiscount.Discount;
                        string officialReceiptName = currentUser.FirstOrDefault().OfficialReceiptName;
                        
                        ViewData.Add("MstUserId", mstUserId);
                        ViewData.Add("SalesInvoiceName", salesInvoiceName);
                        ViewData.Add("defaultSalesInvoiceDiscountId", defaultSalesInvoiceDiscountId);
                        ViewData.Add("DefaultSalesInvoiceDiscount", defaultSalesInvoiceDiscount);
                        ViewData.Add("OfficialReceiptName", officialReceiptName);

                        // ================
                        // System (Current)
                        // ================
                        int netIncomeAccountId = currentUser.FirstOrDefault().IncomeAccountId;
                        string netIncomeAccount = currentUser.FirstOrDefault().MstAccount.Account;
                        int supplierAdvancesAccountId = currentUser.FirstOrDefault().SupplierAdvancesAccountId;
                        string supplierAdvancesAccount = currentUser.FirstOrDefault().MstAccount1.Account;
                        int customerAdvancesAccountId = currentUser.FirstOrDefault().CustomerAdvancesAccountId;
                        string customerAdvancesAccount = currentUser.FirstOrDefault().MstAccount2.Account;
                        string inventoryType = currentUser.FirstOrDefault().InventoryType;
                        bool isIncludeCostStockReports = currentUser.FirstOrDefault().IsIncludeCostStockReports;

                        ViewData.Add("NetIncomeAccountId", netIncomeAccountId);
                        ViewData.Add("NetIncomeAccount", netIncomeAccount);
                        ViewData.Add("SupplierAdvancesAccountId", supplierAdvancesAccountId);
                        ViewData.Add("SupplierAdvancesAccount", supplierAdvancesAccount);
                        ViewData.Add("CustomerAdvancesAccountId", customerAdvancesAccountId);
                        ViewData.Add("CustomerAdvancesAccount", customerAdvancesAccount);
                        ViewData.Add("InventoryType", inventoryType);
                        ViewData.Add("IsIncludeCostStockReports", isIncludeCostStockReports);
                    }
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
