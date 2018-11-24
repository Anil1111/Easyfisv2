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
    public class RepPurchaseRequestController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ======================
        // Purchase Request - PDF
        // ======================
        [Authorize]
        public ActionResult PurchaseRequest(Int32 PRId)
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

            var purchaseRequest = from d in db.TrnPurchaseRequests where d.Id == Convert.ToInt32(PRId) && d.IsLocked == true select d;
            if (purchaseRequest.Any())
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
                if (purchaseRequest.FirstOrDefault().IsPrinted)
                {
                    reprinted = "Reprinted";
                }

                PdfPTable headerPage = new PdfPTable(2);
                headerPage.SetWidths(new float[] { 100f, 75f });
                headerPage.WidthPercentage = 100;
                headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
                headerPage.AddCell(new PdfPCell(new Phrase("Purchase Request", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyTaxNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(branchName, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyAddress, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyContactNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(reprinted, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });

                document.Add(headerPage);
                document.Add(line);

                String supplier = purchaseRequest.FirstOrDefault().MstArticle.Article;
                String PRNumber = purchaseRequest.FirstOrDefault().PRNumber;
                String term = purchaseRequest.FirstOrDefault().MstTerm.Term;
                String PRDate = purchaseRequest.FirstOrDefault().PRDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String dateNeeded = purchaseRequest.FirstOrDefault().DateNeeded.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String remarks = purchaseRequest.FirstOrDefault().Remarks;
                String preparedBy = purchaseRequest.FirstOrDefault().MstUser3.FullName;
                String checkedBy = purchaseRequest.FirstOrDefault().MstUser1.FullName;
                String approvedBy = purchaseRequest.FirstOrDefault().MstUser.FullName;
                String requestedBy = purchaseRequest.FirstOrDefault().MstUser4.FullName;

                PdfPTable tblPurchaseRequest = new PdfPTable(4);
                tblPurchaseRequest.SetWidths(new float[] { 40f, 150f, 70f, 70f });
                tblPurchaseRequest.WidthPercentage = 100;
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase("Supplier", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase(supplier, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase("No.", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase("PR-" + purchaseRequest.FirstOrDefault().MstBranch.BranchCode + "-" + PRNumber, fontArial13Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase("Term", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase(term, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase("Date", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase(PRDate, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase("Date Needed", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase(dateNeeded, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase(" ", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase(" ", fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase("Remarks ", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase(remarks, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseRequest.AddCell(new PdfPCell(new Phrase(" ", fontArial11)) { Colspan = 2, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });

                document.Add(tblPurchaseRequest);

                PdfPTable spaceTable = new PdfPTable(1);
                spaceTable.SetWidths(new float[] { 100f });
                spaceTable.WidthPercentage = 100;
                spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });

                document.Add(spaceTable);

                var purchaseRequestItems = from d in purchaseRequest.FirstOrDefault().TrnPurchaseRequestItems select d;
                if (purchaseRequestItems.Any())
                {
                    PdfPTable tblPurchaseRequestLines = new PdfPTable(7);
                    tblPurchaseRequestLines.SetWidths(new float[] { 50f, 100f, 70f, 200f, 150f, 100f, 100f });
                    tblPurchaseRequestLines.WidthPercentage = 100;
                    tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase("No.", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Quantity", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Unit", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Item", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Particulars", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Price", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    Decimal totalAmount = 0;
                    Int32 count = 0;

                    foreach (var purchaseRequestItem in purchaseRequestItems)
                    {
                        count += 1;
                        totalAmount += purchaseRequestItem.Amount;

                        tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase(count.ToString("#,##0"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase(purchaseRequestItem.Quantity.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase(purchaseRequestItem.MstUnit.Unit, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase(purchaseRequestItem.MstArticle.Article, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase(purchaseRequestItem.Particulars, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase(purchaseRequestItem.Cost.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase(purchaseRequestItem.Amount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                    }

                    tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Total", fontArial11Bold)) { Colspan = 6, HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    tblPurchaseRequestLines.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                    document.Add(tblPurchaseRequestLines);
                    document.Add(spaceTable);
                }

                PdfPTable tblSignatures = new PdfPTable(4);
                tblSignatures.SetWidths(new float[] { 100f, 100f, 100f, 100f });
                tblSignatures.WidthPercentage = 100;
                tblSignatures.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase("Checked by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase("Approved by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase("Requested by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(requestedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                document.Add(tblSignatures);

                if (!purchaseRequest.FirstOrDefault().IsPrinted)
                {
                    purchaseRequest.FirstOrDefault().IsPrinted = true;
                    db.SubmitChanges();
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