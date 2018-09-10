using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepBIRCASGeneralLedgerController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/BIRCASGeneralLedger/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListBIRCASGeneralLedgerListCompany()
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
        [Authorize, HttpGet, Route("api/BIRCASGeneralLedger/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListBIRCASGeneralLedgerListBranch(String companyId)
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

        // ========================
        // List General Ledger Data
        // ========================
        [Authorize, HttpGet, Route("api/BIRCASGeneralLedger/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepBIRCASGeneralLedger> ListBIRCASGeneralLedger(String startDate, String endDate, String companyId, String branchId)
        {
            var journals = from d in db.TrnJournals
                           where d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                           && d.BranchId == Convert.ToInt32(branchId)
                           && d.JournalDate >= Convert.ToDateTime(startDate)
                           && d.JournalDate <= Convert.ToDateTime(endDate)
                           select new Entities.RepBIRCASGeneralLedger
                           {
                               Date = d.JournalDate.ToShortDateString(),
                               ReferenceNumber = d.ORId != null ? "OR-" + d.TrnCollection.ORNumber :
                                                 d.CVId != null ? "CV-" + d.TrnDisbursement.CVNumber :
                                                 d.JVId != null ? "JV-" + d.TrnJournalVoucher.JVNumber :
                                                 d.RRId != null ? "RR-" + d.TrnReceivingReceipt.RRNumber :
                                                 d.SIId != null ? "SI-" + d.TrnSalesInvoice.SINumber :
                                                 d.INId != null ? "IN-" + d.TrnStockIn.INNumber :
                                                 d.OTId != null ? "OT-" + d.TrnStockOut.OTNumber :
                                                 d.STId != null ? "ST-" + d.TrnStockTransfer.STNumber :
                                                 d.SWId != null ? "SW-" + d.TrnStockWithdrawal.SWNumber :
                                                 "0000000000",
                               AccountCode = d.MstAccount.AccountCode,
                               Account = d.MstAccount.Account,
                               DebitAmount = d.DebitAmount,
                               CreditAmount = d.CreditAmount
                           };

            return journals.ToList();
        }
    }
}
