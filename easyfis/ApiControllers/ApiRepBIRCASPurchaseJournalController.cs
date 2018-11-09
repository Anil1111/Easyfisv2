using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepBIRCASPurchaseJournalController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/BIRCASPurchaseJournal/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListBIRCASPurchaseJournalListCompany()
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
        [Authorize, HttpGet, Route("api/BIRCASPurchaseJournal/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListBIRCASPurchaseJournalListBranch(String companyId)
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

        // ==========================
        // List Purchase Journal Data
        // ==========================
        [Authorize, HttpGet, Route("api/BIRCASPurchaseJournal/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepBIRCASPurchaseJournal> ListBIRCASPurchaseJournal(String startDate, String endDate, String companyId, String branchId)
        {
            var receivingReceiptItems = from d in db.TrnReceivingReceiptItems
                                        where d.TrnReceivingReceipt.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                        && d.TrnReceivingReceipt.BranchId == Convert.ToInt32(branchId)
                                        && d.TrnReceivingReceipt.RRDate >= Convert.ToDateTime(startDate)
                                        && d.TrnReceivingReceipt.RRDate <= Convert.ToDateTime(endDate)
                                        && (d.TrnReceivingReceipt.IsLocked==true && d.TrnReceivingReceipt.IsCancelled==false)
                                        select new Entities.RepBIRCASPurchaseJournal
                                        {
                                            Date = d.TrnReceivingReceipt.RRDate.ToShortDateString(),
                                            ReferenceNumber = "RR-" + d.TrnReceivingReceipt.RRNumber,
                                            Supplier = d.TrnReceivingReceipt.MstArticle.Article,
                                            SupplierTIN = d.TrnReceivingReceipt.MstArticle.TaxNumber,
                                            Address = d.TrnReceivingReceipt.MstArticle.Address,
                                            ManualReferenceNumber = d.TrnReceivingReceipt.ManualRRNumber,
                                            TotalAmount = d.Amount,
                                            Discount = 0,
                                            VAT = d.VATAmount,
                                            NetPurchase = d.Amount
                                        };

            return receivingReceiptItems.ToList();
        }
    }
}
