﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace easyfis.Reports
{
    public class RepBalanceSheetController : Controller
    {
        // Easyfis data context
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // PDF Collection Summary Report
        [Authorize]
        public ActionResult BalanceSheet(String DateAsOf, Int32 CompanyId)
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
            tableHeaderPage.AddCell(new PdfPCell(new Phrase("Balance Sheet", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
            tableHeaderPage.AddCell(new PdfPCell(new Phrase(address, fontArial11)) { Border = 0, PaddingTop = 5f });
            tableHeaderPage.AddCell(new PdfPCell(new Phrase("Date as of " + DateAsOf, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2, });
            tableHeaderPage.AddCell(new PdfPCell(new Phrase(contactNo, fontArial11)) { Border = 0, PaddingTop = 5f });
            tableHeaderPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            document.Add(tableHeaderPage);
            document.Add(line);

            // retrieve account sub category journal for Asset
            var accountTypeSubCategory_Journal_Assets = from d in db.TrnJournals
                                                        where d.JournalDate <= Convert.ToDateTime(DateAsOf)
                                                        && d.MstAccount.MstAccountType.MstAccountCategory.Id == 1
                                                        && d.MstBranch.CompanyId == CompanyId
                                                        group d by new
                                                        {
                                                            AccountCategory = d.MstAccount.MstAccountType.MstAccountCategory.AccountCategory,
                                                            SubCategoryDescription = d.MstAccount.MstAccountType.SubCategoryDescription
                                                        } into g
                                                        select new Models.TrnJournal
                                                        {
                                                            AccountCategory = g.Key.AccountCategory,
                                                            SubCategoryDescription = g.Key.SubCategoryDescription,
                                                            DebitAmount = g.Sum(d => d.DebitAmount),
                                                            CreditAmount = g.Sum(d => d.CreditAmount)
                                                        };

            // retrieve account type journal for Asset
            var accountType_Journal_Assets = from d in db.TrnJournals
                                             where d.JournalDate <= Convert.ToDateTime(DateAsOf)
                                             && d.MstAccount.MstAccountType.MstAccountCategory.Id == 1
                                             && d.MstBranch.CompanyId == CompanyId
                                             group d by new
                                             {
                                                 AccountType = d.MstAccount.MstAccountType.AccountType
                                             } into g
                                             select new Models.TrnJournal
                                             {
                                                 AccountType = g.Key.AccountType,
                                                 DebitAmount = g.Sum(d => d.DebitAmount),
                                                 CreditAmount = g.Sum(d => d.CreditAmount)
                                             };

            Decimal totalCurrentAsset = 0;
            Decimal totalCurrentLiabilities = 0;
            Decimal totalStockHoldersEquity = 0;

            if (accountTypeSubCategory_Journal_Assets.Any())
            {
                if (accountType_Journal_Assets.Any())
                {
                    foreach (var accountTypeSubCategory_Journal_Asset in accountTypeSubCategory_Journal_Assets)
                    {
                        // table Balance Sheet header
                        PdfPTable tableBalanceSheetHeader = new PdfPTable(3);
                        float[] widthCellsTableBalanceSheetHeader = new float[] { 50f, 100f, 50f };
                        tableBalanceSheetHeader.SetWidths(widthCellsTableBalanceSheetHeader);
                        tableBalanceSheetHeader.WidthPercentage = 100;

                        document.Add(line);

                        PdfPCell headerSubCategoryColspan = (new PdfPCell(new Phrase(accountTypeSubCategory_Journal_Asset.SubCategoryDescription, fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, BackgroundColor = BaseColor.LIGHT_GRAY });
                        headerSubCategoryColspan.Colspan = 3;
                        tableBalanceSheetHeader.AddCell(headerSubCategoryColspan);

                        document.Add(tableBalanceSheetHeader);
                        //totalCurrentAsset = accountTypeSubCategory_Journal_Asset.DebitAmount;
                    }

                    foreach (var accountType_JournalAsset in accountType_Journal_Assets)
                    {
                        Decimal balanceAssetAmount = accountType_JournalAsset.DebitAmount - accountType_JournalAsset.CreditAmount;

                        // table Balance Sheet
                        PdfPTable tableBalanceSheetAccounts = new PdfPTable(3);
                        float[] widthCellsTableBalanceSheetAccounts = new float[] { 50f, 100f, 50f };
                        tableBalanceSheetAccounts.SetWidths(widthCellsTableBalanceSheetAccounts);
                        tableBalanceSheetAccounts.WidthPercentage = 100;
                        tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(accountType_JournalAsset.AccountType, fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f, PaddingBottom = 5f, PaddingLeft = 25f });
                        tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f, PaddingBottom = 5f });
                        tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(balanceAssetAmount.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 10f, PaddingBottom = 5f });

                        totalCurrentAsset = totalCurrentAsset + balanceAssetAmount;

                        // retrieve accounts journal Asset
                        var accounts_JournalAssets = from d in db.TrnJournals
                                                     where d.JournalDate <= Convert.ToDateTime(DateAsOf)
                                                     && d.MstAccount.MstAccountType.AccountType == accountType_JournalAsset.AccountType
                                                     && d.MstAccount.MstAccountType.MstAccountCategory.Id == 1
                                                     && d.MstBranch.CompanyId == CompanyId
                                                     group d by new
                                                     {
                                                         AccountCode = d.MstAccount.AccountCode,
                                                         Account = d.MstAccount.Account
                                                     } into g
                                                     select new Models.TrnJournal
                                                     {
                                                         AccountCode = g.Key.AccountCode,
                                                         Account = g.Key.Account,
                                                         DebitAmount = g.Sum(d => d.DebitAmount),
                                                         CreditAmount = g.Sum(d => d.CreditAmount)
                                                     };

                        foreach (var accounts_JournalAsset in accounts_JournalAssets)
                        {
                            Decimal balanceAssetAmountForAccounts = accounts_JournalAsset.DebitAmount - accounts_JournalAsset.CreditAmount;

                            tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(accounts_JournalAsset.AccountCode, fontArial10)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
                            tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(accounts_JournalAsset.Account, fontArial10)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 20f });
                            tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(balanceAssetAmountForAccounts.ToString("#,##0.00"), fontArial10)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
                        }

                        document.Add(tableBalanceSheetAccounts);
                    }

                    document.Add(line);
                    foreach (var accountTypeSubCategory_Journal_Asset_Footer in accountTypeSubCategory_Journal_Assets)
                    {
                        // table Balance Sheet footer in Asset CAtegory
                        PdfPTable tableBalanceSheetFooterAsset = new PdfPTable(4);
                        float[] widthCellsTableBalanceSheetFooterAsset = new float[] { 20f, 20f, 150f, 30f };
                        tableBalanceSheetFooterAsset.SetWidths(widthCellsTableBalanceSheetFooterAsset);
                        tableBalanceSheetFooterAsset.WidthPercentage = 100;
                        tableBalanceSheetFooterAsset.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 25f });
                        tableBalanceSheetFooterAsset.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f });
                        tableBalanceSheetFooterAsset.AddCell(new PdfPCell(new Phrase("Total " + accountTypeSubCategory_Journal_Asset_Footer.SubCategoryDescription, fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
                        tableBalanceSheetFooterAsset.AddCell(new PdfPCell(new Phrase(totalCurrentAsset.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
                        tableBalanceSheetFooterAsset.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 25f });
                        tableBalanceSheetFooterAsset.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f });
                        tableBalanceSheetFooterAsset.AddCell(new PdfPCell(new Phrase("Total Asset", fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
                        tableBalanceSheetFooterAsset.AddCell(new PdfPCell(new Phrase(totalCurrentAsset.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });

                        document.Add(tableBalanceSheetFooterAsset);
                        document.Add(Chunk.NEWLINE);
                    }
                }
            }

            // retrieve account sub category journal Liabilities
            var accountTypeSubCategory_JournalLiabilities = from d in db.TrnJournals
                                                            where d.JournalDate <= Convert.ToDateTime(DateAsOf)
                                                            && d.MstAccount.MstAccountType.MstAccountCategory.Id == 2
                                                            && d.MstBranch.CompanyId == CompanyId
                                                            group d by new
                                                            {
                                                                AccountCategory = d.MstAccount.MstAccountType.MstAccountCategory.AccountCategory,
                                                                SubCategoryDescription = d.MstAccount.MstAccountType.SubCategoryDescription
                                                            } into g
                                                            select new Models.TrnJournal
                                                            {
                                                                AccountCategory = g.Key.AccountCategory,
                                                                SubCategoryDescription = g.Key.SubCategoryDescription,
                                                                DebitAmount = g.Sum(d => d.DebitAmount),
                                                                CreditAmount = g.Sum(d => d.CreditAmount)
                                                            };

            // retrieve account type journal Liabilities
            var accountTypeJournal_Liabilities = from d in db.TrnJournals
                                                 where d.JournalDate <= Convert.ToDateTime(DateAsOf)
                                                 && d.MstAccount.MstAccountType.MstAccountCategory.Id == 2
                                                 && d.MstBranch.CompanyId == CompanyId
                                                 group d by new
                                                 {
                                                     AccountType = d.MstAccount.MstAccountType.AccountType
                                                 } into g
                                                 select new Models.TrnJournal
                                                 {
                                                     AccountType = g.Key.AccountType,
                                                     DebitAmount = g.Sum(d => d.DebitAmount),
                                                     CreditAmount = g.Sum(d => d.CreditAmount)
                                                 };

            if (accountTypeSubCategory_JournalLiabilities.Any())
            {
                if (accountTypeJournal_Liabilities.Any())
                {
                    foreach (var accountTypeSubCategory_JournalsLiability in accountTypeSubCategory_JournalLiabilities)
                    {
                        // table Balance Sheet account Type Sub Category liabilities
                        PdfPTable tableBalanceSheetHeader = new PdfPTable(3);
                        float[] widthCellsTableBalanceSheetHeader = new float[] { 50f, 100f, 50f };
                        tableBalanceSheetHeader.SetWidths(widthCellsTableBalanceSheetHeader);
                        tableBalanceSheetHeader.WidthPercentage = 100;

                        document.Add(line);

                        PdfPCell headerSubCategoryColspan = (new PdfPCell(new Phrase(accountTypeSubCategory_JournalsLiability.SubCategoryDescription, fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, BackgroundColor = BaseColor.LIGHT_GRAY });
                        headerSubCategoryColspan.Colspan = 3;
                        tableBalanceSheetHeader.AddCell(headerSubCategoryColspan);

                        document.Add(tableBalanceSheetHeader);
                        //totalCurrentLiabilities = accountTypeSubCategory_JournalsLiability.CreditAmount;
                    }

                    foreach (var accountTypeJournal_Liability in accountTypeJournal_Liabilities)
                    {
                        Decimal balanceLiabilityAmount = accountTypeJournal_Liability.CreditAmount - accountTypeJournal_Liability.DebitAmount;

                        // table Balance Sheet liabilities
                        PdfPTable tableBalanceSheetAccounts = new PdfPTable(3);
                        float[] widthCellsTableBalanceSheetAccounts = new float[] { 50f, 100f, 50f };
                        tableBalanceSheetAccounts.SetWidths(widthCellsTableBalanceSheetAccounts);
                        tableBalanceSheetAccounts.WidthPercentage = 100;
                        tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(accountTypeJournal_Liability.AccountType, fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f, PaddingBottom = 5f, PaddingLeft = 25f });
                        tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f, PaddingBottom = 5f });
                        tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(balanceLiabilityAmount.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 10f, PaddingBottom = 5f });

                        totalCurrentLiabilities = totalCurrentLiabilities + balanceLiabilityAmount;

                        // retrieve accounts journal Liabilities
                        var accounts_JournalsLiabilities = from d in db.TrnJournals
                                                           where d.JournalDate <= Convert.ToDateTime(DateAsOf)
                                                           && d.MstAccount.MstAccountType.AccountType == accountTypeJournal_Liability.AccountType
                                                           && d.MstAccount.MstAccountType.MstAccountCategory.Id == 2
                                                           && d.MstBranch.CompanyId == CompanyId
                                                           group d by new
                                                           {
                                                               AccountCode = d.MstAccount.AccountCode,
                                                               Account = d.MstAccount.Account
                                                           } into g
                                                           select new Models.TrnJournal
                                                           {
                                                               AccountCode = g.Key.AccountCode,
                                                               Account = g.Key.Account,
                                                               DebitAmount = g.Sum(d => d.DebitAmount),
                                                               CreditAmount = g.Sum(d => d.CreditAmount)
                                                           };

                        foreach (var accounts_JournalsLiability in accounts_JournalsLiabilities)
                        {
                            Decimal balanceLiabilityAmountForAccounts = accounts_JournalsLiability.CreditAmount - accounts_JournalsLiability.DebitAmount;

                            tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(accounts_JournalsLiability.AccountCode, fontArial10)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
                            tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(accounts_JournalsLiability.Account, fontArial10)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 20f });
                            tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(balanceLiabilityAmountForAccounts.ToString("#,##0.00"), fontArial10)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
                        }

                        document.Add(tableBalanceSheetAccounts);
                    }

                    document.Add(line);

                    foreach (var accountTypeSubCategory_JournalsLiability_Footer in accountTypeSubCategory_JournalLiabilities)
                    {
                        // table Balance Sheet
                        PdfPTable tableBalanceSheetFooterLiabilities = new PdfPTable(4);
                        float[] widthCellsTableBalanceSheetFooterLiablities = new float[] { 20f, 20f, 150f, 30f };
                        tableBalanceSheetFooterLiabilities.SetWidths(widthCellsTableBalanceSheetFooterLiablities);
                        tableBalanceSheetFooterLiabilities.WidthPercentage = 100;
                        tableBalanceSheetFooterLiabilities.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 25f });
                        tableBalanceSheetFooterLiabilities.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f });
                        tableBalanceSheetFooterLiabilities.AddCell(new PdfPCell(new Phrase("Total " + accountTypeSubCategory_JournalsLiability_Footer.SubCategoryDescription, fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
                        tableBalanceSheetFooterLiabilities.AddCell(new PdfPCell(new Phrase(totalCurrentLiabilities.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
                        tableBalanceSheetFooterLiabilities.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 25f });
                        tableBalanceSheetFooterLiabilities.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f });
                        tableBalanceSheetFooterLiabilities.AddCell(new PdfPCell(new Phrase("Total Liabilities", fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
                        tableBalanceSheetFooterLiabilities.AddCell(new PdfPCell(new Phrase(totalCurrentLiabilities.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });

                        document.Add(tableBalanceSheetFooterLiabilities);
                        document.Add(Chunk.NEWLINE);
                    }
                }
            }

            // retrieve account sub category journal Equity
            var accountTypeSubCategory_JournalEquities = from d in db.TrnJournals
                                                         where d.JournalDate <= Convert.ToDateTime(DateAsOf)
                                                         && d.MstAccount.MstAccountType.MstAccountCategory.Id == 4
                                                         && d.MstBranch.CompanyId == CompanyId
                                                         group d by new
                                                         {
                                                             AccountCategory = d.MstAccount.MstAccountType.MstAccountCategory.AccountCategory,
                                                             SubCategoryDescription = d.MstAccount.MstAccountType.SubCategoryDescription
                                                         } into g
                                                         select new Models.TrnJournal
                                                         {
                                                             AccountCategory = g.Key.AccountCategory,
                                                             SubCategoryDescription = g.Key.SubCategoryDescription,
                                                             DebitAmount = g.Sum(d => d.DebitAmount),
                                                             CreditAmount = g.Sum(d => d.CreditAmount)
                                                         };

            // retrieve account type journal Equity
            var accountTypeJournal_Equities = from d in db.TrnJournals
                                              where d.JournalDate <= Convert.ToDateTime(DateAsOf)
                                              && d.MstAccount.MstAccountType.MstAccountCategory.Id == 4
                                              && d.MstBranch.CompanyId == CompanyId
                                              group d by new
                                              {
                                                  AccountType = d.MstAccount.MstAccountType.AccountType
                                              } into g
                                              select new Models.TrnJournal
                                              {
                                                  AccountType = g.Key.AccountType,
                                                  DebitAmount = g.Sum(d => d.DebitAmount),
                                                  CreditAmount = g.Sum(d => d.CreditAmount)
                                              };

            if (accountTypeSubCategory_JournalEquities.Any())
            {
                if (accountTypeJournal_Equities.Any())
                {
                    foreach (var accountTypeSubCategory_JournalEquity in accountTypeSubCategory_JournalEquities)
                    {
                        // table Balance Sheet Equity
                        PdfPTable tableBalanceSheetHeader = new PdfPTable(3);
                        float[] widthCellsTableBalanceSheetHeader = new float[] { 50f, 100f, 50f };
                        tableBalanceSheetHeader.SetWidths(widthCellsTableBalanceSheetHeader);
                        tableBalanceSheetHeader.WidthPercentage = 100;

                        document.Add(line);

                        PdfPCell headerSubCategoryColspan = (new PdfPCell(new Phrase(accountTypeSubCategory_JournalEquity.SubCategoryDescription, fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, BackgroundColor = BaseColor.LIGHT_GRAY });
                        headerSubCategoryColspan.Colspan = 3;
                        tableBalanceSheetHeader.AddCell(headerSubCategoryColspan);

                        document.Add(tableBalanceSheetHeader);
                        //totalStockHoldersEquity = accountTypeSubCategory_JournalEquity.CreditAmount;
                    }

                    foreach (var accountTypeJournal_Equity in accountTypeJournal_Equities)
                    {
                        Decimal balanceEquityAmount = accountTypeJournal_Equity.CreditAmount - accountTypeJournal_Equity.DebitAmount;

                        // table Balance Sheet Equity
                        PdfPTable tableBalanceSheetAccounts = new PdfPTable(3);
                        float[] widthCellsTableBalanceSheetAccounts = new float[] { 50f, 100f, 50f };
                        tableBalanceSheetAccounts.SetWidths(widthCellsTableBalanceSheetAccounts);
                        tableBalanceSheetAccounts.WidthPercentage = 100;
                        tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(accountTypeJournal_Equity.AccountType, fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f, PaddingBottom = 5f, PaddingLeft = 25f });
                        tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f, PaddingBottom = 5f });
                        tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(balanceEquityAmount.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 10f, PaddingBottom = 5f });

                        totalStockHoldersEquity = totalStockHoldersEquity + balanceEquityAmount;

                        // retrieve accounts journal Equity
                        var accounts_JournalEquities = from d in db.TrnJournals
                                                       where d.JournalDate <= Convert.ToDateTime(DateAsOf)
                                                       && d.MstAccount.MstAccountType.AccountType == accountTypeJournal_Equity.AccountType
                                                       && d.MstAccount.MstAccountType.MstAccountCategory.Id == 4
                                                       && d.MstBranch.CompanyId == CompanyId
                                                       group d by new
                                                       {
                                                           AccountCode = d.MstAccount.AccountCode,
                                                           Account = d.MstAccount.Account
                                                       } into g
                                                       select new Models.TrnJournal
                                                       {
                                                           AccountCode = g.Key.AccountCode,
                                                           Account = g.Key.Account,
                                                           DebitAmount = g.Sum(d => d.DebitAmount),
                                                           CreditAmount = g.Sum(d => d.CreditAmount)
                                                       };

                        foreach (var accounts_JournalEquity in accounts_JournalEquities)
                        {
                            Decimal balanceEquityAmountForAccounts = accounts_JournalEquity.CreditAmount - accounts_JournalEquity.DebitAmount;

                            tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(accounts_JournalEquity.AccountCode, fontArial10)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
                            tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(accounts_JournalEquity.Account, fontArial10)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 20f });
                            tableBalanceSheetAccounts.AddCell(new PdfPCell(new Phrase(balanceEquityAmountForAccounts.ToString("#,##0.00"), fontArial10)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
                        }

                        document.Add(tableBalanceSheetAccounts);
                    }

                    document.Add(line);

                    foreach (var accountTypeSubCategory_JournalEquity_Footer in accountTypeSubCategory_JournalEquities)
                    {
                        // table Balance Sheet
                        PdfPTable tableBalanceSheetFooterEquity = new PdfPTable(4);
                        float[] widthCellsTableBalanceSheetFooterEquity = new float[] { 20f, 20f, 150f, 30f };
                        tableBalanceSheetFooterEquity.SetWidths(widthCellsTableBalanceSheetFooterEquity);
                        tableBalanceSheetFooterEquity.WidthPercentage = 100;
                        tableBalanceSheetFooterEquity.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 25f });
                        tableBalanceSheetFooterEquity.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f });
                        tableBalanceSheetFooterEquity.AddCell(new PdfPCell(new Phrase("Total " + accountTypeSubCategory_JournalEquity_Footer.SubCategoryDescription, fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
                        tableBalanceSheetFooterEquity.AddCell(new PdfPCell(new Phrase(totalStockHoldersEquity.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
                        tableBalanceSheetFooterEquity.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 25f });
                        tableBalanceSheetFooterEquity.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f });
                        tableBalanceSheetFooterEquity.AddCell(new PdfPCell(new Phrase("Total Equity", fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
                        tableBalanceSheetFooterEquity.AddCell(new PdfPCell(new Phrase(totalStockHoldersEquity.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
                        document.Add(tableBalanceSheetFooterEquity);
                        document.Add(Chunk.NEWLINE);
                    }
                }
            }

            document.Add(line);

            Decimal totalLiabilityAndEquity = totalCurrentLiabilities + totalStockHoldersEquity;
            Decimal totalBalance = totalCurrentAsset - totalCurrentLiabilities - totalStockHoldersEquity;

            // table Balance Sheet
            PdfPTable tableBalanceSheetFooterTotalLiabilityAndEquity = new PdfPTable(4);
            float[] widthCellsTableBalanceSheetFooterTotalLiabilityAndEquity = new float[] { 20f, 20f, 150f, 30f };
            tableBalanceSheetFooterTotalLiabilityAndEquity.SetWidths(widthCellsTableBalanceSheetFooterTotalLiabilityAndEquity);
            tableBalanceSheetFooterTotalLiabilityAndEquity.WidthPercentage = 100;
            tableBalanceSheetFooterTotalLiabilityAndEquity.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 25f });
            tableBalanceSheetFooterTotalLiabilityAndEquity.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f });
            tableBalanceSheetFooterTotalLiabilityAndEquity.AddCell(new PdfPCell(new Phrase("Total Liability and Equity", fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
            tableBalanceSheetFooterTotalLiabilityAndEquity.AddCell(new PdfPCell(new Phrase(totalLiabilityAndEquity.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
            tableBalanceSheetFooterTotalLiabilityAndEquity.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 25f });
            tableBalanceSheetFooterTotalLiabilityAndEquity.AddCell(new PdfPCell(new Phrase("", fontArial10Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 5f });
            tableBalanceSheetFooterTotalLiabilityAndEquity.AddCell(new PdfPCell(new Phrase("Balance", fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 50f });
            tableBalanceSheetFooterTotalLiabilityAndEquity.AddCell(new PdfPCell(new Phrase(totalBalance.ToString("#,##0.00"), fontArial10Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f });
            document.Add(tableBalanceSheetFooterTotalLiabilityAndEquity);

            // Document End
            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}