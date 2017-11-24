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
        public List<Entities.TrnJournal> ListAccountLedger(String startDate, String endDate, String companyId, String branchId, String accountId)
        {
            try
            {
                var journals = from d in db.TrnJournals
                               where d.JournalDate >= Convert.ToDateTime(startDate)
                               && d.JournalDate <= Convert.ToDateTime(endDate)
                               && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                               && d.BranchId == Convert.ToInt32(branchId)
                               && d.AccountId == Convert.ToInt32(accountId)
                               select new Entities.TrnJournal
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
    }
}
