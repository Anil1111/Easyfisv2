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
                                select new Entities.MstUserForm
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
                                select new Entities.MstUserForm
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
            return UserRights("AccountsPayableReport");
        }

        // ============================
        // View Accounts Payable Report
        // ============================
        [Authorize]
        public ActionResult AccountsPayableReport()
        {
            return UserRights("ViewAccountsPayableReport");
        }

        // ============================
        // View Purchase Summary Report
        // ============================
        [Authorize]
        public ActionResult PurchaseSummaryReport()
        {
            return UserRights("ViewPurchaseSummaryReport");
        }

        // ===========================
        // View Purchase Detail Report
        // ===========================
        [Authorize]
        public ActionResult PurchaseDetailReport()
        {
            return UserRights("ViewPurchaseDetailReport");
        }

        // =====================================
        // View Receiving Receipt Summary Report
        // =====================================
        [Authorize]
        public ActionResult ReceivingReceiptSummaryReport()
        {
            return UserRights("ViewReceivingReceiptSummaryReport");
        }

        // ====================================
        // View Receiving Receipt Detail Report
        // ====================================
        [Authorize]
        public ActionResult ReceivingReceiptDetailReport()
        {
            return UserRights("ViewReceivingReceiptDetailReport");
        }

        // ================================
        // View Disbursement Summary Report
        // ================================
        [Authorize]
        public ActionResult DisbursementSummaryReport()
        {
            return UserRights("ViewDisbursementSummaryReport");
        }

        // ===============================
        // View Disbursement Detail Report
        // ===============================
        [Authorize]
        public ActionResult DisbursementDetailReport()
        {
            return UserRights("ViewDisbursementDetailReport");
        }

        // ===================
        // Accounts Receivable
        // ===================
        [Authorize]
        public ActionResult AccountsReceivable()
        {
            return UserRights("AccountsReceivableReport");
        }

        // ================================
        // View Accounts Receivable  Report
        // ================================
        [Authorize]
        public ActionResult AccountsReceivableReport()
        {
            return UserRights("ViewAccountsReceivableReport");
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
            return UserRights("ViewSalesSummaryReport");
        }

        // ========================
        // View Sales Detail Report
        // ========================
        [Authorize]
        public ActionResult SalesDetailReport()
        {
            return UserRights("ViewSalesDetailReport");
        }

        // ==============================
        // View Collection Summary Report
        // ==============================
        [Authorize]
        public ActionResult CollectionSummaryReport()
        {
            return UserRights("ViewCollectionSummaryReport");
        }

        // =============================
        // View Collection Detail Report
        // =============================
        [Authorize]
        public ActionResult CollectionDetailReport()
        {
            return UserRights("ViewCollectionDetailReport");
        }

        // =======================
        // View Consignment Report
        // =======================
        [Authorize]
        public ActionResult ConsignmentReport()
        {
            return UserRights("ViewConsignmentReport");
        }

        // =========
        // Inventory
        // =========
        [Authorize]
        public ActionResult Inventory()
        {
            return UserRights("InventoryReport");
        }

        // =====================
        // View Inventory Report
        // =====================
        [Authorize]
        public ActionResult InventoryReport()
        {
            return UserRights("ViewInventoryReport");
        }

        // =====================
        // View Inventory Report
        // =====================
        [Authorize]
        public ActionResult StockCard()
        {
            return UserRights("ViewStockCard");
        }

        // ===========================
        // View Stock In Detail Report
        // ===========================
        [Authorize]
        public ActionResult StockInDetailReport()
        {
            return UserRights("ViewStockInDetailReport");
        }

        // ============================
        // View Stock Out Detail Report
        // ============================
        [Authorize]
        public ActionResult StockOutDetailReport()
        {
            return UserRights("ViewStockOutDetailReport");
        }

        // =================================
        // View Stock Transfer Detail Report
        // =================================
        [Authorize]
        public ActionResult StockTransferDetailReport()
        {
            return UserRights("ViewStockTransferDetailReport");
        }

        // =====================
        // View Item List Report
        // =====================
        [Authorize]
        public ActionResult ItemList()
        {
            return UserRights("ViewItemList");
        }

        // ===============================
        // View Item Component List Report
        // ===============================
        [Authorize]
        public ActionResult ItemComponentList()
        {
            return UserRights("ViewItemComponentList");
        }

        // ================================
        // View Physical Count Sheet Report
        // ================================
        [Authorize]
        public ActionResult PhysicalCountSheet()
        {
            return UserRights("ViewPhysicalCountSheet");
        }

        // ====================
        // Financial Statements
        // ====================
        [Authorize]
        public ActionResult FinancialStatements()
        {
            return UserRights("FinancialStatementReport");
        }

        // ==================
        // View Trial Balance
        // ==================
        [Authorize]
        public ActionResult TrialBalance()
        {
            return UserRights("ViewTrialBalance");
        }

        // ===================
        // View Account Ledger
        // ===================
        [Authorize]
        public ActionResult AccountLedger()
        {
            return UserRights("ViewAccountLedger");
        }

        // ===========================
        // View Receiving Receipt Book 
        // ===========================
        [Authorize]
        public ActionResult ReceivingReceiptBook()
        {
            return UserRights("ViewReceivingReceiptBook");
        }

        // ======================
        // View Disbursement Book 
        // ======================
        [Authorize]
        public ActionResult DisbursementBook()
        {
            return UserRights("ViewDisbursementBook");
        }

        // ===============
        // View Sales Book 
        // ===============
        [Authorize]
        public ActionResult SalesBook()
        {
            return UserRights("ViewSalesBook");
        }

        // ====================
        // View Collection Book 
        // ====================
        [Authorize]
        public ActionResult CollectionBook()
        {
            return UserRights("ViewCollectionBook");
        }

        // ===================
        // View Stock In Book 
        // ==================
        [Authorize]
        public ActionResult StockInBook()
        {
            return UserRights("ViewStockInBook");

        }

        // ===================
        // View Stock Out Book 
        // ===================
        [Authorize]
        public ActionResult StockOutBook()
        {
            return UserRights("ViewStockOutBook");
        }

        // ========================
        // View Stock Transfer Book 
        // ========================
        [Authorize]
        public ActionResult StockTransferBook()
        {
            return UserRights("ViewStockTransferBook");
        }

        // =========================
        // View Journal Voucher Book 
        // =========================
        [Authorize]
        public ActionResult JournalVoucherBook()
        {
            return UserRights("ViewJournalVoucherBook");
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
            return UserRights("ViewBalanceSheet");
        }

        // =============================
        // View Balance Sheet Per Branch
        // =============================
        [Authorize]
        public ActionResult BalanceSheetPerBranch()
        {
            return UserRights("ViewBalanceSheetPerBranch");
        }

        // =====================
        // View Income Statement
        // =====================
        [Authorize]
        public ActionResult IncomeStatement()
        {
            return UserRights("ViewIncomeStatement");
        }

        // ==============
        // View Cash Flow
        // ==============
        [Authorize]
        public ActionResult CashFlow()
        {
            return UserRights("ViewCashFlow");
        }

        // ===============
        // BIR CAS Reports
        // ===============
        [Authorize]
        public ActionResult BIRCASReports()
        {
            return UserRights("BIRCASReport");
        }

        // ==============================
        // View BIR CAS Cash Receipt Book 
        // ==============================
        [Authorize]
        public ActionResult BIRCASCashReceiptBook()
        {
            return UserRights("ViewBIRCASCashReceiptBook");
        }

        // ==========================
        // View BIR CAS Sales Journal
        // ==========================
        [Authorize]
        public ActionResult BIRCASSalesJournal()
        {
            return UserRights("ViewBIRCASSalesJournal");
        }

        // =============================
        // View BIR CAS Purchase Journal
        // =============================
        [Authorize]
        public ActionResult BIRCASPurchaseJournal()
        {
            return UserRights("ViewBIRCASPurchaseJournal");
        }

        // ==============================
        // View BIR CAS Disbursement Book
        // ==============================
        [Authorize]
        public ActionResult BIRCASDisbursementBook()
        {
            return UserRights("ViewBIRCASDisbursementBook");
        }

        // ===========================
        // View BIR CAS General Ledger
        // ===========================
        [Authorize]
        public ActionResult BIRCASGeneralLedger()
        {
            return UserRights("ViewBIRCASGeneralLedger");
        }

        // ==============================
        // View BIR CAS Inventory Journal
        // ==============================
        [Authorize]
        public ActionResult BIRCASInventoryJournal()
        {
            return UserRights("ViewBIRCASInventoryJournal");
        }

        // ========================
        // View BIR CAS Audit Trail
        // ========================
        [Authorize]
        public ActionResult BIRCASAuditTrail()
        {
            return UserRights("ViewBIRCASAuditTrail");
        }

        // ==================================
        // View Inventory Report Per Supplier
        // ==================================
        [Authorize]
        public ActionResult ItemListSupplier()
        {
            return View();
        }

        // ==============================
        // View Inventory Report Per Item
        // ==============================
        [Authorize]
        public ActionResult InventoryReportItem()
        {
            return View();
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