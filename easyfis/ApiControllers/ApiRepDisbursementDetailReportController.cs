using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepDisbursementDetailReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===============================
        // Disbursement Detail Report List
        // ===============================
        [Authorize, HttpGet, Route("api/disbursementDetailReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepDisbursementDetailReport> ListDisbursementDetailReport(String startDate, String endDate, String companyId, String branchId)
        {
            var disbursementLines = from d in db.TrnDisbursementLines
                                    where d.TrnDisbursement.CVDate >= Convert.ToDateTime(startDate)
                                    && d.TrnDisbursement.CVDate <= Convert.ToDateTime(endDate)
                                    && d.TrnDisbursement.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                    && d.TrnDisbursement.BranchId == Convert.ToInt32(branchId)
                                    && d.TrnDisbursement.IsLocked == true
                                    select new Entities.RepDisbursementDetailReport
                                    {
                                        CVId = d.CVId,
                                        CVBranch = d.TrnDisbursement.MstBranch.Branch,
                                        CVNumber = d.TrnDisbursement.CVNumber,
                                        CVDate = d.TrnDisbursement.CVDate.ToShortDateString(),
                                        Payee = d.TrnDisbursement.Payee,
                                        Branch = d.MstBranch.Branch,
                                        Account = d.MstAccount.Account,
                                        Article = d.MstArticle.Article,
                                        RRNumber = d.RRId != null ? d.TrnReceivingReceipt.RRNumber : " ",
                                        Amount = d.Amount
                                    };

            return disbursementLines.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/disbursementDetailReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListDisbursementDetailReportListCompany()
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
        [Authorize, HttpGet, Route("api/disbursementDetailReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListDisbursementDetailReportBranch(String companyId)
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

