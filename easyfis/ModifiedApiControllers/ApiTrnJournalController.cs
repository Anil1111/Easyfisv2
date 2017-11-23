using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ModifiedApiControllers
{
    public class ApiTrnJournalController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ================================
        // List Journal - Receiving Receipt
        // ================================
        [Authorize, HttpGet, Route("api/jounal/receivingReceipt/list/{RRId}")]
        public List<Entities.TrnJournal> ListJournalReceivingReceipt(String RRId)
        {
            var journals = from d in db.TrnJournals
                           where d.RRId == Convert.ToInt32(RRId)
                           select new Entities.TrnJournal
                           {
                               Branch = d.MstBranch.Branch,
                               JournalDate = d.JournalDate.ToShortDateString(),
                               AccountCode = d.MstAccount.AccountCode,
                               Account = d.MstAccount.Account,
                               Article = d.MstArticle.Article,
                               DebitAmount = d.DebitAmount,
                               CreditAmount = d.CreditAmount
                           };

            return journals.ToList();
        }

        // ============================
        // List Journal - Sales Invoice
        // ============================
        [Authorize, HttpGet, Route("api/jounal/salesInvoice/list/{SIId}")]
        public List<Entities.TrnJournal> ListJournalSalesInvoice(String SIId)
        {
            var journals = from d in db.TrnJournals
                           where d.SIId == Convert.ToInt32(SIId)
                           select new Entities.TrnJournal
                           {
                               Branch = d.MstBranch.Branch,
                               JournalDate = d.JournalDate.ToShortDateString(),
                               AccountCode = d.MstAccount.AccountCode,
                               Account = d.MstAccount.Account,
                               Article = d.MstArticle.Article,
                               DebitAmount = d.DebitAmount,
                               CreditAmount = d.CreditAmount
                           };

            return journals.ToList();
        }

        // ===========================
        // List Journal - Disbursement
        // ===========================
        [Authorize, HttpGet, Route("api/jounal/disbursement/list/{CVId}")]
        public List<Entities.TrnJournal> ListJournalDisbursement(String CVId)
        {
            var journals = from d in db.TrnJournals
                           where d.CVId == Convert.ToInt32(CVId)
                           select new Entities.TrnJournal
                           {
                               Branch = d.MstBranch.Branch,
                               JournalDate = d.JournalDate.ToShortDateString(),
                               AccountCode = d.MstAccount.AccountCode,
                               Account = d.MstAccount.Account,
                               Article = d.MstArticle.Article,
                               DebitAmount = d.DebitAmount,
                               CreditAmount = d.CreditAmount
                           };

            return journals.ToList();
        }

        // =========================
        // List Journal - Collection
        // =========================
        [Authorize, HttpGet, Route("api/jounal/collection/list/{ORId}")]
        public List<Entities.TrnJournal> ListJournalCollection(String ORId)
        {
            var journals = from d in db.TrnJournals
                           where d.ORId == Convert.ToInt32(ORId)
                           select new Entities.TrnJournal
                           {
                               Branch = d.MstBranch.Branch,
                               JournalDate = d.JournalDate.ToShortDateString(),
                               AccountCode = d.MstAccount.AccountCode,
                               Account = d.MstAccount.Account,
                               Article = d.MstArticle.Article,
                               DebitAmount = d.DebitAmount,
                               CreditAmount = d.CreditAmount
                           };

            return journals.ToList();
        }

        // ==============================
        // List Journal - Journal Voucher
        // ==============================
        [Authorize, HttpGet, Route("api/jounal/journalVoucher/list/{JVId}")]
        public List<Entities.TrnJournal> ListJournalJournalVoucher(String JVId)
        {
            var journals = from d in db.TrnJournals
                           where d.JVId == Convert.ToInt32(JVId)
                           select new Entities.TrnJournal
                           {
                               Branch = d.MstBranch.Branch,
                               JournalDate = d.JournalDate.ToShortDateString(),
                               AccountCode = d.MstAccount.AccountCode,
                               Account = d.MstAccount.Account,
                               Article = d.MstArticle.Article,
                               DebitAmount = d.DebitAmount,
                               CreditAmount = d.CreditAmount
                           };

            return journals.ToList();
        }

        // =======================
        // List Journal - Stock In
        // =======================
        [Authorize, HttpGet, Route("api/jounal/stockIn/list/{INId}")]
        public List<Entities.TrnJournal> ListJournalStockIn(String INId)
        {
            var journals = from d in db.TrnJournals
                           where d.INId == Convert.ToInt32(INId)
                           select new Entities.TrnJournal
                           {
                               Branch = d.MstBranch.Branch,
                               JournalDate = d.JournalDate.ToShortDateString(),
                               AccountCode = d.MstAccount.AccountCode,
                               Account = d.MstAccount.Account,
                               Article = d.MstArticle.Article,
                               DebitAmount = d.DebitAmount,
                               CreditAmount = d.CreditAmount
                           };

            return journals.ToList();
        }

        // ========================
        // List Journal - Stock Out
        // ========================
        [Authorize, HttpGet, Route("api/jounal/stockOut/list/{OTId}")]
        public List<Entities.TrnJournal> ListJournalStockOut(String OTId)
        {
            var journals = from d in db.TrnJournals
                           where d.OTId == Convert.ToInt32(OTId)
                           select new Entities.TrnJournal
                           {
                               Branch = d.MstBranch.Branch,
                               JournalDate = d.JournalDate.ToShortDateString(),
                               AccountCode = d.MstAccount.AccountCode,
                               Account = d.MstAccount.Account,
                               Article = d.MstArticle.Article,
                               DebitAmount = d.DebitAmount,
                               CreditAmount = d.CreditAmount
                           };

            return journals.ToList();
        }
    }
}
