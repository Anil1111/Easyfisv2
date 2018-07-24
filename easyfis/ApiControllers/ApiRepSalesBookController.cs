﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepSalesBookController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ======================
        // Sales Book List Report
        // ======================
        [Authorize, HttpGet, Route("api/salesBook/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepSalesBook> ListSalesBook(String startDate, String endDate, String companyId, String branchId)
        {
            var journalsDocumentReferences = from d in db.TrnJournals
                                             where d.JournalDate >= Convert.ToDateTime(startDate)
                                             && d.JournalDate <= Convert.ToDateTime(endDate)
                                             && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                             && d.BranchId == Convert.ToInt32(branchId)
                                             && d.SIId != null
                                             select new Entities.RepSalesBook
                                             {
                                                 DocumentReference = d.DocumentReference,
                                                 ManualDocumentCode = d.SIId != null ? d.TrnSalesInvoice.ManualSINumber : "",
                                                 AccountCode = d.MstAccount.AccountCode,
                                                 Account = d.MstAccount.Account,
                                                 Article = d.MstArticle.Article,
                                                 Particulars = d.Particulars,
                                                 DebitAmount = d.DebitAmount,
                                                 CreditAmount = d.CreditAmount,
                                                 Balance = d.DebitAmount - d.CreditAmount
                                             };

            return journalsDocumentReferences.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/salesBook/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListSalesBookListCompany()
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
        [Authorize, HttpGet, Route("api/salesBook/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListSalesBookListBranch(String companyId)
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
