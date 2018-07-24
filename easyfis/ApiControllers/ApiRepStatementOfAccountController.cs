﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ModifiedApiControllers
{
    public class ApiRepStatementOfAccountController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =============
        // Compute Aging
        // =============
        public Decimal ComputeAge(Int32 Age, Int32 Elapsed, Decimal Amount)
        {
            Decimal returnValue = 0;

            if (Age == 0)
            {
                if (Elapsed < 30)
                {
                    returnValue = Amount;
                }
            }
            else if (Age == 1)
            {
                if (Elapsed >= 30 && Elapsed < 60)
                {
                    returnValue = Amount;
                }
            }
            else if (Age == 2)
            {
                if (Elapsed >= 60 && Elapsed < 90)
                {
                    returnValue = Amount;
                }
            }
            else if (Age == 3)
            {
                if (Elapsed >= 90 && Elapsed < 120)
                {
                    returnValue = Amount;
                }
            }
            else if (Age == 4)
            {
                if (Elapsed >= 120)
                {
                    returnValue = Amount;
                }
            }
            else
            {
                returnValue = 0;
            }

            return returnValue;
        }

        // =========================
        // Statement of Account list
        // =========================
        [Authorize]
        [HttpGet]
        [Route("api/statementOfAccount/list/{dateAsOf}/{companyId}/{branchId}/{customerId}")]
        public List<Models.TrnSalesInvoice> ListStatementOfAccount(String dateAsOf, String companyId, String branchId, String customerId)
        {
            try
            {
                var salesInvoice = from d in db.TrnSalesInvoices
                                   where d.SIDate <= Convert.ToDateTime(dateAsOf)
                                   && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                   && d.BranchId == Convert.ToInt32(branchId)
                                   && d.CustomerId == Convert.ToInt32(customerId)
                                   && d.BalanceAmount > 0
                                   && d.IsLocked == true
                                   select new Models.TrnSalesInvoice
                                   {
                                       Id = d.Id,
                                       Branch = d.MstBranch.Branch,
                                       AccountId = d.MstArticle.AccountId,
                                       AccountCode = d.MstArticle.MstAccount.AccountCode,
                                       Account = d.MstArticle.MstAccount.Account,
                                       SINumber = d.SINumber,
                                       SIDate = d.SIDate.ToShortDateString(),
                                       DocumentReference = d.DocumentReference,
                                       BalanceAmount = d.BalanceAmount,
                                       CustomerId = d.CustomerId,
                                       Customer = d.MstArticle.Article,
                                       DueDate = d.SIDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays)).ToShortDateString(),
                                       NumberOfDaysFromDueDate = Convert.ToDateTime(dateAsOf).Subtract(d.SIDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days,
                                       CurrentAmount = ComputeAge(0, Convert.ToDateTime(dateAsOf).Subtract(d.SIDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount),
                                       Age30Amount = ComputeAge(1, Convert.ToDateTime(dateAsOf).Subtract(d.SIDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount),
                                       Age60Amount = ComputeAge(2, Convert.ToDateTime(dateAsOf).Subtract(d.SIDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount),
                                       Age90Amount = ComputeAge(3, Convert.ToDateTime(dateAsOf).Subtract(d.SIDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount),
                                       Age120Amount = ComputeAge(4, Convert.ToDateTime(dateAsOf).Subtract(d.SIDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount)
                                   };

                return salesInvoice.ToList();
            }
            catch
            {
                return null;
            }
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/statementOfAccount/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListStatementOfAccountListCompany()
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
        [Authorize, HttpGet, Route("api/statementOfAccount/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListStatementOfAccountListBranch(String companyId)
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

        // =================================
        // Dropdown List - Customer (Filter)
        // =================================
        [Authorize, HttpGet, Route("api/statementOfAccount/dropdown/list/customer")]
        public List<Entities.MstArticle> DropdownListStatementOfAccountListCustomer()
        {
            var customers = from d in db.MstArticles.OrderBy(d => d.Article)
                            where d.ArticleTypeId == 2
                            && d.IsLocked == true
                            select new Entities.MstArticle
                            {
                                Id = d.Id,
                                Article = d.Article
                            };

            return customers.ToList();
        }
    }
}
