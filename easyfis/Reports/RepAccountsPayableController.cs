using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace easyfis.Controllers
{
    public class RepAccountsPayableController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Compute Age
        // ===========
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

        // =====================
        // Preview and Print PDF
        // =====================
        [Authorize]
        public ActionResult AccountsPayableReport(String DateAsOf, Int32 CompanyId, Int32 BranchId, Int32 AccountId)
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
            var fontName = "Calibri";
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\calibri.ttf";
                FontFactory.Register(fontPath);
            }

            Font fontArial17Bold = FontFactory.GetFont("Tahoma", 17, Font.BOLD);
            Font fontArial11 = FontFactory.GetFont("Tahoma", 11);
            Font fontArial9Bold = FontFactory.GetFont("Tahoma", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Tahoma", 9);
            Font fontArial9Italic = FontFactory.GetFont("Tahoma", 9, Font.ITALIC);
            Font fontArial12Bold = FontFactory.GetFont("Tahoma", 12, Font.BOLD);
            Font fontArial10Bold = FontFactory.GetFont("Tahoma", 10, Font.BOLD);

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
            headerPage.AddCell(new PdfPCell(new Phrase("Accounts Payable", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(address, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase("Date as of " + Convert.ToDateTime(DateAsOf).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2, });
            headerPage.AddCell(new PdfPCell(new Phrase(contactNo, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            document.Add(headerPage);
            document.Add(line);

            // ==================
            // Receiving Receipts
            // ==================
            var receivingReceipts = from d in db.TrnReceivingReceipts
                                    where d.RRDate <= Convert.ToDateTime(DateAsOf)
                                    && d.MstBranch.CompanyId == CompanyId
                                    && d.BranchId == BranchId
                                    && d.MstArticle.AccountId == AccountId
                                    && d.BalanceAmount > 0
                                    && d.IsLocked == true
                                    select new
                                    {
                                        Branch = d.MstBranch.Branch,
                                        AccountId = d.MstArticle.AccountId,
                                        AccountCode = d.MstArticle.MstAccount.AccountCode,
                                        Account = d.MstArticle.MstAccount.Account,
                                        RRNumber = d.RRNumber,
                                        RRDate = d.RRDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture),
                                        SupplierId = d.SupplierId,
                                        Supplier = d.MstArticle.Article,
                                        DocumentReference = d.DocumentReference,
                                        DueDate = d.RRDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays)).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture),
                                        BalanceAmount = d.BalanceAmount,
                                        CurrentAmount = ComputeAge(0, Convert.ToDateTime(DateAsOf).Subtract(d.RRDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount),
                                        Age30Amount = ComputeAge(1, Convert.ToDateTime(DateAsOf).Subtract(d.RRDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount),
                                        Age60Amount = ComputeAge(2, Convert.ToDateTime(DateAsOf).Subtract(d.RRDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount),
                                        Age90Amount = ComputeAge(3, Convert.ToDateTime(DateAsOf).Subtract(d.RRDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount),
                                        Age120Amount = ComputeAge(4, Convert.ToDateTime(DateAsOf).Subtract(d.RRDate.AddDays(Convert.ToInt32(d.MstTerm.NumberOfDays))).Days, d.BalanceAmount)
                                    };

            if (receivingReceipts.Any())
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
                // Account Title 
                // =============
                PdfPTable accountTitle = new PdfPTable(1);
                float[] widthCellsAccountTitle = new float[] { 100f };
                accountTitle.SetWidths(widthCellsAccountTitle);
                accountTitle.WidthPercentage = 100;
                accountTitle.AddCell(new PdfPCell(new Phrase(receivingReceipts.FirstOrDefault().AccountCode + " - " + receivingReceipts.FirstOrDefault().Account, fontArial12Bold)) { Border = 0, HorizontalAlignment = 0, PaddingBottom = 5f });
                document.Add(accountTitle);

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
                tableHeader.AddCell(new PdfPCell(new Phrase("RR Number", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("RR Date", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("Document Ref.", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("Due Date", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("Balance", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("Current", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("30 Days", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("60 Days", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("90 Days", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                tableHeader.AddCell(new PdfPCell(new Phrase("Over 120 Days", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f });
                document.Add(tableHeader);

                var receivingReceipGroupedSuppliers = from d in receivingReceipts
                                                      group d by new
                                                      {
                                                          SupplierId = d.SupplierId,
                                                          Supplier = d.Supplier
                                                      } into g
                                                      select new
                                                      {
                                                          SupplierId = g.Key.SupplierId,
                                                          Supplier = g.Key.Supplier,
                                                          BalanceAmount = g.Sum(d => d.BalanceAmount)
                                                      };

                if (receivingReceipGroupedSuppliers.Any())
                {
                    Decimal accountSubTotalBalanceAmount = 0;
                    Decimal accountSubTotalCurrentAmount = 0;
                    Decimal accountSubTotalAge30Amount = 0;
                    Decimal accountSubTotalAge60Amount = 0;
                    Decimal accountSubTotalAge90Amount = 0;
                    Decimal accountSubTotalAge120AmountAmount = 0;

                    foreach (var receivingReceipGroupedSupplier in receivingReceipGroupedSuppliers)
                    {
                        // =============
                        // Supplier Name
                        // =============
                        PdfPTable supplierName = new PdfPTable(1);
                        float[] widthCellsSupplierName = new float[] { 100f };
                        supplierName.SetWidths(widthCellsSupplierName);
                        supplierName.WidthPercentage = 100;
                        supplierName.AddCell(new PdfPCell(new Phrase(receivingReceipGroupedSupplier.Supplier, fontArial10Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 9f, PaddingBottom = 6f });
                        document.Add(supplierName);

                        var receivingReceiptsComputeAges = from d in receivingReceipts
                                                           where d.SupplierId == receivingReceipGroupedSupplier.SupplierId
                                                           select new
                                                           {
                                                               RRNumber = d.RRNumber,
                                                               RRDate = d.RRDate,
                                                               DocumentReference = d.DocumentReference,
                                                               BalanceAmount = d.BalanceAmount,
                                                               DueDate = d.DueDate,
                                                               CurrentAmount = d.CurrentAmount,
                                                               Age30Amount = d.Age30Amount,
                                                               Age60Amount = d.Age60Amount,
                                                               Age90Amount = d.Age90Amount,
                                                               Age120Amount = d.Age120Amount
                                                           };

                        if (receivingReceiptsComputeAges.Any())
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

                            foreach (var receivingReceiptsWithComputeAge in receivingReceiptsComputeAges)
                            {
                                subTotalBalanceAmount += receivingReceiptsWithComputeAge.BalanceAmount;
                                subTotalCurrentAmount += receivingReceiptsWithComputeAge.CurrentAmount;
                                subTotalAge30Amount += receivingReceiptsWithComputeAge.Age30Amount;
                                subTotalAge60Amount += receivingReceiptsWithComputeAge.Age60Amount;
                                subTotalAge90Amount += receivingReceiptsWithComputeAge.Age90Amount;
                                subTotalAge120AmountAmount += receivingReceiptsWithComputeAge.Age120Amount;

                                data.AddCell(new PdfPCell(new Phrase(receivingReceiptsWithComputeAge.RRNumber, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(receivingReceiptsWithComputeAge.RRDate, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(receivingReceiptsWithComputeAge.DocumentReference, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(receivingReceiptsWithComputeAge.DueDate, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(receivingReceiptsWithComputeAge.BalanceAmount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(receivingReceiptsWithComputeAge.CurrentAmount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(receivingReceiptsWithComputeAge.Age30Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(receivingReceiptsWithComputeAge.Age60Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(receivingReceiptsWithComputeAge.Age90Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                                data.AddCell(new PdfPCell(new Phrase(receivingReceiptsWithComputeAge.Age120Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 1f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f, Border = 0 });
                            }

                            // =========
                            // Sub Total
                            // =========
                            data.AddCell(new PdfPCell(new Phrase(receivingReceipGroupedSupplier.Supplier + " Total", fontArial9Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 6f, PaddingBottom = 6f, PaddingRight = 10f, PaddingLeft = 10f, Border = 1 });
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
                    document.Add(spaceTable);

                    // ============================
                    // Account Title with Sub Total
                    // ============================
                    PdfPTable accountTitleSubTotal = new PdfPTable(10);
                    float[] widthsCellsAccountTitleSubTotal = new float[] { 100f, 100f, 120f, 100f, 100f, 100f, 100f, 100f, 100f, 100f };
                    accountTitleSubTotal.SetWidths(widthsCellsAccountTitleSubTotal);
                    accountTitleSubTotal.WidthPercentage = 100;
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(receivingReceipts.FirstOrDefault().AccountCode + " - " + receivingReceipts.FirstOrDefault().Account + " Total", fontArial9Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 10f, PaddingLeft = 10f });
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(accountSubTotalBalanceAmount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f });
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(accountSubTotalCurrentAmount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f });
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(accountSubTotalAge30Amount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f });
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(accountSubTotalAge60Amount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f });
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(accountSubTotalAge90Amount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f });
                    accountTitleSubTotal.AddCell(new PdfPCell(new Phrase(accountSubTotalAge120AmountAmount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingRight = 5f, PaddingLeft = 5f });

                    document.Add(accountTitleSubTotal);
                }
            }

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}