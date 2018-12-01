using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepBIRCASDisbursementBookController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/BIRCASDisbursementBook/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListBIRCASDisbursementBookListCompany()
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
        [Authorize, HttpGet, Route("api/BIRCASDisbursementBook/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListBIRCASDisbursementBookListBranch(String companyId)
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

        // ===========================
        // List Disbursement Book Data
        // ===========================
        [Authorize, HttpGet, Route("api/BIRCASDisbursementBook/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepBIRCASDisbursementBook> ListBIRCASDisbursementBook(String startDate, String endDate, String companyId, String branchId)
        {
            if (Convert.ToInt32(branchId) != 0)
            {
                var journals = from d in db.TrnJournals
                               where d.CVId != null
                               && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                               && d.BranchId == Convert.ToInt32(branchId)
                               && d.JournalDate >= Convert.ToDateTime(startDate)
                               && d.JournalDate <= Convert.ToDateTime(endDate)
                               select new Entities.RepBIRCASDisbursementBook
                               {
                                   Date = d.JournalDate.ToShortDateString(),
                                   ReferenceNumber = "CV-" + d.TrnDisbursement.MstBranch.BranchCode + "-" + d.TrnDisbursement.CVNumber,
                                   Supplier = d.TrnDisbursement.MstArticle.Article,
                                   TIN = d.TrnDisbursement.MstArticle.TaxNumber,
                                   Address = d.TrnDisbursement.MstArticle.Address,
                                   AccountCode = d.MstAccount.AccountCode,
                                   Account = d.MstAccount.Account,
                                   DebitAmount = d.DebitAmount,
                                   CreditAmount = d.CreditAmount
                               };

                return journals.ToList();
            }
            else
            {
                var journals = from d in db.TrnJournals
                               where d.CVId != null
                               && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                               && d.JournalDate >= Convert.ToDateTime(startDate)
                               && d.JournalDate <= Convert.ToDateTime(endDate)
                               select new Entities.RepBIRCASDisbursementBook
                               {
                                   Date = d.JournalDate.ToShortDateString(),
                                   ReferenceNumber = "CV-" + d.TrnDisbursement.MstBranch.BranchCode + "-" + d.TrnDisbursement.CVNumber,
                                   Supplier = d.TrnDisbursement.MstArticle.Article,
                                   TIN = d.TrnDisbursement.MstArticle.TaxNumber,
                                   Address = d.TrnDisbursement.MstArticle.Address,
                                   AccountCode = d.MstAccount.AccountCode,
                                   Account = d.MstAccount.Account,
                                   DebitAmount = d.DebitAmount,
                                   CreditAmount = d.CreditAmount
                               };

                return journals.ToList();
            }
        }
    }
}
