using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepPurchaseDetailReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =================================
        // Purchase Order Detail Report List
        // =================================
        [Authorize, HttpGet, Route("api/purchaseDetailReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepPurchaseDetailReport> ListPurchaseDetailReport(String startDate, String endDate, String companyId, String branchId)
        {
            var purchaseOrderItems = from d in db.TrnPurchaseOrderItems
                                     where d.TrnPurchaseOrder.PODate >= Convert.ToDateTime(startDate)
                                     && d.TrnPurchaseOrder.PODate <= Convert.ToDateTime(endDate)
                                     && d.TrnPurchaseOrder.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                     && d.TrnPurchaseOrder.BranchId == Convert.ToInt32(branchId)
                                     && d.TrnPurchaseOrder.IsLocked == true
                                     select new Entities.RepPurchaseDetailReport
                                     {
                                         POId = d.POId,
                                         Branch = d.TrnPurchaseOrder.MstBranch.Branch,
                                         PONumber = d.TrnPurchaseOrder.PONumber,
                                         PODate = d.TrnPurchaseOrder.PODate.ToShortDateString(),
                                         Supplier = d.TrnPurchaseOrder.MstArticle.Article,
                                         Remarks = d.TrnPurchaseOrder.Remarks,
                                         Quantity = d.Quantity,
                                         Unit = d.MstUnit.Unit,
                                         Item = d.MstArticle.Article,
                                         Cost = d.Cost,
                                         Amount = d.Amount
                                     };

            return purchaseOrderItems.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/purchaseDetailReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListPurchaseDetailReportListCompany()
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
        [Authorize, HttpGet, Route("api/purchaseDetailReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListPurchaseDetailReportBranch(String companyId)
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
