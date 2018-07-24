using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;

namespace easyfis.ApiControllers
{
    public class ApiRepSalesSummaryReportSalesNoController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ======================================
        // Sales Summary Report By Sales No. List
        // ======================================
        [Authorize, HttpGet, Route("api/salesSummaryReportSalesNo/list/{startSalesNo}/{endSalesNo}/{companyId}/{branchId}")]
        public List<Entities.RepSalesSummaryReportBySalesNo> ListSalesSummaryBySalesNoReport(String startSalesNo, String endSalesNo, String companyId, String branchId)
        {
            var salesInvoices = from d in db.TrnSalesInvoices
                                where d.BranchId == Convert.ToInt32(branchId)
                                && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                && Convert.ToInt32(d.SINumber) >= Convert.ToInt32(startSalesNo)
                                && Convert.ToInt32(d.SINumber) <= Convert.ToInt32(endSalesNo)
                                && d.IsLocked == true
                                select new Entities.RepSalesSummaryReportBySalesNo
                                {
                                    Id = d.Id,
                                    Branch = d.MstBranch.Branch,
                                    SINumber = d.SINumber,
                                    SIDate = d.SIDate.ToShortDateString(),
                                    Customer = d.MstArticle.Article,
                                    Remarks = d.Remarks,
                                    SoldBy = d.MstUser4.FullName,
                                    Amount = d.Amount
                                };

            return salesInvoices.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/salesSummaryReportSalesNo/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListSalesSummaryBySalesNoReportListCompany()
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
        [Authorize, HttpGet, Route("api/salesSummaryReportSalesNo/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListSalesSummaryBySalesNoReportListBranch(String companyId)
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
