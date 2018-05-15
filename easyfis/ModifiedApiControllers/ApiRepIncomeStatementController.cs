using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ModifiedApiControllers
{
    public class ApiRepIncomeStatementController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =====================
        // Income Statement List
        // =====================
        [Authorize, HttpGet, Route("api/incomeStatement/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepIncomeStatement> ListIncomeStatement(String startDate, String endDate, String companyId, String branchId)
        {
            List<Entities.RepIncomeStatement> listIncomeStatement = new List<Entities.RepIncomeStatement>();

            // ==========
            // Get Income
            // ==========
            var incomes = from d in db.TrnJournals
                          where d.JournalDate >= Convert.ToDateTime(startDate)
                          && d.JournalDate <= Convert.ToDateTime(endDate)
                          && d.MstAccount.MstAccountType.MstAccountCategory.Id == 5
                          && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                          && d.BranchId == Convert.ToInt32(branchId)
                          group d by d.MstAccount into g
                          select new Entities.RepIncomeStatement
                          {
                              DocumentReference = "5 - incomes",
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

            if (incomes.Any())
            {
                foreach (var income in incomes)
                {
                    listIncomeStatement.Add(new Entities.RepIncomeStatement()
                    {
                        DocumentReference = income.DocumentReference,
                        AccountCategoryCode = income.AccountCategoryCode,
                        AccountCategory = income.AccountCategory,
                        SubCategoryDescription = income.SubCategoryDescription,
                        AccountTypeCode = income.AccountTypeCode,
                        AccountType = income.AccountType,
                        AccountCode = income.AccountCode,
                        Account = income.Account,
                        DebitAmount = income.DebitAmount,
                        CreditAmount = income.CreditAmount,
                        Balance = income.Balance
                    });
                }
            }

            // ============
            // Get Expenses
            // ============
            var expenses = from d in db.TrnJournals
                           where d.JournalDate >= Convert.ToDateTime(startDate)
                           && d.JournalDate <= Convert.ToDateTime(endDate)
                           && d.MstAccount.MstAccountType.MstAccountCategory.Id == 6
                           && d.MstBranch.CompanyId == Convert.ToInt32(companyId)
                           && d.BranchId == Convert.ToInt32(branchId)
                           group d by d.MstAccount into g
                           select new Entities.RepIncomeStatement
                           {
                               DocumentReference = "6 - Expenses",
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

            if (expenses.Any())
            {
                foreach (var expense in expenses)
                {
                    listIncomeStatement.Add(new Entities.RepIncomeStatement()
                    {
                        DocumentReference = expense.DocumentReference,
                        AccountCategoryCode = expense.AccountCategoryCode,
                        AccountCategory = expense.AccountCategory,
                        SubCategoryDescription = expense.SubCategoryDescription,
                        AccountTypeCode = expense.AccountTypeCode,
                        AccountType = expense.AccountType,
                        AccountCode = expense.AccountCode,
                        Account = expense.Account,
                        DebitAmount = expense.DebitAmount,
                        CreditAmount = expense.CreditAmount,
                        Balance = expense.Balance
                    });
                }
            }

            var incomeStatement = from d in listIncomeStatement
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
                                  select new Entities.RepIncomeStatement
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

            if (incomeStatement.Any())
            {
                return incomeStatement.ToList();
            }
            else
            {
                return null;
            }
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/incomeStatement/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListIncomeStatementListCompany()
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
        [Authorize, HttpGet, Route("api/incomeStatement/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListIncomeStatementBranch(String companyId)
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
