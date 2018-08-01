using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Web.Http;
using System.Diagnostics;

namespace easyfis.ApiControllers
{
    public class ApiRepDisbursementBookController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =============================
        // Disbrusement Book List Report
        // =============================
        [Authorize, HttpGet, Route("api/disbursementBook/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepDisbursementBook> ListDisbursementBook(String startDate, String endDate, String companyId, String branchId)
        {
            var journalsDocumentReferences = from d in db.TrnJournals
                                             where d.JournalDate >= Convert.ToDateTime(startDate)
                                             && d.JournalDate <= Convert.ToDateTime(endDate)
                                             && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                             && d.BranchId == Convert.ToInt32(branchId)
                                             && d.CVId != null
                                             select new
                                             {
                                                 DocumentReference = d.DocumentReference,
                                                 ManualDocumentCode = d.CVId != null ? d.TrnDisbursement.ManualCVNumber : "",
                                                 AccountCode = d.MstAccount.AccountCode,
                                                 Account = d.MstAccount.Account,
                                                 Article = d.MstArticle.Article,
                                                 Particulars = d.Particulars,
                                                 CheckNumber = d.CVId != null ? d.TrnDisbursement.CheckNumber : "",
                                                 CheckDate = d.CVId != null ? d.TrnDisbursement.CheckDate : DateTime.Now,
                                                 Amount = d.CVId != null ? d.TrnDisbursement.Amount : 0,
                                                 DebitAmount = d.DebitAmount,
                                                 CreditAmount = d.CreditAmount,
                                                 Balance = d.DebitAmount - d.CreditAmount
                                             };

            if (journalsDocumentReferences.Any())
            {
                var disbursmentBooks = from d in journalsDocumentReferences
                                       select new Entities.RepDisbursementBook
                                       {
                                           DocumentReference = d.DocumentReference,
                                           ManualDocumentCode =  d.ManualDocumentCode,
                                           AccountCode = d.AccountCode,
                                           Account = d.Account,
                                           Article = d.Article,
                                           Particulars = d.Particulars,
                                           CheckNumber = d.CheckNumber,
                                           CheckDate = d.CheckDate.ToShortDateString(),
                                           Amount = d.Amount,
                                           DebitAmount = d.DebitAmount,
                                           CreditAmount = d.CreditAmount,
                                           Balance = d.DebitAmount - d.CreditAmount
                                       };

                return disbursmentBooks.ToList();
            }
            else
            {
                return new List<Entities.RepDisbursementBook>();
            }
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/disbursementBook/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListDisbrusementBookListCompany()
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
        [Authorize, HttpGet, Route("api/disbursementBook/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListDisbrusementBookListBranch(String companyId)
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

