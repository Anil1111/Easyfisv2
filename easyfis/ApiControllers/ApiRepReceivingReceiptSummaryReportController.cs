using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepReceivingReceiptSummaryReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =====================================
        // Receiving Receipt Summary Report List
        // =====================================
        [Authorize, HttpGet, Route("api/receivingReceiptSummaryReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepReceivingReceiptSummaryReport> ListReceivingReceiptSummaryReport(String startDate, String endDate, String companyId, String branchId)
        {
            var receivingReceipts = from d in db.TrnReceivingReceipts
                                    where d.RRDate >= Convert.ToDateTime(startDate)
                                    && d.RRDate <= Convert.ToDateTime(endDate)
                                    && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                    && d.BranchId == Convert.ToInt32(branchId)
                                    && d.IsLocked == true
                                    select new Entities.RepReceivingReceiptSummaryReport
                                    {

                                        RRId = d.Id,
                                        Branch = d.MstBranch.Branch,
                                        RRNumber = d.RRNumber,
                                        RRDate = d.RRDate.ToShortDateString(),
                                        Supplier = d.MstArticle.Article,
                                        Term = d.MstArticle.MstTerm.Term,
                                        Remarks = d.Remarks,
                                        DocumentReference = d.DocumentReference,
                                        Amount = d.Amount,
                                        WTaxAmount = d.WTaxAmount,
                                        RRAmount = d.Amount - d.WTaxAmount
                                    };

            return receivingReceipts.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/receivingReceiptSummaryReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListReceivingReceiptSummaryReportListCompany()
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
        [Authorize, HttpGet, Route("api/receivingReceiptSummaryReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListReceivingReceiptSummaryReportBranch(String companyId)
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

