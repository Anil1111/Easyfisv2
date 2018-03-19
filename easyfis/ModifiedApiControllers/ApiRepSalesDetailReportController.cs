using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepSalesDetailReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========================
        // Sales Detail Report List
        // ========================
        [Authorize, HttpGet, Route("api/salesDetailReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepSalesDetailReport> ListSalesDetailReport(String startDate, String endDate, String companyId, String branchId)
        {
            var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                    where d.TrnSalesInvoice.BranchId == Convert.ToInt32(branchId)
                                    && d.TrnSalesInvoice.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                    && d.TrnSalesInvoice.SIDate >= Convert.ToDateTime(startDate)
                                    && d.TrnSalesInvoice.SIDate <= Convert.ToDateTime(endDate)
                                    && d.TrnSalesInvoice.IsLocked == true
                                    select new Entities.RepSalesDetailReport
                                    {
                                        SIId = d.SIId,
                                        Branch = d.TrnSalesInvoice.MstBranch.Branch,
                                        SINumber = d.TrnSalesInvoice.SINumber,
                                        SIDate = d.TrnSalesInvoice.SIDate.ToShortDateString(),
                                        Customer = d.TrnSalesInvoice.MstArticle.Article,
                                        Sales = d.TrnSalesInvoice.MstUser4.FullName,
                                        ItemCode = d.MstArticle.ManualArticleCode,
                                        ItemManualArticleOldCode = d.MstArticle.ManualArticleOldCode,
                                        Item = d.MstArticle.Article,
                                        ItemCategory = d.MstArticle.Category,
                                        Unit = d.MstUnit.Unit,
                                        Quantity = d.Quantity,
                                        Price = d.Price,
                                        DiscountAmount = d.DiscountAmount,
                                        NetPrice = d.NetPrice,
                                        Amount = d.Amount,
                                        VATAmount = d.VATAmount
                                    };

            return salesInvoiceItems.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/salesDetailReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListSalesDetailReportListCompany()
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
        [Authorize, HttpGet, Route("api/salesDetailReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListSalesDetailReportBranch(String companyId)
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
