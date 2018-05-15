using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;

namespace easyfis.ModifiedApiControllers
{
    public class ApiRepBalanceSheetController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==================
        // Balance Sheet List 
        // ==================
        [Authorize, HttpGet, Route("api/balanceSheet/list/{dateAsOf}/{companyId}")]
        public List<Entities.RepBalanceSheet> ListBalanceSheet(String dateAsOf, String companyId)
        {
            List<Entities.RepBalanceSheet> listBalanceSheet = new List<Entities.RepBalanceSheet>();

            // ==========
            // Get Assets
            // ==========
            var assets = from d in db.TrnJournals
                         where d.JournalDate <= Convert.ToDateTime(dateAsOf) &&
                         d.MstAccount.MstAccountType.MstAccountCategory.Id == 1 &&
                         d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                         group d by d.MstAccount into g
                         select new Entities.RepBalanceSheet
                         {
                             DocumentReference = "1 - Asset",
                             AccountCategoryCode = g.Key.MstAccountType.MstAccountCategory.AccountCategoryCode,
                             AccountCategory = g.Key.MstAccountType.MstAccountCategory.AccountCategory,
                             SubCategoryDescription = g.Key.MstAccountType.SubCategoryDescription,
                             AccountTypeCode = g.Key.MstAccountType.AccountTypeCode,
                             AccountType = g.Key.MstAccountType.AccountType,
                             AccountCode = g.Key.AccountCode,
                             Account = g.Key.Account,
                             DebitAmount = g.Sum(d => d.DebitAmount),
                             CreditAmount = g.Sum(d => d.CreditAmount),
                             Balance = g.Sum(d => d.DebitAmount - d.CreditAmount)
                         };

            if (assets.Any())
            {
                foreach (var asset in assets)
                {
                    listBalanceSheet.Add(new Entities.RepBalanceSheet()
                    {
                        DocumentReference = asset.DocumentReference,
                        AccountCategoryCode = asset.AccountCategoryCode,
                        AccountCategory = asset.AccountCategory,
                        SubCategoryDescription = asset.SubCategoryDescription,
                        AccountTypeCode = asset.AccountTypeCode,
                        AccountType = asset.AccountType,
                        AccountCode = asset.AccountCode,
                        Account = asset.Account,
                        DebitAmount = asset.DebitAmount,
                        CreditAmount = asset.CreditAmount,
                        Balance = asset.Balance
                    });
                }
            }

            // ===============
            // Get Liabilities
            // ===============
            var liabilities = from d in db.TrnJournals
                              where d.JournalDate <= Convert.ToDateTime(dateAsOf) &&
                              d.MstAccount.MstAccountType.MstAccountCategory.Id == 2 &&
                              d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                              group d by d.MstAccount into g
                              select new Entities.RepBalanceSheet
                              {
                                  DocumentReference = "2 - Liability",
                                  AccountCategoryCode = g.Key.MstAccountType.MstAccountCategory.AccountCategoryCode,
                                  AccountCategory = g.Key.MstAccountType.MstAccountCategory.AccountCategory,
                                  SubCategoryDescription = g.Key.MstAccountType.SubCategoryDescription,
                                  AccountTypeCode = g.Key.MstAccountType.AccountTypeCode,
                                  AccountType = g.Key.MstAccountType.AccountType,
                                  AccountCode = g.Key.AccountCode,
                                  Account = g.Key.Account,
                                  DebitAmount = g.Sum(d => d.DebitAmount),
                                  CreditAmount = g.Sum(d => d.CreditAmount),
                                  Balance = g.Sum(d => d.CreditAmount - d.DebitAmount)
                              };

            if (liabilities.Any())
            {
                foreach (var liability in liabilities)
                {
                    listBalanceSheet.Add(new Entities.RepBalanceSheet()
                    {
                        DocumentReference = liability.DocumentReference,
                        AccountCategoryCode = liability.AccountCategoryCode,
                        AccountCategory = liability.AccountCategory,
                        SubCategoryDescription = liability.SubCategoryDescription,
                        AccountTypeCode = liability.AccountTypeCode,
                        AccountType = liability.AccountType,
                        AccountCode = liability.AccountCode,
                        Account = liability.Account,
                        DebitAmount = liability.DebitAmount,
                        CreditAmount = liability.CreditAmount,
                        Balance = liability.Balance
                    });
                }
            }

            // ===================
            // Get Profit and Loss
            // ===================
            var profitAndLoss = from d in db.TrnJournals
                                where d.JournalDate <= Convert.ToDateTime(dateAsOf)
                                && (d.MstAccount.MstAccountType.MstAccountCategory.Id == 5 || d.MstAccount.MstAccountType.MstAccountCategory.Id == 6)
                                && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                group d by d.MstAccount into g
                                select new Entities.RepBalanceSheet
                                {
                                    DocumentReference = "ProfitAndLoss",
                                    AccountCategoryCode = g.Key.MstAccountType.MstAccountCategory.AccountCategoryCode,
                                    AccountCategory = g.Key.MstAccountType.MstAccountCategory.AccountCategory,
                                    SubCategoryDescription = g.Key.MstAccountType.SubCategoryDescription,
                                    AccountTypeCode = g.Key.MstAccountType.AccountTypeCode,
                                    AccountType = g.Key.MstAccountType.AccountType,
                                    AccountCode = g.Key.AccountCode,
                                    Account = g.Key.Account,
                                    DebitAmount = g.Sum(d => d.DebitAmount),
                                    CreditAmount = g.Sum(d => d.CreditAmount),
                                    Balance = g.Sum(d => d.CreditAmount - d.DebitAmount)
                                };

            // ============
            // Get Equities
            // ============
            var equities = from d in db.TrnJournals
                           where d.JournalDate <= Convert.ToDateTime(dateAsOf)
                           && d.MstAccount.MstAccountType.MstAccountCategory.Id == 4
                           && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                           group d by d.MstAccount into g
                           select new Entities.RepBalanceSheet
                           {
                               DocumentReference = "3 - Equity",
                               AccountCategoryCode = g.Key.MstAccountType.MstAccountCategory.AccountCategoryCode,
                               AccountCategory = g.Key.MstAccountType.MstAccountCategory.AccountCategory,
                               SubCategoryDescription = g.Key.MstAccountType.SubCategoryDescription,
                               AccountTypeCode = g.Key.MstAccountType.AccountTypeCode,
                               AccountType = g.Key.MstAccountType.AccountType,
                               AccountCode = g.Key.AccountCode,
                               Account = g.Key.Account,
                               DebitAmount = g.Sum(d => d.DebitAmount),
                               CreditAmount = g.Sum(d => d.CreditAmount),
                               Balance = g.Sum(d => d.CreditAmount - d.DebitAmount)
                           };

            var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
            var incomeAccount = from d in db.MstAccounts where d.Id == currentUser.FirstOrDefault().IncomeAccountId select d;

            var retainedEarnings = from d in profitAndLoss
                                   group d by d.DocumentReference into g
                                   select new Entities.RepBalanceSheet
                                   {
                                       DocumentReference = "3 - Equity",
                                       AccountCategoryCode = incomeAccount.FirstOrDefault().MstAccountType.MstAccountCategory.AccountCategoryCode,
                                       AccountCategory = incomeAccount.FirstOrDefault().MstAccountType.MstAccountCategory.AccountCategory,
                                       SubCategoryDescription = incomeAccount.FirstOrDefault().MstAccountType.SubCategoryDescription,
                                       AccountTypeCode = incomeAccount.FirstOrDefault().MstAccountType.AccountTypeCode,
                                       AccountType = incomeAccount.FirstOrDefault().MstAccountType.AccountType,
                                       AccountCode = incomeAccount.FirstOrDefault().AccountCode,
                                       Account = incomeAccount.FirstOrDefault().Account,
                                       DebitAmount = g.Sum(d => d.DebitAmount),
                                       CreditAmount = g.Sum(d => d.CreditAmount),
                                       Balance = g.Sum(d => d.Balance),
                                   };

            var unionEquitiesWithRetainEarnings = equities.Union(retainedEarnings);

            if (unionEquitiesWithRetainEarnings.Any())
            {
                foreach (var unionEquitiesWithRetainEarning in unionEquitiesWithRetainEarnings)
                {
                    listBalanceSheet.Add(new Entities.RepBalanceSheet()
                    {
                        DocumentReference = unionEquitiesWithRetainEarning.DocumentReference,
                        AccountCategoryCode = unionEquitiesWithRetainEarning.AccountCategoryCode,
                        AccountCategory = unionEquitiesWithRetainEarning.AccountCategory,
                        SubCategoryDescription = unionEquitiesWithRetainEarning.SubCategoryDescription,
                        AccountTypeCode = unionEquitiesWithRetainEarning.AccountTypeCode,
                        AccountType = unionEquitiesWithRetainEarning.AccountType,
                        AccountCode = unionEquitiesWithRetainEarning.AccountCode,
                        Account = unionEquitiesWithRetainEarning.Account,
                        DebitAmount = unionEquitiesWithRetainEarning.DebitAmount,
                        CreditAmount = unionEquitiesWithRetainEarning.CreditAmount,
                        Balance = unionEquitiesWithRetainEarning.Balance
                    });
                }
            }

            var balanceSheets = from d in listBalanceSheet
                                group d by new
                                {
                                    DocumentReference = d.DocumentReference,
                                    AccountCategoryCode = d.AccountCategoryCode,
                                    AccountCategory = d.AccountCategory,
                                    SubCategoryDescription = d.SubCategoryDescription,
                                    AccountTypeCode = d.AccountTypeCode,
                                    AccountType = d.AccountType,
                                    AccountCode = d.AccountCode,
                                    Account = d.Account
                                } into g
                                select new Entities.RepBalanceSheet
                                {
                                    DocumentReference = g.Key.DocumentReference,
                                    AccountCategoryCode = g.Key.AccountCategoryCode,
                                    AccountCategory = g.Key.AccountCategory,
                                    SubCategoryDescription = g.Key.SubCategoryDescription,
                                    AccountTypeCode = g.Key.AccountTypeCode,
                                    AccountType = g.Key.AccountType,
                                    AccountCode = g.Key.AccountCode,
                                    Account = g.Key.Account,
                                    DebitAmount = g.Sum(s => s.DebitAmount),
                                    CreditAmount = g.Sum(s => s.CreditAmount),
                                    Balance = g.Sum(s => s.Balance)
                                };

            if (balanceSheets.Any())
            {
                return balanceSheets.ToList();
            }
            else
            {
                return null;
            }
        }

        // ===============================
        // Dropdown List - Company (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/balanceSheet/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListCancelledSalesSummaryReportListCompany()
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
