using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace easyfis.Controllers
{
    public class RepStatementOfAccountController : Controller
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
        // Statement of Account Report PDF
        // ===============================
        [Authorize]
        public ActionResult StatementOfAccount(String DateAsOf, Int32 CompanyId, Int32 BranchId, Int32 CustomerId)
        {
            // ========================
            // PDF settings and Formats
            // ========================
            MemoryStream workStream = new MemoryStream();
            Rectangle rectangle = new Rectangle(PageSize.A3);
            Document document = new Document(rectangle, 72, 72, 72, 72);
            document.SetMargins(30f, 30f, 30f, 30f);
            PdfWriter.GetInstance(document, workStream).CloseStream = false;

            document.Open();

            // ===================
            // Fonts Customization
            // ===================
            Font fontArial17Bold = FontFactory.GetFont("Arial", 17, Font.BOLD);
            Font fontArial11 = FontFactory.GetFont("Arial", 11);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Italic = FontFactory.GetFont("Arial", 9, Font.ITALIC);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);

            // ====
            // line
            // ====
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 4.5F)));

            // ==============
            // Company Detail
            // ==============
            var companyName = (from d in db.MstCompanies where d.Id == CompanyId select d).FirstOrDefault().Company;
            var address = (from d in db.MstCompanies where d.Id == CompanyId select d).FirstOrDefault().Address;
            var contactNo = (from d in db.MstCompanies where d.Id == CompanyId select d).FirstOrDefault().ContactNumber;

            // =================
            // table main header
            // =================
            PdfPTable headerPage = new PdfPTable(2);
            float[] widthsCellsHeaderPage = new float[] { 100f, 75f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;
            headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
            headerPage.AddCell(new PdfPCell(new Phrase("Statement of Account", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(address, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase("Date as of " + Convert.ToDateTime(DateAsOf).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2, });
            headerPage.AddCell(new PdfPCell(new Phrase(contactNo, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            document.Add(headerPage);
            document.Add(line);

            // ==============
            // Sales Invoices
            // ==============
            var salesInvoices = from d in db.TrnSalesInvoices
                                where d.SIDate <= Convert.ToDateTime(DateAsOf)
                                && d.MstBranch.CompanyId == Convert.ToInt32(CompanyId)
                                && d.BranchId == Convert.ToInt32(BranchId)
                                && d.CustomerId == Convert.ToInt32(CustomerId)
                                && d.BalanceAmount > 0
                                && d.IsLocked == true
                                select new
                                {
                                    SIId = d.Id,
                                    Branch = d.MstBranch.Branch,
                                    AccountId = d.MstArticle.AccountId,
                                    AccountCode = d.MstArticle.MstAccount.AccountCode,
                                    Account = d.MstArticle.MstAccount.Account,
                                    SINumber = d.SINumber,
                                    SIDate = d.SIDate.ToShortDateString(),
                                    CustomerId = d.CustomerId,
                                    Customer = d.MstArticle.Article,
                                    DocumentReference = d.DocumentReference,
                                    DueDate = d.SIDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays)).ToShortDateString(),
                                    BalanceAmount = d.BalanceAmount,
                                    CurrentAmount = ComputeAge(0, Convert.ToDateTime(DateAsOf).Subtract(d.SIDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount),
                                    Age30Amount = ComputeAge(1, Convert.ToDateTime(DateAsOf).Subtract(d.SIDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount),
                                    Age60Amount = ComputeAge(2, Convert.ToDateTime(DateAsOf).Subtract(d.SIDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount),
                                    Age90Amount = ComputeAge(3, Convert.ToDateTime(DateAsOf).Subtract(d.SIDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount),
                                    Age120Amount = ComputeAge(4, Convert.ToDateTime(DateAsOf).Subtract(d.SIDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount)
                                };

            if (salesInvoices.Any())
            {
                // ============
                // Branch Title 
                // ============
                var branch = from d in db.MstBranches where d.Id == BranchId select d;
                String branchName = "N/A";
                if (branch.Any())
                {
                    branchName = branch.FirstOrDefault().Branch;
                }
                PdfPTable branchTitle = new PdfPTable(1);
                float[] widthCellsBranchTitle = new float[] { 100f };
                branchTitle.SetWidths(widthCellsBranchTitle);
                branchTitle.WidthPercentage = 100;
                branchTitle.AddCell(new PdfPCell(new Phrase(branchName, fontArial12Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f });
                document.Add(branchTitle);

                // =============
                // Customer Name
                // =============
                PdfPTable customerName = new PdfPTable(1);
                float[] widthCellsCustomerName = new float[] { 100f };
                customerName.SetWidths(widthCellsCustomerName);
                customerName.WidthPercentage = 100;
                customerName.AddCell(new PdfPCell(new Phrase(salesInvoices.FirstOrDefault().Customer, fontArial12Bold)) { Border = 0, HorizontalAlignment = 0, PaddingBottom = 5f });
                document.Add(customerName);

                // =====
                // Space
                // =====
                PdfPTable spaceTable = new PdfPTable(1);
                float[] widthCellsSpaceTable = new float[] { 100f };
                spaceTable.SetWidths(widthCellsSpaceTable);
                spaceTable.WidthPercentage = 100;
                spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });
                document.Add(spaceTable);

                // ======================
                // Receiving Receipt Data
                // ======================
                PdfPTable tableHeader = new PdfPTable(10);
                float[] widthCellsTableHeader = new float[] { 100f, 100f, 120f, 100f, 100f, 100f, 100f, 100f, 100f, 100f };
                tableHeader.SetWidths(widthCellsTableHeader);
                tableHeader.WidthPercentage = 100;
                tableHeader.AddCell(new PdfPCell(new Phrase("SI Number", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("SI Date", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("Document Ref.", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("Due Date", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("Balance", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("Current", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("30 Days", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("60 Days", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("90 Days", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("Over 120 Days", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                document.Add(tableHeader);

                var salesInvoiceGroupedAccounts = from d in salesInvoices
                                                  group d by new
                                                  {
                                                      AccountId = d.AccountId,
                                                      AccountCode = d.AccountCode,
                                                      Account = d.Account
                                                  } into g
                                                  select new
                                                  {
                                                      AccountId = g.Key.AccountId,
                                                      AccountCode = g.Key.AccountCode,
                                                      Account = g.Key.Account,
                                                      BalanceAmount = g.Sum(d => d.BalanceAmount)
                                                  };

                if (salesInvoiceGroupedAccounts.Any())
                {
                    Decimal accountSubTotalBalanceAmount = 0;
                    Decimal accountSubTotalCurrentAmount = 0;
                    Decimal accountSubTotalAge30Amount = 0;
                    Decimal accountSubTotalAge60Amount = 0;
                    Decimal accountSubTotalAge90Amount = 0;
                    Decimal accountSubTotalAge120AmountAmount = 0;

                    foreach (var salesInvoiceGroupedAccount in salesInvoiceGroupedAccounts)
                    {
                        // =============
                        // Account Title
                        // =============
                        PdfPTable accountTitle = new PdfPTable(1);
                        float[] widthCellsAccountTitle = new float[] { 100f };
                        accountTitle.SetWidths(widthCellsAccountTitle);
                        accountTitle.WidthPercentage = 100;
                        accountTitle.AddCell(new PdfPCell(new Phrase(salesInvoiceGroupedAccount.AccountCode + " - " + salesInvoiceGroupedAccount.Account, fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 9f, PaddingBottom = 6f });
                        document.Add(accountTitle);

                        var salesInvoicesComputeAges = from d in salesInvoices
                                                       where d.AccountId == salesInvoiceGroupedAccount.AccountId
                                                       select new
                                                       {
                                                           SINumber = d.SINumber,
                                                           SIDate = d.SIDate,
                                                           DocumentReference = d.DocumentReference,
                                                           BalanceAmount = d.BalanceAmount,
                                                           DueDate = d.DueDate,
                                                           CurrentAmount = d.CurrentAmount,
                                                           Age30Amount = d.Age30Amount,
                                                           Age60Amount = d.Age60Amount,
                                                           Age90Amount = d.Age90Amount,
                                                           Age120Amount = d.Age120Amount
                                                       };

                        if (salesInvoicesComputeAges.Any())
                        {
                            Decimal subTotalBalanceAmount = 0;
                            Decimal subTotalCurrentAmount = 0;
                            Decimal subTotalAge30Amount = 0;
                            Decimal subTotalAge60Amount = 0;
                            Decimal subTotalAge90Amount = 0;
                            Decimal subTotalAge120AmountAmount = 0;

                            PdfPTable data = new PdfPTable(10);
                            float[] widthCellsData = new float[] { 100f, 100f, 120f, 100f, 100f, 100f, 100f, 100f, 100f, 100f };
                            data.SetWidths(widthCellsData);
                            data.WidthPercentage = 100;

                            foreach (var salesInvoicesWithComputeAge in salesInvoicesComputeAges)
                            {
                                subTotalBalanceAmount += salesInvoicesWithComputeAge.BalanceAmount;
                                subTotalCurrentAmount += salesInvoicesWithComputeAge.CurrentAmount;
                                subTotalAge30Amount += salesInvoicesWithComputeAge.Age30Amount;
                                subTotalAge60Amount += salesInvoicesWithComputeAge.Age60Amount;
                                subTotalAge90Amount += salesInvoicesWithComputeAge.Age90Amount;
                                subTotalAge120AmountAmount += salesInvoicesWithComputeAge.Age120Amount;

                                data.AddCell(new PdfPCell(new Phrase(salesInvoicesWithComputeAge.SINumber, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoicesWithComputeAge.SIDate, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoicesWithComputeAge.DocumentReference, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoicesWithComputeAge.DueDate, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoicesWithComputeAge.BalanceAmount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoicesWithComputeAge.CurrentAmount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoicesWithComputeAge.Age30Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoicesWithComputeAge.Age60Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoicesWithComputeAge.Age90Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoicesWithComputeAge.Age120Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                            }

                            // =========
                            // Sub Total
                            // =========
                            data.AddCell(new PdfPCell(new Phrase(salesInvoiceGroupedAccount.AccountCode + " - " + salesInvoiceGroupedAccount.Account + " Total", fontArial9Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 6f, PaddingBottom = 6f, PaddingRight = 10f, PaddingLeft = 10f, Border = 1 });
                            data.AddCell(new PdfPCell(new Phrase(subTotalBalanceAmount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 6f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 1 });
                            data.AddCell(new PdfPCell(new Phrase(subTotalCurrentAmount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 6f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 1 });
                            data.AddCell(new PdfPCell(new Phrase(subTotalAge30Amount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 6f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 1 });
                            data.AddCell(new PdfPCell(new Phrase(subTotalAge60Amount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 6f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 1 });
                            data.AddCell(new PdfPCell(new Phrase(subTotalAge90Amount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 6f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 1 });
                            data.AddCell(new PdfPCell(new Phrase(subTotalAge120AmountAmount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 6f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 1 });

                            document.Add(data);

                            accountSubTotalBalanceAmount = accountSubTotalBalanceAmount + subTotalBalanceAmount;
                            accountSubTotalCurrentAmount = accountSubTotalCurrentAmount + subTotalCurrentAmount;
                            accountSubTotalAge30Amount = accountSubTotalAge30Amount + subTotalAge30Amount;
                            accountSubTotalAge60Amount = accountSubTotalAge60Amount + subTotalAge60Amount;
                            accountSubTotalAge90Amount = accountSubTotalAge90Amount + subTotalAge90Amount;
                            accountSubTotalAge120AmountAmount = accountSubTotalAge120AmountAmount + subTotalAge120AmountAmount;
                        }
                    }

                    document.Add(spaceTable);

                    // ============================
                    // Account Title with Sub Total
                    // ============================
                    PdfPTable accountTitleSubTotal = new PdfPTable(10);
                    float[] widthsCellsAccountTitleSubTotal = new float[] { 100f, 100f, 120f, 100f, 100f, 100f, 100f, 100f, 100f, 100f };
                    accountTitleSubTotal.SetWidths(widthsCellsAccountTitleSubTotal);
                    accountTitleSubTotal.WidthPercentage = 100;
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(salesInvoices.FirstOrDefault().Customer + " Total", fontArial9Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 10f, PaddingLeft = 10f });
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(accountSubTotalBalanceAmount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f });
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(accountSubTotalCurrentAmount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f });
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(accountSubTotalAge30Amount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f });
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(accountSubTotalAge60Amount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f });
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(accountSubTotalAge90Amount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f });
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(accountSubTotalAge120AmountAmount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f });

                    document.Add(accountTitleSubTotal);
                }

                var identityUserId = User.Identity.GetUserId();
                var currentUser = from d in db.MstUsers
                                  where d.UserId == identityUserId
                                  select d;

                var preparedBy = currentUser.FirstOrDefault().FullName;

                var checkedBy = " ";
                var approvedBy = " ";

                var mstUsers = from d in db.MstUsers
                               select new
                               {
                                   Id = d.Id,
                                   FullName = d.FullName
                               };

                if (mstUsers.Any())
                {
                    var checkedByUser = from d in mstUsers
                                        where d.Id == currentUser.FirstOrDefault().SalesInvoiceCheckedById
                                        select d;

                    if (checkedByUser.Any())
                    {
                        checkedBy = checkedByUser.FirstOrDefault().FullName;
                    }

                    var approvedByUser = from d in mstUsers
                                         where d.Id == currentUser.FirstOrDefault().SalesInvoiceApprovedById
                                         select d;

                    if (approvedByUser.Any())
                    {
                        approvedBy = approvedByUser.FirstOrDefault().FullName;
                    }
                }

                document.Add(spaceTable);
                document.Add(spaceTable);

                // ==============
                // User Signature
                // ==============
                PdfPTable tableUsers = new PdfPTable(4);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f, 100f };
                tableUsers.WidthPercentage = 100;
                tableUsers.SetWidths(widthsCellsTableUsers);
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase("Received by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 50f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 50f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 50f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 50f });
                tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase("Date: ", fontArial11Bold)) { HorizontalAlignment = 0, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                document.Add(tableUsers);
            }

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}