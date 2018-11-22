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
                                            ManualReferenceNumber = d.TrnSalesInvoice.ManualSINumber,
                                            TotalAmount = d.Price * d.Quantity,
                                            Discount = d.DiscountAmount,
                                            VAT = d.VATAmount,
                                            NetSales = d.Amount
                                        };

                return salesInvoiceItems.ToList();
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
                                            ManualReferenceNumber = d.TrnSalesInvoice.ManualSINumber,
                                            TotalAmount = d.Price * d.Quantity,
                                            Discount = d.DiscountAmount,
                                            VAT = d.VATAmount,
                                            NetSales = d.Amount
                                        };

                return salesInvoiceItems.ToList();
            }
        }
    }
}
