using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepPurchaseSummaryReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===================
        // Purchase Order List
        // ===================
        [Authorize, HttpGet, Route("api/purchaseSummaryReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepPurchaseSummaryReport> ListPurchaseSummaryReport(String startDate, String endDate, String companyId, String branchId)
        {
            var purchaseOrders = from d in db.TrnPurchaseOrders
                                 where d.PODate >= Convert.ToDateTime(startDate)
                                 && d.PODate <= Convert.ToDateTime(endDate)
                                 && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                 && d.BranchId == Convert.ToInt32(branchId)
                                 && d.IsLocked == true
                                 select new Entities.RepPurchaseSummaryReport
                                 {
                                     POId = d.Id,
                                     Branch = d.MstBranch.Branch,
                                     PONumber = d.PONumber,
                                     PODate = d.PODate.ToShortDateString(),
                                     Supplier = d.MstArticle.Article,
                                     Term = d.MstTerm.Term,
                                     ManualRequestNumber = d.ManualRequestNumber,
                                     DateNeeded = d.DateNeeded.ToShortDateString(),
                                     IsClose = d.IsClose,
                                     Amount = d.TrnPurchaseOrderItems != null ? d.TrnPurchaseOrderItems.Sum(a => a.Amount) : 0
                                 };

            return purchaseOrders.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/purchaseSummaryReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListPurchaseSummaryReportListCompany()
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
        [Authorize, HttpGet, Route("api/purchaseSummaryReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListPurchaseSummaryReportBranch(String companyId)
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
