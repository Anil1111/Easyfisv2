using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;

namespace easyfis.ApiControllers
{
    public class ApiRepSalesSummaryReportAllFieldsController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ====================================
        // Sales Summary Report All Fields List
        // ====================================
        [Authorize, HttpGet, Route("api/salesSummaryReportAllFields/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepSalesSummaryReportAllFields> ListSalesSummaryReport(String startDate, String endDate, String companyId, String branchId)
        {
            var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                    where d.TrnSalesInvoice.BranchId == Convert.ToInt32(branchId)
                                    && d.TrnSalesInvoice.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                    && d.TrnSalesInvoice.SIDate >= Convert.ToDateTime(startDate)
                                    && d.TrnSalesInvoice.SIDate <= Convert.ToDateTime(endDate)
                                    && d.TrnSalesInvoice.IsLocked == true
                                    select new Entities.RepSalesSummaryReportAllFields
                                    {
                                        Id = d.Id,
                                        Branch = d.TrnSalesInvoice.MstBranch.Branch,
                                        SIId = d.SIId,
                                        SI = d.TrnSalesInvoice.SINumber,
                                        SIDate = d.TrnSalesInvoice.SIDate.ToShortDateString(),
                                        Customer = d.TrnSalesInvoice.MstArticle.Article,
                                        Item = d.MstArticle.Article,
                                        ItemInventory = d.MstArticleInventory.InventoryCode,
                                        Price = d.Price,
                                        Unit = d.MstUnit.Unit,
                                        Quantity = d.Quantity,
                                        Amount = d.Amount,
                                        Discount = d.MstDiscount.Discount,
                                        DiscountRate = d.MstDiscount.DiscountRate,
                                        DiscountAmount = d.DiscountAmount,
                                        VAT = d.MstTaxType.TaxType,
                                        VATPercentage = d.MstTaxType.TaxRate,
                                        VATAmount = d.VATAmount
                                    };

            return salesInvoiceItems.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/salesSummaryReportAllFields/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListSalesSummaryReportAllFieldsListCompany()
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
        [Authorize, HttpGet, Route("api/salesSummaryReportAllFields/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListSalesSummaryReportAllFieldsListBranch(String companyId)
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
    }
}
