using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepDisbursementSummaryReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ================================
        // Disbursement Summary Report List
        // ================================
        [Authorize, HttpGet, Route("api/disbursementSummaryReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepDisbursementSummaryReport> ListDisbursementSummaryReport(String startDate, String endDate, String companyId, String branchId)
        {
            var disbursements = from d in db.TrnDisbursements
                                where d.CVDate >= Convert.ToDateTime(startDate)
                                && d.CVDate <= Convert.ToDateTime(endDate)
                                && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                && d.BranchId == Convert.ToInt32(branchId)
                                && d.IsLocked == true
                                select new Entities.RepDisbursementSummaryReport
                                {
                                    CVId = d.Id,
                                    Branch = d.MstBranch.Branch,
                                    CVNumber = d.CVNumber,
                                    CVDate = d.CVDate.ToShortDateString(),
                                    Payee = d.Payee,
                                    Particulars = d.Particulars,
                                    Bank = d.MstArticle1.Article,
                                    CheckNumber = d.CheckNumber,
                                    CheckDate = d.CheckDate.ToShortDateString(),
                                    Amount = d.Amount
                                };

            return disbursements.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/disbursementSummaryReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListDisbursementSummaryReportListCompany()
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
        [Authorize, HttpGet, Route("api/disbursementSummaryReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListDisbursementSummaryReportBranch(String companyId)
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
