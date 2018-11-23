using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace easyfis.Reports
{
    public class RepReceivingReceiptController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =======================
        // Receiving Receipt - PDF
        // =======================
        [Authorize]
        public ActionResult ReceivingReceipt(Int32 RRId)
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

            PdfPTable headerPage = new PdfPTable(2);
            headerPage.SetWidths(new float[] { 100f, 75f });
            headerPage.WidthPercentage = 100;
            headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
            headerPage.AddCell(new PdfPCell(new Phrase("Receiving Receipt", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(companyTaxNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase(branchName, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(companyAddress, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(companyContactNumber, fontArial11)) { Border = 0, PaddingTop = 5f, Colspan = 2 });

            document.Add(headerPage);
            document.Add(line);

            var receivingReceipt = from d in db.TrnReceivingReceipts where d.Id == RRId && d.IsLocked == true select d;
            if (receivingReceipt.Any())
            {
                String supplier = receivingReceipt.FirstOrDefault().MstArticle.Article;
                String term = receivingReceipt.FirstOrDefault().MstTerm.Term;
                String dueDate = receivingReceipt.FirstOrDefault().RRDate.AddDays(Convert.ToInt32(receivingReceipt.FirstOrDefault().MstTerm.NumberOfDays)).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String remarks = receivingReceipt.FirstOrDefault().Remarks;
                String RRNumber = receivingReceipt.FirstOrDefault().RRNumber;
                String RRDate = receivingReceipt.FirstOrDefault().RRDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String documentRef = receivingReceipt.FirstOrDefault().DocumentReference;
                String preparedBy = receivingReceipt.FirstOrDefault().MstUser3.FullName;
                String checkedBy = receivingReceipt.FirstOrDefault().MstUser1.FullName;
                String approvedBy = receivingReceipt.FirstOrDefault().MstUser.FullName;
                String receivedBy = receivingReceipt.FirstOrDefault().MstUser4.FullName;

                PdfPTable tblReceivingReceipt = new PdfPTable(4);
                tblReceivingReceipt.SetWidths(new float[] { 40f, 150f, 70f, 70f });
                tblReceivingReceipt.WidthPercentage = 100;
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase("Customer:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase(supplier, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase("No.:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase("RR-" + receivingReceipt.FirstOrDefault().MstBranch.BranchCode + "-" + RRNumber, fontArial13Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase("Term:", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase(term, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase("Date:", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase(RRDate, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase("Due Date:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase(dueDate, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase("Doc. Ref. No.:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase(documentRef, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase("Remarks:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase(remarks, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblReceivingReceipt.AddCell(new PdfPCell(new Phrase(" ", fontArial11)) { Colspan = 2, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });

                document.Add(tblReceivingReceipt);

                PdfPTable spaceTable = new PdfPTable(1);
                spaceTable.SetWidths(new float[] { 100f });
                spaceTable.WidthPercentage = 100;
                spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });

                document.Add(spaceTable);

                var receivingReceiptItems = from d in receivingReceipt.FirstOrDefault().TrnReceivingReceiptItems select d;
                if (receivingReceiptItems.Any())
                {
                    PdfPTable tblReceivingReceiptItems = new PdfPTable(8);
                    float[] widthsCellsReceivingReceiptItems = new float[] { 50f, 145f, 100f, 60f, 200f, 200f, 100f, 100f };
                    tblReceivingReceiptItems.SetWidths(widthsCellsReceivingReceiptItems);
                    tblReceivingReceiptItems.WidthPercentage = 100;
                    tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase("No.", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase("PO No.", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase("Item", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase("Branch", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase("Price", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase("Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    Decimal totalAmount = 0;
                    Decimal count = 0;

                    foreach (var receivingReceiptItem in receivingReceiptItems)
                    {
                        count += 1;
                        totalAmount += receivingReceiptItem.Amount;

                        string SKUCode = receivingReceiptItem.MstArticle.ManualArticleOldCode;
                        if (receivingReceiptItem.MstArticle.ManualArticleOldCode.Equals("NA") || receivingReceiptItem.MstArticle.ManualArticleOldCode.Equals("na"))
                        {
                            SKUCode = " ";
                        }

                        tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase(count.ToString("#,##0"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase("PO-" + receivingReceiptItem.TrnPurchaseOrder.MstBranch.BranchCode + "-" + receivingReceiptItem.TrnPurchaseOrder.PONumber, fontArial11)) { PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.Quantity.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.MstUnit.Unit, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.MstArticle.Article + " " + SKUCode, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.MstBranch.Branch, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.Cost.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.Amount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                    }

                    tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase("Total", fontArial11Bold)) { Colspan = 7, HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    tblReceivingReceiptItems.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                    document.Add(tblReceivingReceiptItems);
                    document.Add(spaceTable);
                }

                PdfPTable tblSignatures = new PdfPTable(4);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f, 100f };
                tblSignatures.WidthPercentage = 100;
                tblSignatures.SetWidths(widthsCellsTableUsers);
                tblSignatures.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase("Checked by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase("Approved by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase("Received by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(receivedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                document.Add(tblSignatures);

                PdfPTable tblFooter = new PdfPTable(1);
                tblFooter.SetWidths(new float[] { 100f });
                tblFooter.WidthPercentage = 100;
                tblFooter.AddCell(new PdfPCell(new Phrase("THIS DOCUMENT IS NOT VALID FOR CLAIM OF INPUT TAXES. THIS RECEIVING RECEIPT SHALL BE VALID FOR FIVE (5) YEARS FROM THE DATE OF ATP.", fontArial9Italic)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 1 });

                document.Add(tblFooter);
            }

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}