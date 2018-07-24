using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepCollectionSummaryReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==============================
        // Collection Summary Report List
        // ==============================
        [Authorize, HttpGet, Route("api/collectionSummaryReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepCollectionSummaryReport> ListCollectionSummaryReport(String startDate, String endDate, String companyId, String branchId)
        {
            var collections = from d in db.TrnCollections
                              where d.BranchId == Convert.ToInt32(branchId)
                              && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                              && d.ORDate >= Convert.ToDateTime(startDate)
                              && d.ORDate <= Convert.ToDateTime(endDate)
                              && d.IsLocked == true
                              select new Entities.RepCollectionSummaryReport
                              {
                                  ORId = d.Id,
                                  Branch = d.MstBranch.Branch,
                                  ORNumber = d.ORNumber,
                                  ORDate = d.ORDate.ToShortDateString(),
                                  Customer = d.MstArticle.Article,
                                  ManualORNumber = d.ManualORNumber,
                                  Amount = d.TrnCollectionLines != null ? d.TrnCollectionLines.Sum(a => a.Amount) : 0
                              };

            return collections.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/collectionSummaryReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListCollectionSummaryReportListCompany()
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
        [Authorize, HttpGet, Route("api/collectionSummaryReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListCollectionSummaryReportListBranch(String companyId)
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
