using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Web.Http;
using System.Globalization;

namespace easyfis.ApiControllers
{
    public class ApiRepSalesSummaryReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ================================
        // Get Latest Sales Item Time Stamp
        // ================================
        public String GetSalesTime(Int32 SIId)
        {
            var salesInvoiceItems = from d in db.TrnSalesInvoiceItems.OrderByDescending(d => d.SalesItemTimeStamp)
                                    where d.SIId == SIId
                                    select d;

            if (salesInvoiceItems.Any())
            {
                return salesInvoiceItems.FirstOrDefault().SalesItemTimeStamp.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture);
            }
            else
            {
                return " ";
            }
        }

        // =========================
        // Sales Summary Report List
        // =========================
        [Authorize, HttpGet, Route("api/salesSummaryReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepSalesSummaryReport> ListSalesSummaryReport(String startDate, String endDate, String companyId, String branchId)
        {
            var salesInvoices = from d in db.TrnSalesInvoices
                                where d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                && d.BranchId == Convert.ToInt32(branchId)
                                && d.SIDate >= Convert.ToDateTime(startDate)
                                && d.SIDate <= Convert.ToDateTime(endDate)
                                && d.IsLocked == true
                                select new Entities.RepSalesSummaryReport
                                {
                                    SIId = d.Id,
                                    Branch = d.MstBranch.Branch,
                                    SINumber = d.SINumber,
                                    SIDate = d.SIDate.ToShortDateString(),
                                    Customer = d.MstArticle.Article,
                                    Term = d.MstTerm.Term,
                                    DocumentReference = d.DocumentReference,
                                    Sales = d.MstUser4.FullName,
                                    Time = GetSalesTime(d.Id),
                                    Amount = d.Amount
                                };

            return salesInvoices.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/salesSummaryReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListSalesSummaryReportListCompany()
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
        [Authorize, HttpGet, Route("api/salesSummaryReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListSalesSummaryReportBranch(String companyId)
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
