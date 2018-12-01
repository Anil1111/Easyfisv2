using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepBIRCASSalesJournalController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ============================
        // Compute VATable Sales Amount
        // ============================
        public Decimal ComputeVATableSales(String taxType, Decimal amount)
        {
            Decimal VATableSalesAmount = 0;
            if (taxType.Equals("VAT Output"))
            {
                VATableSalesAmount = amount;
            }

            return VATableSalesAmount;
        }

        // ===============================
        // Compute VAT Exempt Sales Amount
        // ===============================
        public Decimal ComputeVATExemptSales(String taxType, Decimal taxRate, String discountType, Decimal price, Decimal quantity, Decimal amount)
        {
            Decimal VATExemptAmount = 0;
            if (taxType.Equals("VAT Exempt") && (discountType.Equals("Senior Citizen Discount") || discountType.Equals("PWD")))
            {
                VATExemptAmount = amount - ((taxRate / 100) * (price * quantity) / ((taxRate / 100) + 1));
            }

            return VATExemptAmount;
        }

        // ===============================
        // Compute Zero Rated Sales Amount
        // ===============================
        public Decimal ComputeZeroRatedSales(String taxType, Decimal amount)
        {
            Decimal zeroRatedAmount = 0;
            if (taxType.Equals("VAT Zero Rated"))
            {
                zeroRatedAmount = amount;
            }

            return 0;
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/BIRCASSalesJournal/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListBIRCASSalesJournalListCompany()
        {
            var companies = from d in db.MstCompanies.OrderBy(d => d.Company)
                            select new Entities.MstCompany
                            {
                                Id = d.Id,
                                Company = d.Company
                            };

            return companies.ToList();
        }

        // ===============================
        // Dropdown List - Branch (Filter)
        // ===============================
        [Authorize, HttpGet, Route("api/BIRCASSalesJournal/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListBIRCASSalesJournalListBranch(String companyId)
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

        // =======================
        // List Sales Journal Data
        // =======================
        [Authorize, HttpGet, Route("api/BIRCASSalesJournal/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepBIRCASSalesJournal> ListBIRCASSalesJournal(String startDate, String endDate, String companyId, String branchId)
        {
            if (Convert.ToInt32(branchId) != 0)
            {
                var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                        where d.TrnSalesInvoice.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                        && d.TrnSalesInvoice.BranchId == Convert.ToInt32(branchId)
                                        && d.TrnSalesInvoice.SIDate >= Convert.ToDateTime(startDate)
                                        && d.TrnSalesInvoice.SIDate <= Convert.ToDateTime(endDate)
                                        && (d.TrnSalesInvoice.IsLocked == true && d.TrnSalesInvoice.IsCancelled == false)
                                        select new Entities.RepBIRCASSalesJournal
                                        {
                                            Date = d.TrnSalesInvoice.SIDate.ToShortDateString(),
                                            ReferenceNumber = "SI-" + d.TrnSalesInvoice.MstBranch.BranchCode + "-" + d.TrnSalesInvoice.SINumber,
                                            Customer = d.TrnSalesInvoice.MstArticle.Article,
                                            CustomerTIN = d.TrnSalesInvoice.MstArticle.TaxNumber,
                                            Address = d.TrnSalesInvoice.MstArticle.Address,
                                            DocumentReference = d.TrnSalesInvoice.DocumentReference,
                                            ManualReferenceNumber = d.TrnSalesInvoice.ManualSINumber,
                                            ItemCode = d.MstArticle.ManualArticleCode,
                                            DiscountAmount = d.DiscountAmount,
                                            Amount = d.Amount,
                                            VATableSalesAmount = ComputeVATableSales(d.MstTaxType.TaxType, d.Amount),
                                            VATExemptSalesAmount = ComputeVATExemptSales(d.MstTaxType.TaxType, d.MstTaxType.TaxRate, d.MstDiscount.Discount, d.Price, d.Quantity, d.Amount),
                                            ZeroRatedSalesAmount = ComputeZeroRatedSales(d.MstTaxType.TaxType, d.Amount),
                                            VATAmount = d.VATAmount
                                        };

                return salesInvoiceItems.OrderBy(d => d.ReferenceNumber).ToList();
            }
            else
            {
                var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                        where d.TrnSalesInvoice.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                        && d.TrnSalesInvoice.SIDate >= Convert.ToDateTime(startDate)
                                        && d.TrnSalesInvoice.SIDate <= Convert.ToDateTime(endDate)
                                        && (d.TrnSalesInvoice.IsLocked == true && d.TrnSalesInvoice.IsCancelled == false)
                                        select new Entities.RepBIRCASSalesJournal
                                        {
                                            Date = d.TrnSalesInvoice.SIDate.ToShortDateString(),
                                            ReferenceNumber = "SI-" + d.TrnSalesInvoice.MstBranch.BranchCode + "-" + d.TrnSalesInvoice.SINumber,
                                            Customer = d.TrnSalesInvoice.MstArticle.Article,
                                            CustomerTIN = d.TrnSalesInvoice.MstArticle.TaxNumber,
                                            Address = d.TrnSalesInvoice.MstArticle.Address,
                                            DocumentReference = d.TrnSalesInvoice.DocumentReference,
                                            ManualReferenceNumber = d.TrnSalesInvoice.ManualSINumber,
                                            ItemCode = d.MstArticle.ManualArticleCode,
                                            DiscountAmount = d.DiscountAmount,
                                            Amount = d.Amount,
                                            VATableSalesAmount = ComputeVATableSales(d.MstTaxType.TaxType, d.Amount),
                                            VATExemptSalesAmount = ComputeVATExemptSales(d.MstTaxType.TaxType, d.MstTaxType.TaxRate, d.MstDiscount.Discount, d.Price, d.Quantity, d.Amount),
                                            ZeroRatedSalesAmount = ComputeZeroRatedSales(d.MstTaxType.TaxType, d.Amount),
                                            VATAmount = d.VATAmount
                                        };

                return salesInvoiceItems.OrderBy(d => d.ReferenceNumber).ToList();
            }
        }
    }
}
