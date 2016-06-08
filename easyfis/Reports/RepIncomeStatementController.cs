﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace easyfis.Reports
{
    public class RepIncomeStatementController : Controller
    {
        // Easyfis data context
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // PDF Collection Summary Report
        [Authorize]
        public ActionResult IncomeStatement(String StartDate, String EndDate, Int32 CompanyId)
        {
            // PDF settings
            MemoryStream workStream = new MemoryStream();
            Rectangle rectangle = new Rectangle(PageSize.A3);
            Document document = new Document(rectangle, 72, 72, 72, 72);
            document.SetMargins(30f, 30f, 30f, 30f);
            PdfWriter.GetInstance(document, workStream).CloseStream = false;

            // Document Starts
            document.Open();

            // Fonts Customization
            Font fontArial17Bold = FontFactory.GetFont("Arial", 17, Font.BOLD);
            Font fontArial11 = FontFactory.GetFont("Arial", 11);
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);

            // line
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));

            // Company Detail
            var companyName = (from d in db.MstCompanies where d.Id == CompanyId select d.Company).SingleOrDefault();
            var address = (from d in db.MstCompanies where d.Id == CompanyId select d.Address).SingleOrDefault();
            var contactNo = (from d in db.MstCompanies where d.Id == CompanyId select d.ContactNumber).SingleOrDefault();

            // table main header
            PdfPTable tableHeaderPage = new PdfPTable(2);
            float[] widthsCellsheaderPage = new float[] { 100f, 75f };
            tableHeaderPage.SetWidths(widthsCellsheaderPage);
            tableHeaderPage.WidthPercentage = 100;
            tableHeaderPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
            tableHeaderPage.AddCell(new PdfPCell(new Phrase("Income Statement", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
            tableHeaderPage.AddCell(new PdfPCell(new Phrase(address, fontArial11)) { Border = 0, PaddingTop = 5f });
            tableHeaderPage.AddCell(new PdfPCell(new Phrase("Date from " + StartDate + " to " + EndDate, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2, });
            tableHeaderPage.AddCell(new PdfPCell(new Phrase(contactNo, fontArial11)) { Border = 0, PaddingTop = 5f });
            tableHeaderPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            document.Add(tableHeaderPage);
            document.Add(line);

            var incomes = from d in db.TrnJournals
                         where d.JournalDate >= Convert.ToDateTime(StartDate)
                            && d.JournalDate <= Convert.ToDateTime(EndDate)
                            && d.MstAccount.MstAccountType.MstAccountCategory.Id == 5
                            && d.MstBranch.CompanyId == CompanyId
                         group d by d.MstAccount into g
                         select new Models.TrnJournal
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

            var expenses = from d in db.TrnJournals
                          where d.JournalDate >= Convert.ToDateTime(StartDate)
                             && d.JournalDate <= Convert.ToDateTime(EndDate)
                             && d.MstAccount.MstAccountType.MstAccountCategory.Id == 6
                             && d.MstBranch.CompanyId == CompanyId
                          group d by d.MstAccount into g
                          select new Models.TrnJournal
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
                              Balance = g.Sum(d => d.CreditAmount - d.DebitAmount)
                          };

            // Income
            PdfPTable tableIncome = new PdfPTable(1);
            float[] widthIncomeCells = new float[] { 100f };
            tableIncome.SetWidths(widthIncomeCells);
            tableIncome.WidthPercentage = 100;
            tableIncome.AddCell(new PdfPCell(new Phrase("Income", fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f, PaddingBottom = 10f });
            document.Add(tableIncome);

            PdfPTable tableHeaderIncome = new PdfPTable(5);
            float[] widthHeaderIncomeCells = new float[] { 50f, 70f, 100f, 100f, 60f };
            tableHeaderIncome.SetWidths(widthHeaderIncomeCells);
            tableHeaderIncome.WidthPercentage = 100;
            tableHeaderIncome.AddCell(new PdfPCell(new Phrase("Document Ref.", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
            tableHeaderIncome.AddCell(new PdfPCell(new Phrase("Sub Category", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
            tableHeaderIncome.AddCell(new PdfPCell(new Phrase("Account Type", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
            tableHeaderIncome.AddCell(new PdfPCell(new Phrase("Account", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
            tableHeaderIncome.AddCell(new PdfPCell(new Phrase("Balance", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });

            Decimal totalIncome = 0;
            foreach (var income in incomes)
            {
                tableHeaderIncome.AddCell(new PdfPCell(new Phrase(income.DocumentReference, fontArial10)) { HorizontalAlignment = 0, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableHeaderIncome.AddCell(new PdfPCell(new Phrase(income.SubCategoryDescription, fontArial10)) { HorizontalAlignment = 0, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableHeaderIncome.AddCell(new PdfPCell(new Phrase(income.AccountTypeCode + "-" + income.AccountType, fontArial10)) { HorizontalAlignment = 0, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableHeaderIncome.AddCell(new PdfPCell(new Phrase(income.AccountCode + "-" + income.Account, fontArial10)) { HorizontalAlignment = 0, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableHeaderIncome.AddCell(new PdfPCell(new Phrase(income.Balance.ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });

                totalIncome = totalIncome + income.Balance;
            }
            document.Add(tableHeaderIncome);

            document.Add(line);
            PdfPTable tableTotalIncomes = new PdfPTable(5);
            float[] widthTotalIncomesCells = new float[] { 50f, 70f, 100f, 100f, 60f };
            tableTotalIncomes.SetWidths(widthTotalIncomesCells);
            tableTotalIncomes.WidthPercentage = 100;
            tableTotalIncomes.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            tableTotalIncomes.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            tableTotalIncomes.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            tableTotalIncomes.AddCell(new PdfPCell(new Phrase("Total Incomes", fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            tableTotalIncomes.AddCell(new PdfPCell(new Phrase(totalIncome.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            document.Add(tableTotalIncomes);

            document.Add(Chunk.NEWLINE);

            // Expenses
            PdfPTable tableExpense = new PdfPTable(1);
            float[] widthExpenseCells = new float[] { 100f };
            tableExpense.SetWidths(widthExpenseCells);
            tableExpense.WidthPercentage = 100;
            tableExpense.AddCell(new PdfPCell(new Phrase("Expenses", fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f, PaddingBottom = 10f });
            document.Add(tableExpense);

            PdfPTable tableHeaderExpense = new PdfPTable(5);
            float[] widthHeaderExpenseCells = new float[] { 50f, 70f, 100f, 100f, 60f };
            tableHeaderExpense.SetWidths(widthHeaderExpenseCells);
            tableHeaderExpense.WidthPercentage = 100;
            tableHeaderExpense.AddCell(new PdfPCell(new Phrase("Document Ref.", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
            tableHeaderExpense.AddCell(new PdfPCell(new Phrase("Sub Category", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
            tableHeaderExpense.AddCell(new PdfPCell(new Phrase("Account Type", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
            tableHeaderExpense.AddCell(new PdfPCell(new Phrase("Account", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
            tableHeaderExpense.AddCell(new PdfPCell(new Phrase("Balance", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });

            Decimal totalExpense = 0;
            foreach (var expense in expenses)
            {
                tableHeaderExpense.AddCell(new PdfPCell(new Phrase(expense.DocumentReference, fontArial10)) { HorizontalAlignment = 0, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableHeaderExpense.AddCell(new PdfPCell(new Phrase(expense.SubCategoryDescription, fontArial10)) { HorizontalAlignment = 0, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableHeaderExpense.AddCell(new PdfPCell(new Phrase(expense.AccountTypeCode + "-" + expense.AccountType, fontArial10)) { HorizontalAlignment = 0, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableHeaderExpense.AddCell(new PdfPCell(new Phrase(expense.AccountCode + "-" + expense.Account, fontArial10)) { HorizontalAlignment = 0, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableHeaderExpense.AddCell(new PdfPCell(new Phrase(expense.Balance.ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });

                totalExpense = totalExpense + expense.Balance;
            }
            document.Add(tableHeaderExpense);

            document.Add(line);
            PdfPTable tableTotalExpenses = new PdfPTable(5);
            float[] widthTotalExpensesCells = new float[] { 50f, 70f, 100f, 100f, 60f };
            tableTotalExpenses.SetWidths(widthTotalExpensesCells);
            tableTotalExpenses.WidthPercentage = 100;
            tableTotalExpenses.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            tableTotalExpenses.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            tableTotalExpenses.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            tableTotalExpenses.AddCell(new PdfPCell(new Phrase("Total Expenses", fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            tableTotalExpenses.AddCell(new PdfPCell(new Phrase(totalExpense.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            document.Add(tableTotalExpenses);

            document.Add(Chunk.NEWLINE);
            document.Add(line);
            Decimal totalNetIncome = totalIncome + totalExpense;
            PdfPTable tableTotalNetIncome = new PdfPTable(5);
            float[] widthTotalNetIncomeCells = new float[] { 50f, 70f, 100f, 100f, 60f };
            tableTotalNetIncome.SetWidths(widthTotalNetIncomeCells);
            tableTotalNetIncome.WidthPercentage = 100;
            tableTotalNetIncome.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            tableTotalNetIncome.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            tableTotalNetIncome.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            tableTotalNetIncome.AddCell(new PdfPCell(new Phrase("Net Income", fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            tableTotalNetIncome.AddCell(new PdfPCell(new Phrase(totalNetIncome.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f });
            document.Add(tableTotalNetIncome);

            //// retrieve account sub category journal for income
            //var accountTypeSubCategory_Journal_Incomes = from d in db.TrnJournals
            //                                             where d.JournalDate >= Convert.ToDateTime(StartDate)
            //                                             && d.JournalDate <= Convert.ToDateTime(EndDate)
            //                                             && d.MstAccount.MstAccountType.MstAccountCategory.Id == 5
            //                                             && d.MstBranch.CompanyId == CompanyId
            //                                             group d by new
            //                                             {
            //                                                 AccountCategory = d.MstAccount.MstAccountType.MstAccountCategory.AccountCategory,
            //                                                 SubCategoryDescription = d.MstAccount.MstAccountType.SubCategoryDescription
            //                                             } into g
            //                                             select new Models.TrnJournal
            //                                             {
            //                                                 AccountCategory = g.Key.AccountCategory,
            //                                                 SubCategoryDescription = g.Key.SubCategoryDescription,
            //                                                 DebitAmount = g.Sum(d => d.DebitAmount),
            //                                                 CreditAmount = g.Sum(d => d.CreditAmount)
            //                                             };

            //// retrieve account type journal for income
            //var accountType_Journal_Incomes = from d in db.TrnJournals
            //                                  where d.JournalDate >= Convert.ToDateTime(StartDate)
            //                                  && d.JournalDate <= Convert.ToDateTime(EndDate)
            //                                  && d.MstAccount.MstAccountType.MstAccountCategory.Id == 5
            //                                  && d.MstBranch.CompanyId == CompanyId
            //                                  group d by new
            //                                  {
            //                                      AccountType = d.MstAccount.MstAccountType.AccountType
            //                                  } into g
            //                                  select new Models.TrnJournal
            //                                  {
            //                                      AccountType = g.Key.AccountType,
            //                                      DebitAmount = g.Sum(d => d.DebitAmount),
            //                                      CreditAmount = g.Sum(d => d.CreditAmount)
            //                                  };

            //Decimal totalRevenue = 0;
            ////Decimal totalCurrentLiabilities = 0;
            ////Decimal totalStockHoldersEquity = 0;

            //if (accountTypeSubCategory_Journal_Incomes.Any())
            //{
            //    if (accountType_Journal_Incomes.Any())
            //    {
            //        foreach (var accountType_Journal_Income in accountTypeSubCategory_Journal_Incomes)
            //        {
            //            // table income statement
            //            PdfPTable tableIncomeStatementHeader = new PdfPTable(3);
            //            float[] widthCellsTableIncomeStatementHeader = new float[] { 50f, 100f, 50f };
            //            tableIncomeStatementHeader.SetWidths(widthCellsTableIncomeStatementHeader);
            //            tableIncomeStatementHeader.WidthPercentage = 100;

            //            document.Add(line);

            //            PdfPCell headerSubCategoryColspan = (new PdfPCell(new Phrase(accountType_Journal_Income.SubCategoryDescription, fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, BackgroundColor = BaseColor.LIGHT_GRAY });
            //            headerSubCategoryColspan.Colspan = 3;
            //            tableIncomeStatementHeader.AddCell(headerSubCategoryColspan);

            //            document.Add(tableIncomeStatementHeader);
            //            //totalCurrentAsset = accountTypeSubCategory_Journal_Asset.DebitAmount;
            //        }

            //        foreach (var accountType_Journal_Income in accountType_Journal_Incomes)
            //        {
            //            Decimal balanceIncomeAmount = accountType_Journal_Income.CreditAmount - accountType_Journal_Income.DebitAmount;

            //            // table Balance Sheet
            //            PdfPTable tableIncomeStatementAccounts = new PdfPTable(3);
            //            float[] widthCellsTableIncomeStatementAccounts = new float[] { 50f, 100f, 50f };
            //            tableIncomeStatementAccounts.SetWidths(widthCellsTableIncomeStatementAccounts);
            //            tableIncomeStatementAccounts.WidthPercentage = 100;
            //            tableIncomeStatementAccounts.AddCell(new PdfPCell(new Phrase(accountType_Journal_Income.AccountType, fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f, PaddingBottom = 5f, PaddingLeft = 25f });
            //            tableIncomeStatementAccounts.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f, PaddingBottom = 5f });
            //            tableIncomeStatementAccounts.AddCell(new PdfPCell(new Phrase(balanceIncomeAmount.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 10f, PaddingBottom = 5f });

            //            totalRevenue = totalRevenue + balanceIncomeAmount;

            //            // table income statement
            //            var accounts_JournalIncomes = from d in db.TrnJournals
            //                                          where d.JournalDate >= Convert.ToDateTime(StartDate)
            //                                          && d.JournalDate <= Convert.ToDateTime(EndDate)
            //                                          && d.MstAccount.MstAccountType.AccountType == accountType_Journal_Income.AccountType
            //                                          && d.MstAccount.MstAccountType.MstAccountCategory.Id == 5
            //                                          && d.MstBranch.CompanyId == CompanyId
            //                                          group d by new
            //                                          {
            //                                              AccountCode = d.MstAccount.AccountCode,
            //                                              Account = d.MstAccount.Account
            //                                          } into g
            //                                          select new Models.TrnJournal
            //                                          {
            //                                              AccountCode = g.Key.AccountCode,
            //                                              Account = g.Key.Account,
            //                                              DebitAmount = g.Sum(d => d.DebitAmount),
            //                                              CreditAmount = g.Sum(d => d.CreditAmount)
            //                                          };

            //            foreach (var accounts_JournalIncome in accounts_JournalIncomes)
            //            {
            //                Decimal balanceIncomeAmountForAccounts = accounts_JournalIncome.DebitAmount - accounts_JournalIncome.CreditAmount;

            //                tableIncomeStatementAccounts.AddCell(new PdfPCell(new Phrase(accounts_JournalIncome.AccountCode, fontArial10)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
            //                tableIncomeStatementAccounts.AddCell(new PdfPCell(new Phrase(accounts_JournalIncome.Account, fontArial10)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 20f });
            //                tableIncomeStatementAccounts.AddCell(new PdfPCell(new Phrase(balanceIncomeAmountForAccounts.ToString("#,##0.00"), fontArial10)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
            //            }

            //            document.Add(tableIncomeStatementAccounts);
            //        }

            //        document.Add(line);
            //        foreach (var accountType_Journal_Income_Footer in accountTypeSubCategory_Journal_Incomes)
            //        {
            //            // table Balance Sheet footer in income CAtegory
            //            PdfPTable tableIncomeStatementooterIncome = new PdfPTable(4);
            //            float[] widthCellsTableIncomeStatementFooterIncome = new float[] { 20f, 20f, 150f, 30f };
            //            tableIncomeStatementooterIncome.SetWidths(widthCellsTableIncomeStatementFooterIncome);
            //            tableIncomeStatementooterIncome.WidthPercentage = 100;
            //            tableIncomeStatementooterIncome.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 25f });
            //            tableIncomeStatementooterIncome.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f });
            //            tableIncomeStatementooterIncome.AddCell(new PdfPCell(new Phrase("Total " + accountType_Journal_Income_Footer.SubCategoryDescription, fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
            //            tableIncomeStatementooterIncome.AddCell(new PdfPCell(new Phrase(totalRevenue.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
            //            document.Add(tableIncomeStatementooterIncome);
            //            document.Add(Chunk.NEWLINE);
            //        }
            //    }
            //}

            //// retrieve account sub category journal for Expenses
            //var accountTypeSubCategory_Journal_Expenses = from d in db.TrnJournals
            //                                              where d.JournalDate >= Convert.ToDateTime(StartDate)
            //                                              && d.JournalDate <= Convert.ToDateTime(EndDate)
            //                                              && d.MstAccount.MstAccountType.MstAccountCategory.Id == 6
            //                                              && d.MstBranch.CompanyId == CompanyId
            //                                              group d by new
            //                                              {
            //                                                  AccountCategory = d.MstAccount.MstAccountType.MstAccountCategory.AccountCategory,
            //                                                  SubCategoryDescription = d.MstAccount.MstAccountType.SubCategoryDescription
            //                                              } into g
            //                                              select new Models.TrnJournal
            //                                              {
            //                                                  AccountCategory = g.Key.AccountCategory,
            //                                                  SubCategoryDescription = g.Key.SubCategoryDescription,
            //                                                  DebitAmount = g.Sum(d => d.DebitAmount),
            //                                                  CreditAmount = g.Sum(d => d.CreditAmount)
            //                                              };

            //// retrieve account type journal for Expenses
            //var accountType_Journal_Expenses = from d in db.TrnJournals
            //                                   where d.JournalDate >= Convert.ToDateTime(StartDate)
            //                                   && d.JournalDate <= Convert.ToDateTime(EndDate)
            //                                   && d.MstAccount.MstAccountType.MstAccountCategory.Id == 6
            //                                   && d.MstBranch.CompanyId == CompanyId
            //                                   group d by new
            //                                   {
            //                                       AccountType = d.MstAccount.MstAccountType.AccountType
            //                                   } into g
            //                                   select new Models.TrnJournal
            //                                   {
            //                                       AccountType = g.Key.AccountType,
            //                                       DebitAmount = g.Sum(d => d.DebitAmount),
            //                                       CreditAmount = g.Sum(d => d.CreditAmount)
            //                                   };

            //Decimal totalCostOfSales = 0;
            ////Decimal totalCurrentLiabilities = 0;
            ////Decimal totalStockHoldersEquity = 0;

            //if (accountTypeSubCategory_Journal_Expenses.Any())
            //{
            //    if (accountType_Journal_Expenses.Any())
            //    {
            //        foreach (var accountTypeSubCategory_Journal_Expense in accountTypeSubCategory_Journal_Expenses)
            //        {
            //            // table income statement
            //            PdfPTable tableIncomeStatementHeader = new PdfPTable(3);
            //            float[] widthCellsTableIncomeStatementHeader = new float[] { 50f, 100f, 50f };
            //            tableIncomeStatementHeader.SetWidths(widthCellsTableIncomeStatementHeader);
            //            tableIncomeStatementHeader.WidthPercentage = 100;

            //            document.Add(line);

            //            PdfPCell headerSubCategoryColspan = (new PdfPCell(new Phrase(accountTypeSubCategory_Journal_Expense.SubCategoryDescription, fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, BackgroundColor = BaseColor.LIGHT_GRAY });
            //            headerSubCategoryColspan.Colspan = 3;
            //            tableIncomeStatementHeader.AddCell(headerSubCategoryColspan);
            //            document.Add(tableIncomeStatementHeader);
            //            //totalCurrentAsset = accountTypeSubCategory_Journal_Asset.DebitAmount;
            //        }

            //        foreach (var accountType_Journal_Expense in accountType_Journal_Expenses)
            //        {
            //            Decimal balanceIncomeAmount = accountType_Journal_Expense.CreditAmount - accountType_Journal_Expense.DebitAmount;

            //            // table income statement
            //            PdfPTable tableIncomeStatementAccounts = new PdfPTable(3);
            //            float[] widthCellsTableIncomeStatementAccounts = new float[] { 50f, 100f, 50f };
            //            tableIncomeStatementAccounts.SetWidths(widthCellsTableIncomeStatementAccounts);
            //            tableIncomeStatementAccounts.WidthPercentage = 100;
            //            tableIncomeStatementAccounts.AddCell(new PdfPCell(new Phrase(accountType_Journal_Expense.AccountType, fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f, PaddingBottom = 5f, PaddingLeft = 25f });
            //            tableIncomeStatementAccounts.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f, PaddingBottom = 5f });
            //            tableIncomeStatementAccounts.AddCell(new PdfPCell(new Phrase(balanceIncomeAmount.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 10f, PaddingBottom = 5f });

            //            totalCostOfSales = totalCostOfSales + balanceIncomeAmount;

            //            // retrieve accounts journal income
            //            var accounts_JournalExpenses = from d in db.TrnJournals
            //                                           where d.JournalDate >= Convert.ToDateTime(StartDate)
            //                                           && d.JournalDate <= Convert.ToDateTime(EndDate)
            //                                           && d.MstAccount.MstAccountType.AccountType == accountType_Journal_Expense.AccountType
            //                                           && d.MstAccount.MstAccountType.MstAccountCategory.Id == 6
            //                                           && d.MstBranch.CompanyId == CompanyId
            //                                           group d by new
            //                                           {
            //                                               AccountCode = d.MstAccount.AccountCode,
            //                                               Account = d.MstAccount.Account
            //                                           } into g
            //                                           select new Models.TrnJournal
            //                                           {
            //                                               AccountCode = g.Key.AccountCode,
            //                                               Account = g.Key.Account,
            //                                               DebitAmount = g.Sum(d => d.DebitAmount),
            //                                               CreditAmount = g.Sum(d => d.CreditAmount)
            //                                           };

            //            foreach (var accounts_JournalExpense in accounts_JournalExpenses)
            //            {
            //                Decimal balanceExpenseAmountForAccounts = accounts_JournalExpense.DebitAmount - accounts_JournalExpense.CreditAmount;

            //                tableIncomeStatementAccounts.AddCell(new PdfPCell(new Phrase(accounts_JournalExpense.AccountCode, fontArial10)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
            //                tableIncomeStatementAccounts.AddCell(new PdfPCell(new Phrase(accounts_JournalExpense.Account, fontArial10)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 20f });
            //                tableIncomeStatementAccounts.AddCell(new PdfPCell(new Phrase(balanceExpenseAmountForAccounts.ToString("#,##0.00"), fontArial10)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
            //            }

            //            document.Add(tableIncomeStatementAccounts);
            //        }

            //        document.Add(line);
            //        foreach (var accountTypeSubCategory_Journal_Expense_Footer in accountTypeSubCategory_Journal_Expenses)
            //        {
            //            // table Balance Sheet footer in income CAtegory
            //            PdfPTable tableIncomeStatementooterExpense = new PdfPTable(4);
            //            float[] widthCellsTableIncomeStatementFooterExpense = new float[] { 20f, 20f, 150f, 30f };
            //            tableIncomeStatementooterExpense.SetWidths(widthCellsTableIncomeStatementFooterExpense);
            //            tableIncomeStatementooterExpense.WidthPercentage = 100;
            //            tableIncomeStatementooterExpense.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 25f });
            //            tableIncomeStatementooterExpense.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f });
            //            tableIncomeStatementooterExpense.AddCell(new PdfPCell(new Phrase("Total " + accountTypeSubCategory_Journal_Expense_Footer.SubCategoryDescription, fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
            //            tableIncomeStatementooterExpense.AddCell(new PdfPCell(new Phrase(totalCostOfSales.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
            //            document.Add(tableIncomeStatementooterExpense);
            //            document.Add(Chunk.NEWLINE);
            //        }
            //    }
            //}

            //document.Add(line);

            //Decimal totalNetIncome = totalRevenue + totalCostOfSales;

            //// table Balance Sheet
            //PdfPTable tableBalanceSheetFooterTotalLiabilityAndEquity = new PdfPTable(4);
            //float[] widthCellsTableBalanceSheetFooterTotalLiabilityAndEquity = new float[] { 20f, 20f, 150f, 30f };
            //tableBalanceSheetFooterTotalLiabilityAndEquity.SetWidths(widthCellsTableBalanceSheetFooterTotalLiabilityAndEquity);
            //tableBalanceSheetFooterTotalLiabilityAndEquity.WidthPercentage = 100;
            //tableBalanceSheetFooterTotalLiabilityAndEquity.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 25f });
            //tableBalanceSheetFooterTotalLiabilityAndEquity.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f });
            //tableBalanceSheetFooterTotalLiabilityAndEquity.AddCell(new PdfPCell(new Phrase("Net Income", fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
            //tableBalanceSheetFooterTotalLiabilityAndEquity.AddCell(new PdfPCell(new Phrase(totalNetIncome.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
            //document.Add(tableBalanceSheetFooterTotalLiabilityAndEquity);

            // Document End
            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}