using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepBIRCASInventoryJournalController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/BIRCASInventoryJournal/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListBIRCASInventoryJournalListCompany()
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
        [Authorize, HttpGet, Route("api/BIRCASInventoryJournal/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListBIRCASInventoryJournalListBranch(String companyId)
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
        // List Inventory Journal Data
        // ===========================
        [Authorize, HttpGet, Route("api/BIRCASInventoryJournal/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepBIRCASInventoryJournal> ListBIRCASInventoryJournal(String startDate, String endDate, String companyId, String branchId)
        {
            var inventories = from d in db.TrnInventories
                              where d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                              && d.BranchId == Convert.ToInt32(branchId)
                              && d.InventoryDate >= Convert.ToDateTime(startDate)
                              && d.InventoryDate <= Convert.ToDateTime(endDate)
                              select new Entities.RepBIRCASInventoryJournal
                              {
                                  Date = d.InventoryDate.ToShortDateString(),
                                  ReferenceNumber = d.RRId != null ? "RR-" + d.TrnReceivingReceipt.RRNumber :
                                                    d.SIId != null ? "SI-" + d.TrnSalesInvoice.SINumber :
                                                    d.INId != null ? "IN-" + d.TrnStockIn.INNumber :
                                                    d.OTId != null ? "OT-" + d.TrnStockOut.OTNumber :
                                                    d.STId != null ? "ST-" + d.TrnStockTransfer.STNumber :
                                                    d.SWId != null ? "SW-" + d.TrnStockWithdrawal.SWNumber :
                                                    "0000000000",
                                  AccountCode = d.MstArticle.MstAccount.AccountCode,
                                  Account = d.MstArticle.MstAccount.Account,
                                  Item = d.MstArticle.Article,
                                  DebitAmount = d.Quantity > 0 ? d.Amount : 0,
                                  CreditAmount = d.Quantity < 0 ? d.Amount : 0
                              };

            return inventories.ToList();
        }
    }
}