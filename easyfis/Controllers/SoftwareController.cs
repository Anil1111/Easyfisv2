using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System;
using System.Diagnostics;


namespace easyfis.Controllers
{
    public class SoftwareController : UserAccountController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Page Access
        // ===========
        public String PageAccess(String page)
        {
            String form = "";

            var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
            if (currentUser.Any())
            {
                var userForms = from d in db.MstUserForms
                                where d.UserId == currentUser.FirstOrDefault().Id
                                select new Models.MstUserForm
                                {
                                    Id = d.Id,
                                    UserId = d.UserId,
                                    User = d.MstUser.FullName,
                                    FormId = d.FormId,
                                    Form = d.SysForm.FormName,
                                    Particulars = d.SysForm.Particulars,
                                    CanAdd = d.CanAdd,
                                    CanEdit = d.CanEdit,
                                    CanDelete = d.CanDelete,
                                    CanLock = d.CanLock,
                                    CanUnlock = d.CanUnlock,
                                    CanPrint = d.CanPrint
                                };

                foreach (var userForm in userForms)
                {
                    if (page.Equals(userForm.Form))
                    {
                        ViewData.Add("CanAdd", userForm.CanAdd);
                        ViewData.Add("CanEdit", userForm.CanEdit);
                        ViewData.Add("CanDelete", userForm.CanDelete);
                        ViewData.Add("CanLock", userForm.CanLock);
                        ViewData.Add("CanUnlock", userForm.CanUnlock);
                        ViewData.Add("CanPrint", userForm.CanPrint);

                        form = userForm.Form;
                        break;
                    }
                }
            }

            return form;
        }

        // ========================
        // Formas With Detail Pages
        // ========================
        public String AccessToDetail(String page)
        {
            String form = "";

            var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
            if (currentUser.Any())
            {
                var userForms = from d in db.MstUserForms
                                where d.UserId == currentUser.FirstOrDefault().Id
                                select new Models.MstUserForm
                                {
                                    Id = d.Id,
                                    UserId = d.UserId,
                                    User = d.MstUser.FullName,
                                    FormId = d.FormId,
                                    Form = d.SysForm.FormName,
                                    Particulars = d.SysForm.Particulars,
                                    CanAdd = d.CanAdd,
                                    CanEdit = d.CanEdit,
                                    CanDelete = d.CanDelete,
                                    CanLock = d.CanLock,
                                    CanUnlock = d.CanUnlock,
                                    CanPrint = d.CanPrint
                                };

                foreach (var userForm in userForms)
                {
                    if (page.Equals(userForm.Form))
                    {
                        form = userForm.Form;
                        break;
                    }
                }
            }

            return form;
        }

        // ===========
        // User Rights
        // ===========
        [Authorize]
        public ActionResult UserRights(String formName)
        {
            var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
            if (currentUser.Any())
            {
                var userForms = from d in db.MstUserForms
                                where d.UserId == currentUser.FirstOrDefault().Id
                                && d.SysForm.FormName.Equals(formName)
                                select d;

                if (userForms.Any())
                {
                    var userFormsRights = userForms.FirstOrDefault();
                    var model = new Entities.MstUserForm
                    {
                        CanAdd = userFormsRights.CanAdd,
                        CanEdit = userFormsRights.CanEdit,
                        CanDelete = userFormsRights.CanDelete,
                        CanLock = userFormsRights.CanLock,
                        CanUnlock = userFormsRights.CanUnlock,
                        CanPrint = userFormsRights.CanPrint
                    };

                    return View(model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Software");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // ==============
        // Not Found Page
        // ==============
        [Authorize]
        public ActionResult PageNotFound()
        {
            return View();
        }

        // ==============
        // Forbidden Page
        // ==============
        [Authorize]
        public ActionResult Forbidden()
        {
            return View();
        }

        // =====================
        // Dashboard - Main Menu
        // =====================
        [Authorize]
        public ActionResult Index()
        {
            return UserRights("Software");
        }

        // ========
        // Supplier
        // ========
        [Authorize]
        public ActionResult Supplier()
        {
            return UserRights("SupplierList");
        }

        // ===============
        // Supplier Detail
        // ===============
        [Authorize]
        public ActionResult SupplierDetail()
        {
            return UserRights("SupplierDetail");
        }

        // ========
        // Customer
        // ========
        [Authorize]
        public ActionResult Customer()
        {
            return UserRights("CustomerList");
        }

        // ===============
        // Customer Detail
        // ===============
        [Authorize]
        public ActionResult CustomerDetail()
        {
            return UserRights("CustomerDetail");
        }

        // ====
        // Item
        // ====
        [Authorize]
        public ActionResult Item()
        {
            return UserRights("ItemList");
        }

        // ===========
        // Item Detail
        // ===========
        [Authorize]
        public ActionResult ItemDetail()
        {
            return UserRights("ItemDetail");
        }

        // =================
        // Chart of Accounts
        // =================
        [Authorize]
        public ActionResult ChartOfAccounts()
        {
            return UserRights("ChartOfAccounts");
        }

        // ================
        // Purchase Request
        // ================
        [Authorize]
        public ActionResult PurchaseRequest()
        {
            return UserRights("PurchaseRequestList");
        }

        // =======================
        // Purchase Request Detail
        // =======================
        [Authorize]
        public ActionResult PurchaseRequestDetail()
        {
            return UserRights("PurchaseRequestDetail");
        }

        // ==============
        // Purchase Order
        // ==============
        [Authorize]
        public ActionResult PurchaseOrder()
        {
            return UserRights("PurchaseOrderList");
        }

        // =====================
        // Purchase Order Detail
        // =====================
        [Authorize]
        public ActionResult PurchaseOrderDetail()
        {
            return UserRights("PurchaseOrderDetail");
        }

        // =================
        // Receiving Receipt
        // =================
        [Authorize]
        public ActionResult ReceivingReceipt()
        {
            return UserRights("ReceivingReceiptList");
        }

        // ========================
        // Receiving Receipt Detail
        // ========================
        [Authorize]
        public ActionResult ReceivingReceiptDetail()
        {
            return UserRights("ReceivingReceiptDetail");
        }

        // =====
        // Sales
        // =====
        [Authorize]
        public ActionResult Sales()
        {
            return UserRights("SalesInvoiceList");
        }

        // ============
        // Sales Detail
        // ============
        [Authorize]
        public ActionResult SalesDetail()
        {
            return UserRights("SalesInvoiceDetail");
        }

        // ========
        // Stock In
        // ========
        [Authorize]
        public ActionResult StockIn()
        {
            return UserRights("StockInList");
        }

        // ===============
        // Stock In Detail
        // ===============
        [Authorize]
        public ActionResult StockInDetail()
        {
            return UserRights("StockInDetail");
        }

        // =========
        // Stock Out 
        // =========
        [Authorize]
        public ActionResult StockOut()
        {
            return UserRights("StockOutList");
        }

        // ================
        // Stock Out Detail
        // ================
        [Authorize]
        public ActionResult StockOutDetail()
        {
            return UserRights("StockOutDetail");
        }

        // ==============
        // Stock Transfer
        // ==============
        [Authorize]
        public ActionResult StockTransfer()
        {
            return UserRights("StockTransferList");
        }

        // =====================
        // Stock Transfer Detail
        // =====================
        [Authorize]
        public ActionResult StockTransferDetail()
        {
            return UserRights("StockTransferDetail");
        }

        // ===========
        // Stock Count
        // ===========
        [Authorize]
        public ActionResult StockCount()
        {
            return UserRights("StockCountList");
        }

        // ==================
        // Stock Count Detail
        // ==================
        [Authorize]
        public ActionResult StockCountDetail()
        {
            return UserRights("StockCountDetail");
        }

        // ================
        // Stock Withdrawal
        // ================
        [Authorize]
        public ActionResult StockWithdrawal()
        {
            return UserRights("StockWithdrawalList");
        }

        // =======================
        // Stock Withdrawal Detail  
        // =======================
        [Authorize]
        public ActionResult StockWithdrawalDetail()
        {
            return UserRights("StockWithdrawalDetail");
        }

        // ===============
        // Journal Voucher
        // ===============
        [Authorize]
        public ActionResult JournalVoucher()
        {
            return UserRights("JournalVoucherList");
        }

        // ======================
        // Journal Voucher Detail
        // ======================
        [Authorize]
        public ActionResult JournalVoucherDetail()
        {
            return UserRights("JournalVoucherDetail");
        }

        // ==========
        // Item Price
        // ==========
        [Authorize]
        public ActionResult ItemPrice()
        {
            return UserRights("ItemPriceList");
        }

        // =================
        // Item Price Detail
        // =================
        [Authorize]
        public ActionResult ItemPriceDetail()
        {
            return UserRights("ItemPriceDetail");
        }

        // ====
        // Bank
        // ====
        [Authorize]
        public ActionResult Bank()
        {
            return UserRights("BankList");
        }

        // ===================
        // Bank Reconciliation
        // ===================
        [Authorize]
        public ActionResult BankReconciliation()
        {
            return UserRights("BankReconciliation");
        }

        // ============
        // System Table
        // ============
        [Authorize]
        public ActionResult SystemTables()
        {
            return UserRights("SystemTables");
        }

        // =======
        // Company
        // =======
        [Authorize]
        public ActionResult Company()
        {
            return UserRights("CompanyList");
        }

        // ==============
        // Company Detail
        // ==============
        [Authorize]
        public ActionResult CompanyDetail()
        {
            return UserRights("CompanyDetail");
        }

        // =====
        // Users
        // =====
        [Authorize]
        public ActionResult Users()
        {
            return UserRights("UserList");
        }

        // ============
        // Users Detail
        // ============
        [Authorize]
        public ActionResult UsersDetail()
        {
            return UserRights("UserDetail");
        }

        // ================
        // Accounts Payable
        // ================
        [Authorize]
        public ActionResult AccountsPayable()
        {
            if (PageAccess("AccountsPayableReport").Equals("AccountsPayableReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ============================
        // View Accounts Payable Report
        // ============================
        [Authorize]
        public ActionResult AccountsPayableReport()
        {
            if (PageAccess("ViewAccountsPayableReport").Equals("ViewAccountsPayableReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ============================
        // View Purchase Summary Report
        // ============================
        [Authorize]
        public ActionResult PurchaseSummaryReport()
        {
            if (PageAccess("ViewPurchaseSummaryReport").Equals("ViewPurchaseSummaryReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ===========================
        // View Purchase Detail Report
        // ===========================
        [Authorize]
        public ActionResult PurchaseDetailReport()
        {
            if (PageAccess("ViewPurchaseDetailReport").Equals("ViewPurchaseDetailReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // =====================================
        // View Receiving Receipt Summary Report
        // =====================================
        [Authorize]
        public ActionResult ReceivingReceiptSummaryReport()
        {
            if (PageAccess("ViewReceivingReceiptSummaryReport").Equals("ViewReceivingReceiptSummaryReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ====================================
        // View Receiving Receipt Detail Report
        // ====================================
        [Authorize]
        public ActionResult ReceivingReceiptDetailReport()
        {
            if (PageAccess("ViewReceivingReceiptDetailReport").Equals("ViewReceivingReceiptDetailReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ================================
        // View Disbursement Summary Report
        // ================================
        [Authorize]
        public ActionResult DisbursementSummaryReport()
        {
            if (PageAccess("ViewDisbursementSummaryReport").Equals("ViewDisbursementSummaryReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ===============================
        // View Disbursement Detail Report
        // ===============================
        [Authorize]
        public ActionResult DisbursementDetailReport()
        {
            if (PageAccess("ViewDisbursementDetailReport").Equals("ViewDisbursementDetailReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ===================
        // Accounts Receivable
        // ===================
        [Authorize]
        public ActionResult AccountsReceivable()
        {
            if (PageAccess("AccountsReceivableReport").Equals("AccountsReceivableReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ================================
        // View Accounts Receivable  Report
        // ================================
        [Authorize]
        public ActionResult AccountsReceivableReport()
        {
            if (PageAccess("ViewAccountsReceivableReport").Equals("ViewAccountsReceivableReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // =========================
        // View Statement of Account
        // =========================
        [Authorize]
        public ActionResult StatementOfAccount()
        {
            return View();
        }

        // =========================
        // View Sales Summary Report
        // =========================
        [Authorize]
        public ActionResult SalesSummaryReport()
        {
            if (PageAccess("ViewSalesSummaryReport").Equals("ViewSalesSummaryReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ========================
        // View Sales Detail Report
        // ========================
        [Authorize]
        public ActionResult SalesDetailReport()
        {
            if (PageAccess("ViewSalesDetailReport").Equals("ViewSalesDetailReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ==============================
        // View Collection Summary Report
        // ==============================
        [Authorize]
        public ActionResult CollectionSummaryReport()
        {
            if (PageAccess("ViewCollectionSummaryReport").Equals("ViewCollectionSummaryReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // =============================
        // View Collection Detail Report
        // =============================
        [Authorize]
        public ActionResult CollectionDetailReport()
        {
            if (PageAccess("ViewCollectionDetailReport").Equals("ViewCollectionDetailReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // =========
        // Inventory
        // =========
        [Authorize]
        public ActionResult Inventory()
        {
            if (PageAccess("InventoryReport").Equals("InventoryReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // =====================
        // View Inventory Report
        // =====================
        [Authorize]
        public ActionResult InventoryReport()
        {
            if (PageAccess("ViewInventoryReport").Equals("ViewInventoryReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // =====================
        // View Inventory Report
        // =====================
        [Authorize]
        public ActionResult StockCard()
        {
            if (PageAccess("ViewStockCard").Equals("ViewStockCard"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ===========================
        // View Stock In Detail Report
        // ===========================
        [Authorize]
        public ActionResult StockInDetailReport()
        {
            if (PageAccess("ViewStockInDetailReport").Equals("ViewStockInDetailReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ============================
        // View Stock Out Detail Report
        // ============================
        [Authorize]
        public ActionResult StockOutDetailReport()
        {
            if (PageAccess("ViewStockOutDetailReport").Equals("ViewStockOutDetailReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // =================================
        // View Stock Transfer Detail Report
        // =================================
        [Authorize]
        public ActionResult StockTransferDetailReport()
        {
            if (PageAccess("ViewStockTransferDetailReport").Equals("ViewStockTransferDetailReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // =====================
        // View Item List Report
        // =====================
        [Authorize]
        public ActionResult ItemList()
        {
            if (PageAccess("ViewItemList").Equals("ViewItemList"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ===============================
        // View Item Component List Report
        // ===============================
        [Authorize]
        public ActionResult ItemComponentList()
        {
            if (PageAccess("ViewItemComponentList").Equals("ViewItemComponentList"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ================================
        // View Physical Count Sheet Report
        // ================================
        [Authorize]
        public ActionResult PhysicalCountSheet()
        {
            if (PageAccess("ViewPhysicalCountSheet").Equals("ViewPhysicalCountSheet"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ====================
        // Financial Statements
        // ====================
        [Authorize]
        public ActionResult FinancialStatements()
        {
            if (PageAccess("FinancialStatementReport").Equals("FinancialStatementReport"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ==================
        // View Trial Balance
        // ==================
        [Authorize]
        public ActionResult TrialBalance()
        {
            if (PageAccess("ViewTrialBalance").Equals("ViewTrialBalance"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ===================
        // View Account Ledger
        // ===================
        [Authorize]
        public ActionResult AccountLedger()
        {
            if (PageAccess("ViewAccountLedger").Equals("ViewAccountLedger"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ===========================
        // View Receiving Receipt Book 
        // ===========================
        [Authorize]
        public ActionResult ReceivingReceiptBook()
        {
            if (PageAccess("ViewReceivingReceiptBook").Equals("ViewReceivingReceiptBook"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ======================
        // View Disbursement Book 
        // ======================
        [Authorize]
        public ActionResult DisbursementBook()
        {
            if (PageAccess("ViewDisbursementBook").Equals("ViewDisbursementBook"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ===============
        // View Sales Book 
        // ===============
        [Authorize]
        public ActionResult SalesBook()
        {
            if (PageAccess("ViewSalesBook").Equals("ViewSalesBook"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ====================
        // View Collection Book 
        // ====================
        [Authorize]
        public ActionResult CollectionBook()
        {
            if (PageAccess("ViewCollectionBook").Equals("ViewCollectionBook"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ===================
        // View Stock In Book 
        // ==================
        [Authorize]
        public ActionResult StockInBook()
        {
            if (PageAccess("ViewStockInBook").Equals("ViewStockInBook"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }

        }

        // ===================
        // View Stock Out Book 
        // ===================
        [Authorize]
        public ActionResult StockOutBook()
        {
            if (PageAccess("ViewStockOutBook").Equals("ViewStockOutBook"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ========================
        // View Stock Transfer Book 
        // ========================
        [Authorize]
        public ActionResult StockTransferBook()
        {
            if (PageAccess("ViewStockTransferBook").Equals("ViewStockTransferBook"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // =========================
        // View Journal Voucher Book 
        // =========================
        [Authorize]
        public ActionResult JournalVoucherBook()
        {
            if (PageAccess("ViewJournalVoucherBook").Equals("ViewJournalVoucherBook"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ============
        // Disbursement
        // ============
        [Authorize]
        public ActionResult Disbursement()
        {
            return UserRights("DisbursementList");
        }

        // ===================
        // Disbursement Detail
        // ===================
        [Authorize]
        public ActionResult DisbursementDetail()
        {
            return UserRights("DisbursementDetail");
        }

        // ==========
        // Collection
        // ==========
        [Authorize]
        public ActionResult Collection()
        {
            return UserRights("CollectionList");
        }

        // =================
        // Collection Detail
        // =================
        [Authorize]
        public ActionResult CollectionDetail()
        {
            return UserRights("CollectionDetail");
        }

        // ==================
        // View Balance Sheet
        // ==================
        [Authorize]
        public ActionResult BalanceSheet()
        {
            if (PageAccess("ViewBalanceSheet").Equals("ViewBalanceSheet"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // =====================
        // View Income Statement
        // =====================
        [Authorize]
        public ActionResult IncomeStatement()
        {
            if (PageAccess("ViewIncomeStatement").Equals("ViewIncomeStatement"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // ==============
        // View Cash Flow
        // ==============
        [Authorize]
        public ActionResult CashFlow()
        {
            if (PageAccess("ViewCashFlow").Equals("ViewCashFlow"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forbidden", "Software");
            }
        }

        // =======================
        // POS Integration Reports
        // =======================
        [Authorize]
        public ActionResult SalesDetailReportVATSales()
        {
            return View();
        }

        [Authorize]
        public ActionResult SalesSummaryReportSalesNo()
        {
            return View();
        }

        [Authorize]
        public ActionResult CancelledSalesSummaryReport()
        {
            return View();
        }

        [Authorize]
        public ActionResult SeniorCitizenSalesSummaryReport()
        {
            return View();
        }

        [Authorize]
        public ActionResult TopSellingItemReport()
        {
            return View();
        }

        [Authorize]
        public ActionResult HourlyTopSellingItemsReport()
        {
            return View();
        }

        [Authorize]
        public ActionResult SalesSummaryReportAllFields()
        {
            return View();
        }
    }
}