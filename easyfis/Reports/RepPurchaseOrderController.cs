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
    public class RepPurchaseOrderController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ====================
        // Purchase Order - PDF
        // ====================
        [Authorize]
        public ActionResult PurchaseOrder(Int32 POId)
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
            headerPage.AddCell(new PdfPCell(new Phrase("Purchase Order", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(companyTaxNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase(branchName, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(companyAddress, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(companyContactNumber, fontArial11)) { Border = 0, PaddingTop = 5f, Colspan = 2 });

            document.Add(headerPage);
            document.Add(line);

            var purchaseOrder = from d in db.TrnPurchaseOrders where d.Id == Convert.ToInt32(POId) && d.IsLocked == true select d;
            if (purchaseOrder.Any())
            {
                String supplier = purchaseOrder.FirstOrDefault().MstArticle.Article;
                String PONumber = purchaseOrder.FirstOrDefault().PONumber;
                String term = purchaseOrder.FirstOrDefault().MstTerm.Term;
                String PODate = purchaseOrder.FirstOrDefault().PODate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String dateNeeded = purchaseOrder.FirstOrDefault().DateNeeded.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String requestNo = purchaseOrder.FirstOrDefault().ManualRequestNumber;
                String remarks = purchaseOrder.FirstOrDefault().Remarks;
                String preparedBy = purchaseOrder.FirstOrDefault().MstUser3.FullName;
                String checkedBy = purchaseOrder.FirstOrDefault().MstUser1.FullName;
                String approvedBy = purchaseOrder.FirstOrDefault().MstUser.FullName;
                String requestedBy = purchaseOrder.FirstOrDefault().MstUser4.FullName;

                PdfPTable tblPurchaseOrder = new PdfPTable(4);
                tblPurchaseOrder.SetWidths(new float[] { 40f, 150f, 70f, 70f });
                tblPurchaseOrder.WidthPercentage = 100;
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase("Supplier:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase(supplier, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase("No.:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase("PO-" + purchaseOrder.FirstOrDefault().MstBranch.BranchCode + "-" + PONumber, fontArial13Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase("Term:", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase(term, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase("Date:", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase(PODate, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase("Date Needed:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase(dateNeeded, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase("Request No.:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase(requestNo, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase("Remarks:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase(remarks, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblPurchaseOrder.AddCell(new PdfPCell(new Phrase(" ", fontArial11)) { Colspan = 2, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });

                document.Add(tblPurchaseOrder);

                PdfPTable spaceTable = new PdfPTable(1);
                spaceTable.SetWidths(new float[] { 100f });
                spaceTable.WidthPercentage = 100;
                spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });

                document.Add(spaceTable);

                var purchaseOrderItems = from d in purchaseOrder.FirstOrDefault().TrnPurchaseOrderItems select d;
                if (purchaseOrderItems.Any())
                {
                    PdfPTable tblPurchaseOrderItems = new PdfPTable(7);
                    tblPurchaseOrderItems.SetWidths(new float[] { 50f, 100f, 70f, 200f, 150f, 100f, 100f });
                    tblPurchaseOrderItems.WidthPercentage = 100;
                    tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase("No.", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase("Item", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase("Particulars", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase("Price", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase("Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    Int32 count = 0;
                    Decimal totalAmount = 0;

                    foreach (var purchaseOrderItem in purchaseOrderItems)
                    {
                        count += 1;
                        totalAmount += purchaseOrderItem.Amount;

                        string SKUCode = purchaseOrderItem.MstArticle.ManualArticleOldCode;
                        if (purchaseOrderItem.MstArticle.ManualArticleOldCode.Equals("NA") || purchaseOrderItem.MstArticle.ManualArticleOldCode.Equals("na"))
                        {
                            SKUCode = " ";
                        }

                        tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase(count.ToString("#,##0"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase(purchaseOrderItem.Quantity.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase(purchaseOrderItem.MstUnit.Unit, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase(purchaseOrderItem.MstArticle.Article + " " + SKUCode, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase(purchaseOrderItem.Particulars, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase(purchaseOrderItem.Cost.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase(purchaseOrderItem.Amount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                    }

                    tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase("Total", fontArial11Bold)) { Colspan = 6, HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    tblPurchaseOrderItems.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                    document.Add(tblPurchaseOrderItems);
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

                PdfPTable tblFooter = new PdfPTable(1);
                tblFooter.SetWidths(new float[] { 100f });
                tblFooter.WidthPercentage = 100;
                tblFooter.AddCell(new PdfPCell(new Phrase("THIS DOCUMENT IS NOT VALID FOR CLAIM OF INPUT TAXES. THIS PURCHASE ORDER SHALL BE VALID FOR FIVE (5) YEARS FROM THE DATE OF ATP.", fontArial9Italic)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 1 });

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