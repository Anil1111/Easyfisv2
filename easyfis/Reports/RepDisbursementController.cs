using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace easyfis.Reports
{
    public class RepDisbursementController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==================
        // Disbursement - PDF
        // ==================
        [Authorize]
        public ActionResult Disbursement(Int32 DisbursementId)
        {
            MemoryStream workStream = new MemoryStream();
            Rectangle rectangle = new Rectangle(PageSize.A3);
            Document document = new Document(rectangle, 72, 72, 72, 72);

            document.SetMargins(30f, 30f, 30f, 30f);

            PdfWriter.GetInstance(document, workStream).CloseStream = false;

            document.Open();

            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9Italic = FontFactory.GetFont("Arial", 9, Font.ITALIC);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            Font fontArial11 = FontFactory.GetFont("Arial", 11);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);
            Font fontArial12 = FontFactory.GetFont("Arial", 12);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial13 = FontFactory.GetFont("Arial", 13);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial17Bold = FontFactory.GetFont("Arial", 17, Font.BOLD);
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 4.5F)));

            var disbursement = from d in db.TrnDisbursements where d.Id == DisbursementId && d.IsLocked == true select d;
            if (disbursement.Any())
            {
                var identityUserId = User.Identity.GetUserId();
                var currentUser = from d in db.MstUsers where d.UserId == identityUserId select d;

                var currentCompanyId = currentUser.FirstOrDefault().CompanyId;
                var currentBranchId = currentUser.FirstOrDefault().BranchId;

                var currentCompany = from d in db.MstCompanies where d.Id == Convert.ToInt32(currentCompanyId) select d;
                var currentBranch = from d in db.MstBranches where d.Id == Convert.ToInt32(currentBranchId) select d;

                String companyName = currentCompany.FirstOrDefault().Company;
                String companyTaxNumber = currentCompany.FirstOrDefault().TaxNumber;
                String companyAddress = currentCompany.FirstOrDefault().Address;
                String companyContactNumber = currentCompany.FirstOrDefault().ContactNumber;
                String branchName = currentBranch.FirstOrDefault().Branch;
                String branchCode = currentBranch.FirstOrDefault().BranchCode;

                String reprinted = "";
                if (disbursement.FirstOrDefault().IsPrinted)
                {
                    reprinted = "Reprinted";
                }

                PdfPTable headerPage = new PdfPTable(2);
                headerPage.SetWidths(new float[] { 100f, 75f });
                headerPage.WidthPercentage = 100;
                headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
                headerPage.AddCell(new PdfPCell(new Phrase("Check/Cash Voucher", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyTaxNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(branchName, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyAddress, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyContactNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(reprinted, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });

                document.Add(headerPage);
                document.Add(line);

                String payee = disbursement.FirstOrDefault().Payee;
                String bank = disbursement.FirstOrDefault().MstArticle1.Article;
                String particulars = disbursement.FirstOrDefault().Particulars;
                String checkNo = disbursement.FirstOrDefault().CheckNumber;
                String CVNumber = disbursement.FirstOrDefault().CVNumber;
                String CVDate = disbursement.FirstOrDefault().CVDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String ManualCVNumber = disbursement.FirstOrDefault().ManualCVNumber;
                String checkDate = disbursement.FirstOrDefault().CheckDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String preparedBy = disbursement.FirstOrDefault().MstUser3.FullName;
                String checkedBy = disbursement.FirstOrDefault().MstUser1.FullName;
                String approvedBy = disbursement.FirstOrDefault().MstUser.FullName;

                PdfPTable tblDisbursement = new PdfPTable(4);
                float[] widthscellsTableDisbursement = new float[] { 40f, 150f, 70f, 70f };
                tblDisbursement.SetWidths(widthscellsTableDisbursement);
                tblDisbursement.WidthPercentage = 100;
                tblDisbursement.AddCell(new PdfPCell(new Phrase("Payee:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblDisbursement.AddCell(new PdfPCell(new Phrase(payee, fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblDisbursement.AddCell(new PdfPCell(new Phrase("No.:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblDisbursement.AddCell(new PdfPCell(new Phrase("CV-" + disbursement.FirstOrDefault().MstBranch.BranchCode + "-" + CVNumber, fontArial13Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblDisbursement.AddCell(new PdfPCell(new Phrase("Particulars:", fontArial11Bold)) { Rowspan = 4, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblDisbursement.AddCell(new PdfPCell(new Phrase(particulars, fontArial11Bold)) { Rowspan = 4, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblDisbursement.AddCell(new PdfPCell(new Phrase("CV Ref. No.:", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblDisbursement.AddCell(new PdfPCell(new Phrase(ManualCVNumber, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblDisbursement.AddCell(new PdfPCell(new Phrase("Date:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblDisbursement.AddCell(new PdfPCell(new Phrase(CVDate, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblDisbursement.AddCell(new PdfPCell(new Phrase("Check No.:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblDisbursement.AddCell(new PdfPCell(new Phrase(checkNo, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblDisbursement.AddCell(new PdfPCell(new Phrase("Check Date:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblDisbursement.AddCell(new PdfPCell(new Phrase(checkDate, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblDisbursement.AddCell(new PdfPCell(new Phrase("Bank:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblDisbursement.AddCell(new PdfPCell(new Phrase(bank, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                document.Add(tblDisbursement);

                PdfPTable spaceTable = new PdfPTable(1);
                spaceTable.SetWidths(new float[] { 100f });
                spaceTable.WidthPercentage = 100;
                spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });

                document.Add(spaceTable);

                var journals = from d in disbursement.FirstOrDefault().TrnJournals where d.CVId != null select d;
                if (journals.Any())
                {
                    PdfPTable tblJournal = new PdfPTable(6);
                    tblJournal.SetWidths(new float[] { 120f, 90f, 130f, 150f, 100f, 100f });
                    tblJournal.WidthPercentage = 100;
                    tblJournal.AddCell(new PdfPCell(new Phrase("Branch", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblJournal.AddCell(new PdfPCell(new Phrase("Code", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblJournal.AddCell(new PdfPCell(new Phrase("Account", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblJournal.AddCell(new PdfPCell(new Phrase("Article", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblJournal.AddCell(new PdfPCell(new Phrase("Debit", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblJournal.AddCell(new PdfPCell(new Phrase("Credit", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    Decimal totalDebitAmount = 0;
                    Decimal totalCreditAmount = 0;

                    foreach (var journal in journals)
                    {
                        tblJournal.AddCell(new PdfPCell(new Phrase(journal.MstBranch.Branch, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblJournal.AddCell(new PdfPCell(new Phrase(journal.MstAccount.AccountCode, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblJournal.AddCell(new PdfPCell(new Phrase(journal.MstAccount.Account, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblJournal.AddCell(new PdfPCell(new Phrase(journal.MstArticle.Article, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblJournal.AddCell(new PdfPCell(new Phrase(journal.DebitAmount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblJournal.AddCell(new PdfPCell(new Phrase(journal.CreditAmount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });

                        totalDebitAmount += journal.DebitAmount;
                        totalCreditAmount += journal.CreditAmount;
                    }

                    tblJournal.AddCell(new PdfPCell(new Phrase("Total", fontArial11Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    tblJournal.AddCell(new PdfPCell(new Phrase(totalDebitAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    tblJournal.AddCell(new PdfPCell(new Phrase(totalCreditAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                    document.Add(tblJournal);
                    document.Add(spaceTable);
                }

                var disbursementLines = from d in disbursement.FirstOrDefault().TrnDisbursementLines select d;
                if (disbursementLines.Any())
                {
                    PdfPTable tblDisbursementLines = new PdfPTable(4);
                    float[] widthscellsDisbursementLines = new float[] { 120f, 80f, 140f, 100f };
                    tblDisbursementLines.SetWidths(widthscellsDisbursementLines);
                    tblDisbursementLines.WidthPercentage = 100;
                    tblDisbursementLines.AddCell(new PdfPCell(new Phrase("RR No.", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblDisbursementLines.AddCell(new PdfPCell(new Phrase("RR Date", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblDisbursementLines.AddCell(new PdfPCell(new Phrase("Particulars", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblDisbursementLines.AddCell(new PdfPCell(new Phrase("Paid Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    Decimal totalPaidAmount = 0;

                    foreach (var disbursementLine in disbursementLines)
                    {
                        String RRNumber = " ", RRDate = " ";
                        if (disbursementLine.RRId != null)
                        {
                            RRNumber = disbursementLine.TrnReceivingReceipt.RRNumber;
                            RRDate = disbursementLine.TrnReceivingReceipt.RRDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                        }

                        tblDisbursementLines.AddCell(new PdfPCell(new Phrase(RRNumber, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblDisbursementLines.AddCell(new PdfPCell(new Phrase(RRDate, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblDisbursementLines.AddCell(new PdfPCell(new Phrase(disbursementLine.Particulars, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblDisbursementLines.AddCell(new PdfPCell(new Phrase(disbursementLine.Amount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });

                        totalPaidAmount += disbursementLine.Amount;
                    }

                    tblDisbursementLines.AddCell(new PdfPCell(new Phrase("Total", fontArial11Bold)) { Colspan = 3, HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    tblDisbursementLines.AddCell(new PdfPCell(new Phrase(totalPaidAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                    document.Add(tblDisbursementLines);
                    document.Add(spaceTable);
                }

                PdfPTable tblSignatures = new PdfPTable(3);
                tblSignatures.SetWidths(new float[] { 100f, 100f, 100f });
                tblSignatures.WidthPercentage = 100;
                tblSignatures.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase("Checked by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase("Approved by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                document.Add(tblSignatures);
                document.Add(spaceTable);
                document.Add(spaceTable);

                PdfPTable tableMoneyWord = new PdfPTable(3);
                tableMoneyWord.SetWidths(new float[] { 40f, 100f, 140f });
                tableMoneyWord.WidthPercentage = 100;
                tableMoneyWord.AddCell(new PdfPCell(new Phrase("Check No.", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableMoneyWord.AddCell(new PdfPCell(new Phrase(checkNo, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });

                String paidAmount = Convert.ToString(Math.Round(disbursement.FirstOrDefault().Amount * 100) / 100);

                var amountTablePhrase = new Phrase();
                var amountString = "ZERO";

                if (Convert.ToDecimal(paidAmount) != 0)
                {
                    amountString = GetMoneyWord(paidAmount).ToUpper();
                }

                amountTablePhrase.Add(new Chunk("Representing Payment from " + companyName + " the amount of ", fontArial11));
                amountTablePhrase.Add(new Chunk(amountString + " (P " + disbursement.FirstOrDefault().Amount.ToString("#,##0.00") + ")", fontArial11Bold));

                Paragraph paragraphAmountTable = new Paragraph();
                paragraphAmountTable.SetLeading(0, 1.4f);
                paragraphAmountTable.Add(amountTablePhrase);

                PdfPCell chunkyAmountTable = new PdfPCell();
                chunkyAmountTable.AddElement(paragraphAmountTable);
                chunkyAmountTable.BorderWidth = PdfPCell.NO_BORDER;

                tableMoneyWord.AddCell(new PdfPCell(chunkyAmountTable) { Rowspan = 4, Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableMoneyWord.AddCell(new PdfPCell(new Phrase("Check Date", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableMoneyWord.AddCell(new PdfPCell(new Phrase(checkDate, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableMoneyWord.AddCell(new PdfPCell(new Phrase("Bank", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableMoneyWord.AddCell(new PdfPCell(new Phrase(bank, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableMoneyWord.AddCell(new PdfPCell(new Phrase("Manual No.", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableMoneyWord.AddCell(new PdfPCell(new Phrase(ManualCVNumber, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });

                document.Add(tableMoneyWord);
                document.Add(Chunk.NEWLINE);
                document.Add(Chunk.NEWLINE);

                PdfPTable tableSignature = new PdfPTable(4);
                tableSignature.SetWidths(new float[] { 115f, 50f, 10f, 50f });
                tableSignature.WidthPercentage = 100;
                tableSignature.AddCell(new PdfPCell(new Phrase(" ", fontArial11Bold)) { Border = 0 });
                tableSignature.AddCell(new PdfPCell(new Phrase(" ", fontArial11Bold)) { Border = 0 });
                tableSignature.AddCell(new PdfPCell(new Phrase(" ", fontArial11Bold)) { Border = 0 });
                tableSignature.AddCell(new PdfPCell(new Phrase(" ", fontArial11Bold)) { Border = 0 });
                tableSignature.AddCell(new PdfPCell(new Phrase(" ", fontArial11)) { Border = 0, });
                tableSignature.AddCell(new PdfPCell(new Phrase("Signature Over Printed Name", fontArial11Bold)) { Border = 1, HorizontalAlignment = 1 });
                tableSignature.AddCell(new PdfPCell(new Phrase(" ", fontArial11Bold)) { Border = 0 });
                tableSignature.AddCell(new PdfPCell(new Phrase("Date", fontArial11Bold)) { Border = 1, HorizontalAlignment = 1 });

                document.Add(tableSignature);
                document.Add(spaceTable);

                PdfPTable tblFooter = new PdfPTable(1);
                tblFooter.SetWidths(new float[] { 100f });
                tblFooter.WidthPercentage = 100;
                tblFooter.AddCell(new PdfPCell(new Phrase("THIS DOCUMENT IS NOT VALID FOR CLAIM OF INPUT TAXES. THIS DISBURSEMENT SHALL BE VALID FOR FIVE (5) YEARS FROM THE DATE OF ATP.", fontArial9Italic)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 1 });

                document.Add(tblFooter);

                if (!disbursement.FirstOrDefault().IsPrinted)
                {
                    disbursement.FirstOrDefault().IsPrinted = true;
                    db.SubmitChanges();
                }
            }

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }

        // =====================
        // Convert To Money Word
        // =====================
        public static String GetMoneyWord(String input)
        {
            String decimals = "";
            if (input.Contains("."))
            {
                decimals = input.Substring(input.IndexOf(".") + 1);
                input = input.Remove(input.IndexOf("."));
            }

            String strWords = GetMoreThanThousandNumberWords(input);
            if (decimals.Length > 0)
            {
                if (Convert.ToDecimal(decimals) > 0)
                {
                    String getFirstRoundedDecimals = new String(decimals.Take(2).ToArray());
                    strWords += " Pesos And " + GetMoreThanThousandNumberWords(getFirstRoundedDecimals) + " Cents Only";
                }
                else
                {
                    strWords += " Pesos Only";
                }
            }
            else
            {
                strWords += " Pesos Only";
            }

            return strWords;
        }

        // ===================================
        // Get More Than Thousand Number Words
        // ===================================
        private static String GetMoreThanThousandNumberWords(string input)
        {
            try
            {
                String[] seperators = { "", " Thousand ", " Million ", " Billion " };

                int i = 0;

                String strWords = "";

                while (input.Length > 0)
                {
                    String _3digits = input.Length < 3 ? input : input.Substring(input.Length - 3);
                    input = input.Length < 3 ? "" : input.Remove(input.Length - 3);

                    Int32 no = Int32.Parse(_3digits);
                    _3digits = GetHundredNumberWords(no);

                    _3digits += seperators[i];
                    strWords = _3digits + strWords;

                    i++;
                }

                return strWords;
            }
            catch
            {
                return "Invalid Amount";
            }
        }

        // =====================================
        // Get From Ones to Hundred Number Words
        // =====================================
        private static String GetHundredNumberWords(Int32 no)
        {
            String[] Ones =
            {
                "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven",
                "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Ninteen"
            };

            String[] Tens = { "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
            String word = "";

            if (no > 99 && no < 1000)
            {
                Int32 i = no / 100;
                word = word + Ones[i - 1] + " Hundred ";
                no = no % 100;
            }

            if (no > 19 && no < 100)
            {
                Int32 i = no / 10;
                word = word + Tens[i - 1] + " ";
                no = no % 10;
            }

            if (no > 0 && no < 20)
            {
                word = word + Ones[no - 1];
            }

            return word;
        }
    }
}