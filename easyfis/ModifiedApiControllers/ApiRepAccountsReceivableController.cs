using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepAccountsReceivableController : ApiController
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

        // ===============================
        // Accounts Receivable Report list
        // ===============================
        [Authorize, HttpGet, Route("api/accountsReceivable/list/{dateAsOf}/{companyId}/{branchId}/{accountId}")]
        public List<Entities.RepAccountsReceivable> ListAccountsReceivable(String dateAsOf, String companyId, String branchId, String accountId)
        {
            try
            {
                var salesInvoice = from d in db.TrnSalesInvoices
                                   where d.SIDate <= Convert.ToDateTime(dateAsOf)
                                   && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                   && d.BranchId == Convert.ToInt32(branchId)
                                   && d.MstArticle.AccountId == Convert.ToInt32(accountId)
                                   && d.BalanceAmount > 0
                                   && d.IsLocked == true
                                   select new Entities.RepAccountsReceivable
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

        // ===============================
        // Dropdown List - Company (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/accountsReceivable/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListAccountsReceivableListCompany()
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
        [Authorize, HttpGet, Route("api/accountsReceivable/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListAccountsReceivableBranch(String companyId)
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
        [Authorize, HttpGet, Route("api/accountsReceivable/dropdown/list/articleGroupAccount")]
        public List<Entities.MstArticleGroup> DropdownListAccountsReceivableArticleGroupAccount()
        {
            var articleGroups = from d in db.MstArticleGroups.OrderBy(d => d.ArticleGroup)
                                where d.ArticleTypeId == 2
                                group d by new
                                {
                                    AccountId = d.AccountId,
                                    AccountCode = d.MstAccount.AccountCode,
                                    Account = d.MstAccount.Account
                                } into g
                                select new Entities.MstArticleGroup
                                {
                                    AccountId = g.Key.AccountId,
                                    Account = g.Key.Account
                                };

            return articleGroups.ToList();
        }
    }
}
