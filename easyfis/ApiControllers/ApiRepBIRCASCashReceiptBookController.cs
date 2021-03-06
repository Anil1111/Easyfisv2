﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepBIRCASCashReceiptBookController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/BIRCASCashReceiptBook/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListBIRCASCashReceiptBookListCompany()
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
        [Authorize, HttpGet, Route("api/BIRCASCashReceiptBook/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListBIRCASCashReceiptBookListBranch(String companyId)
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
        // List Cash Receipt Book Data
        // ===========================
        [Authorize, HttpGet, Route("api/BIRCASCashReceiptBook/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepBIRCASCashReceiptBook> ListBIRCASCashReceiptBook(String startDate, String endDate, String companyId, String branchId)
        {
            if (Convert.ToInt32(branchId) != 0)
            {
                var journals = from d in db.TrnJournals
                               where d.ORId != null
                               && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                               && d.BranchId == Convert.ToInt32(branchId)
                               && d.JournalDate >= Convert.ToDateTime(startDate)
                               && d.JournalDate <= Convert.ToDateTime(endDate)
                               select new Entities.RepBIRCASCashReceiptBook
                               {
                                   Date = d.JournalDate.ToShortDateString(),
                                   ReferenceNumber = "OR-" + d.TrnCollection.MstBranch.BranchCode + "-" + d.TrnCollection.ORNumber,
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
                               where d.ORId != null
                               && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                               && d.JournalDate >= Convert.ToDateTime(startDate)
                               && d.JournalDate <= Convert.ToDateTime(endDate)
                               select new Entities.RepBIRCASCashReceiptBook
                               {
                                   Date = d.JournalDate.ToShortDateString(),
                                   ReferenceNumber = "OR-" + d.TrnCollection.MstBranch.BranchCode + "-" + d.TrnCollection.ORNumber,
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
