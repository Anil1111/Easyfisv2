using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;

namespace easyfis.ModifiedApiControllers
{
    public class ApiRepCashFlowController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==============
        // Cash Flow List
        // ==============
        [Authorize, HttpGet, Route("api/cashFlow/list/{startDate}/{endDate}/{companyId}")]
        public List<Entities.RepCashFlow> ListCashFlow(String startDate, String endDate, String companyId)
        {
            List<Entities.RepCashFlow> listCashFlow = new List<Entities.RepCashFlow>();

            // ================
            // Cash Flow Income
            // ================
            var cashFlowIncome = from d in db.TrnJournals
                                 where d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                 && d.JournalDate >= Convert.ToDateTime(startDate)
                                 && d.JournalDate <= Convert.ToDateTime(endDate)
                                 && (d.MstAccount.MstAccountType.AccountCategoryId == 5 || d.MstAccount.MstAccountType.AccountCategoryId == 6)
                                 select new
                                 {
                                     AccountCashFlowCode = d.MstAccount.MstAccountCashFlow.AccountCashFlowCode,
                                     AccountCashFlow = d.MstAccount.MstAccountCashFlow.AccountCashFlow,
                                     AccountTypeCode = d.MstAccount.MstAccountType.AccountTypeCode,
                                     AccountType = d.MstAccount.MstAccountType.AccountType,
                                     AccountCode = d.MstAccount.AccountCode,
                                     Account = d.MstAccount.Account,
                                     DebitAmount = d.DebitAmount,
                                     CreditAmount = d.CreditAmount
                                 };

            // =======================
            // Cash Flow Balance Sheet
            // =======================
            var cashFlowBalanceSheet = from d in db.TrnJournals
                                       where d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                       && d.JournalDate >= Convert.ToDateTime(startDate)
                                       && d.JournalDate <= Convert.ToDateTime(endDate)
                                       && d.MstAccount.MstAccountType.AccountCategoryId < 5
                                       && d.MstAccount.AccountCashFlowId <= 3
                                       select new
                                       {
                                           AccountCashFlowCode = d.MstAccount.MstAccountCashFlow.AccountCashFlowCode,
                                           AccountCashFlow = d.MstAccount.MstAccountCashFlow.AccountCashFlow,
                                           AccountTypeCode = d.MstAccount.MstAccountType.AccountTypeCode,
                                           AccountType = d.MstAccount.MstAccountType.AccountType,
                                           AccountCode = d.MstAccount.AccountCode,
                                           Account = d.MstAccount.Account,
                                           DebitAmount = d.DebitAmount,
                                           CreditAmount = d.CreditAmount
                                       };

            // ==========
            // Cash Flows
            // ==========
            var groupedCashFlowIncome = from d in cashFlowIncome
                                        group d by new
                                        {
                                            AccountCashFlowCode = d.AccountCashFlowCode,
                                            AccountCashFlow = d.AccountCashFlow
                                        } into g
                                        select new
                                        {
                                            AccountCashFlowCode = g.Key.AccountCashFlowCode,
                                            AccountCashFlow = g.Key.AccountCashFlow
                                        };

            var groupedCashFlowBalanceSheet = from d in cashFlowBalanceSheet
                                              group d by new
                                              {
                                                  AccountCashFlowCode = d.AccountCashFlowCode,
                                                  AccountCashFlow = d.AccountCashFlow
                                              } into g
                                              select new
                                              {
                                                  AccountCashFlowCode = g.Key.AccountCashFlowCode,
                                                  AccountCashFlow = g.Key.AccountCashFlow
                                              };

            // ================
            // Union Cash Flows
            // ================
            var unionCashFlows = groupedCashFlowIncome.Union(groupedCashFlowBalanceSheet).OrderBy(d => d.AccountCashFlowCode);

            if (unionCashFlows.Any())
            {
                foreach (var unionCashFlow in unionCashFlows)
                {
                    // ======================
                    // Default Income Account
                    // ======================
                    var identityUserId = User.Identity.GetUserId();
                    var mstUserId = from d in db.MstUsers where d.UserId == identityUserId select d;
                    var incomeAccount = from d in db.MstAccounts where d.Id == mstUserId.FirstOrDefault().IncomeAccountId select d;

                    // =============
                    // Account Types
                    // =============
                    var groupedAccountTypesIncome = from d in cashFlowIncome
                                                    group d by new
                                                    {
                                                        AccountCashFlowCode = d.AccountCashFlowCode,
                                                        AccountCashFlow = d.AccountCashFlow,
                                                        AccountTypeCode = incomeAccount.FirstOrDefault().MstAccountType.AccountTypeCode,
                                                        AccountType = incomeAccount.FirstOrDefault().MstAccountType.AccountType,
                                                        AccountCode = "0000",
                                                        Account = incomeAccount.FirstOrDefault().Account,
                                                    } into g
                                                    select new
                                                    {
                                                        AccountCashFlowCode = g.Key.AccountCashFlowCode,
                                                        AccountCashFlow = g.Key.AccountCashFlow,
                                                        AccountTypeCode = g.Key.AccountTypeCode,
                                                        AccountType = g.Key.AccountType,
                                                        AccountCode = g.Key.AccountCode,
                                                        Account = g.Key.Account,
                                                        DebitAmount = g.Sum(d => d.DebitAmount),
                                                        CreditAmount = g.Sum(d => d.CreditAmount),
                                                        Balance = g.Sum(d => d.CreditAmount - d.DebitAmount)
                                                    };

                    var groupedAccountTypesBalanceSheet = from d in cashFlowBalanceSheet
                                                          group d by new
                                                          {
                                                              AccountCashFlowCode = d.AccountCashFlowCode,
                                                              AccountCashFlow = d.AccountCashFlow,
                                                              AccountTypeCode = d.AccountTypeCode,
                                                              AccountType = d.AccountType,
                                                              AccountCode = d.AccountCode,
                                                              Account = d.Account
                                                          } into g
                                                          select new
                                                          {
                                                              AccountCashFlowCode = g.Key.AccountCashFlowCode,
                                                              AccountCashFlow = g.Key.AccountCashFlow,
                                                              AccountTypeCode = g.Key.AccountTypeCode,
                                                              AccountType = g.Key.AccountType,
                                                              AccountCode = g.Key.AccountCode,
                                                              Account = g.Key.Account,
                                                              DebitAmount = g.Sum(d => d.DebitAmount),
                                                              CreditAmount = g.Sum(d => d.CreditAmount),
                                                              Balance = g.Sum(d => d.CreditAmount - d.DebitAmount)
                                                          };

                    var unionAccountTypes = groupedAccountTypesIncome.Union(groupedAccountTypesBalanceSheet).Where(d => d.AccountCashFlow.Equals(unionCashFlow.AccountCashFlow)).OrderBy(d => d.AccountCode);

                    if (unionAccountTypes.Any())
                    {
                        foreach (var unionAccountType in unionAccountTypes)
                        {

                            // ========
                            // Accounts
                            // ========
                            var groupedAccountIncome = from d in groupedAccountTypesIncome
                                                       where d.AccountCode.Equals(unionAccountType.AccountCode)
                                                       group d by new
                                                       {
                                                           AccountCashFlowCode = d.AccountCashFlowCode,
                                                           AccountCashFlow = d.AccountCashFlow,
                                                           AccountTypeCode = incomeAccount.FirstOrDefault().MstAccountType.AccountTypeCode,
                                                           AccountType = incomeAccount.FirstOrDefault().MstAccountType.AccountType,
                                                           AccountCode = "0000",
                                                           Account = incomeAccount.FirstOrDefault().Account,
                                                       } into g
                                                       select new
                                                       {
                                                           AccountCashFlowCode = g.Key.AccountCashFlowCode,
                                                           AccountCashFlow = g.Key.AccountCashFlow,
                                                           AccountTypeCode = g.Key.AccountTypeCode,
                                                           AccountType = g.Key.AccountType,
                                                           AccountCode = g.Key.AccountCode,
                                                           Account = g.Key.Account,
                                                           DebitAmount = g.Sum(d => d.DebitAmount),
                                                           CreditAmount = g.Sum(d => d.CreditAmount),
                                                           Balance = g.Sum(d => d.CreditAmount - d.DebitAmount)
                                                       };

                            var groupedAccountBalanceSheet = from d in groupedAccountTypesBalanceSheet
                                                             where d.AccountCode.Equals(unionAccountType.AccountCode)
                                                             group d by new
                                                             {
                                                                 AccountCashFlowCode = d.AccountCashFlowCode,
                                                                 AccountCashFlow = d.AccountCashFlow,
                                                                 AccountTypeCode = d.AccountTypeCode,
                                                                 AccountType = d.AccountType,
                                                                 AccountCode = d.AccountCode,
                                                                 Account = d.Account
                                                             } into g
                                                             select new
                                                             {
                                                                 AccountCashFlowCode = g.Key.AccountCashFlowCode,
                                                                 AccountCashFlow = g.Key.AccountCashFlow,
                                                                 AccountTypeCode = g.Key.AccountTypeCode,
                                                                 AccountType = g.Key.AccountType,
                                                                 AccountCode = g.Key.AccountCode,
                                                                 Account = g.Key.Account,
                                                                 DebitAmount = g.Sum(d => d.DebitAmount),
                                                                 CreditAmount = g.Sum(d => d.CreditAmount),
                                                                 Balance = g.Sum(d => d.CreditAmount - d.DebitAmount)
                                                             };

                            var unionAccountGroups = groupedAccountIncome.Union(groupedAccountBalanceSheet).OrderBy(d => d.AccountCode);
                            if (unionAccountGroups.Any())
                            {
                                foreach (var unionAccountGroup in unionAccountGroups)
                                {
                                    listCashFlow.Add(new Entities.RepCashFlow()
                                    {
                                        AccountCashFlowCode = unionAccountGroup.AccountCashFlowCode,
                                        AccountCashFlow = unionAccountGroup.AccountCashFlow,
                                        AccountTypeCode = unionAccountGroup.AccountTypeCode,
                                        AccountType = unionAccountGroup.AccountType,
                                        AccountCode = unionAccountGroup.AccountCode,
                                        Account = unionAccountGroup.Account,
                                        DebitAmount = unionAccountGroup.DebitAmount,
                                        CreditAmount = unionAccountGroup.CreditAmount,
                                        Balance = unionAccountGroup.Balance
                                    });
                                }
                            }
                        }
                    }
                }
            }

            return listCashFlow;
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/cashFlow/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListCashFlowListCompany()
        {
            var companies = from d in db.MstCompanies.OrderBy(d => d.Company)
                            select new Entities.MstCompany
                            {
                                Id = d.Id,
                                Company = d.Company
                            };

            return companies.ToList();
        }
    }
}
