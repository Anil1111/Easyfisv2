using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepAccountLedgerController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===================
        // Account Ledger List
        // ===================
        [Authorize, HttpGet, Route("api/accountLedger/list/{startDate}/{endDate}/{companyId}/{branchId}/{accountId}")]
        public List<Entities.RepAccountLedger> ListAccountLedger(String startDate, String endDate, String companyId, String branchId, String accountId)
        {
            try
            {
                var journals = from d in db.TrnJournals
                               where d.JournalDate >= Convert.ToDateTime(startDate)
                               && d.JournalDate <= Convert.ToDateTime(endDate)
                               && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                               && d.BranchId == Convert.ToInt32(branchId)
                               && d.AccountId == Convert.ToInt32(accountId)
                               select new Entities.RepAccountLedger
                               {
                                   JournalDate = d.JournalDate.ToShortDateString(),
                                   DocumentReference = d.DocumentReference,
                                   Article = d.MstArticle.Article,
                                   Particulars = d.Particulars,
                                   DebitAmount = d.DebitAmount,
                                   CreditAmount = d.CreditAmount,
                                   Balance = d.DebitAmount - d.CreditAmount,
                                   ORId = d.ORId,
                                   CVId = d.CVId,
                                   JVId = d.JVId,
                                   RRId = d.RRId,
                                   SIId = d.SIId,
                                   INId = d.INId,
                                   OTId = d.OTId,
                                   STId = d.STId
                               };

                return journals.ToList();
            }
            catch
            {
                return null;
            }
        }

        // ===============================
        // Dropdown List - Company (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/accountLedger/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListAccountLedgerListCompany()
        {
            var companies = from d in db.MstCompanies.OrderBy(d => d.Company)
                            select new Entities.MstCompany
                            {
                                Id = d.Id,
                                Company = d.Company
                            };

            return companies.ToList();
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/accountLedger/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListAccountLedgerBranch(String companyId)
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

        // ===============================
        // Dropdown List - Account (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/accountLedger/dropdown/list/account")]
        public List<Entities.MstAccount> DropdownListAccountLedgerAccount()
        {
            var accounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                           where d.IsLocked == true
                           select new Entities.MstAccount
                           {
                               Id = d.Id,
                               AccountCode = d.AccountCode,
                               Account = d.Account
                           };

            return accounts.ToList();
        }
    }
}
