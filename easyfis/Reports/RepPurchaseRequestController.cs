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
            // ==============================
            // PDF Settings and Customization
            // ==============================
            MemoryStream workStream = new MemoryStream();
            Rectangle rectangle = new Rectangle(PageSize.A3);
            Document document = new Document(rectangle, 72, 72, 72, 72);
            document.SetMargins(30f, 30f, 30f, 30f);
            PdfWriter.GetInstance(document, workStream).CloseStream = false;

            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial17Bold = FontFactory.GetFont("Arial", 17, Font.BOLD);
            Font fontArial11 = FontFactory.GetFont("Arial", 11);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);

            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 4.5F)));

            var identityUserId = User.Identity.GetUserId();
            var currentUser = from d in db.MstUsers where d.UserId == identityUserId select d;
            var currentCompanyId = currentUser.FirstOrDefault().CompanyId;
            var currentBranchId = currentUser.FirstOrDefault().BranchId;

            // ==============
            // Company Detail
            // ==============
            var companyName = (from d in db.MstCompanies where d.Id == Convert.ToInt32(currentCompanyId) select d.Company).FirstOrDefault();
            var address = (from d in db.MstCompanies where d.Id == Convert.ToInt32(currentCompanyId) select d.Address).FirstOrDefault();
            var contactNo = (from d in db.MstCompanies where d.Id == Convert.ToInt32(currentCompanyId) select d.ContactNumber).FirstOrDefault();
            var branch = (from d in db.MstBranches where d.Id == Convert.ToInt32(currentBranchId) select d.Branch).FirstOrDefault();

            // ===========
            // Header Page
            // ===========
            PdfPTable headerPage = new PdfPTable(2);
            float[] widthsCellsHeaderPage = new float[] { 100f, 75f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;
            headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
            headerPage.AddCell(new PdfPCell(new Phrase("Purchase Request", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(address, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase(branch, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(contactNo, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            document.Add(headerPage);

            // =====
            // Space
            // =====
            PdfPTable spaceTable = new PdfPTable(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });

            document.Add(line);

            // ===================
            // Get Purchase Requests
            // ===================
            var purchaseRequests = from d in db.TrnPurchaseRequests
                                 where d.Id == Convert.ToInt32(PRId)
                                 && d.IsLocked == true
                                 select d;

            if (purchaseRequests.Any())
            {
                String supplier = purchaseRequests.FirstOrDefault().MstArticle.Article;
                String PRNumber = purchaseRequests.FirstOrDefault().PRNumber;
                String term = purchaseRequests.FirstOrDefault().MstTerm.Term;
                String PRDate = purchaseRequests.FirstOrDefault().PRDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String dateNeeded = purchaseRequests.FirstOrDefault().DateNeeded.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String remarks = purchaseRequests.FirstOrDefault().Remarks;
                String preparedBy = purchaseRequests.FirstOrDefault().MstUser3.FullName;
                String checkedBy = purchaseRequests.FirstOrDefault().MstUser1.FullName;
                String approvedBy = purchaseRequests.FirstOrDefault().MstUser.FullName;
                String requestedBy = purchaseRequests.FirstOrDefault().MstUser4.FullName;

                PdfPTable tablePurchaseRequest = new PdfPTable(4);
                float[] widthscellsTablePurchaseRequest = new float[] { 40f, 150f, 70f, 50f };
                tablePurchaseRequest.SetWidths(widthscellsTablePurchaseRequest);
                tablePurchaseRequest.WidthPercentage = 100;

                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase("Supplier", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase(supplier, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase("No.", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase(PRNumber, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase("Term", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase(term, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase("Date", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase(PRDate, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase("Date Needed", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase(dateNeeded, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase(" ", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase(" ", fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase("Remarks ", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase(remarks, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tablePurchaseRequest.AddCell(new PdfPCell(new Phrase(" ", fontArial11)) { Colspan = 2, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                document.Add(tablePurchaseRequest);

                document.Add(spaceTable);

                // ========================
                // Get Purchase Request Items
                // ========================
                var purchaseRequestItems = from d in db.TrnPurchaseRequestItems
                                         where d.PRId == PRId
                                         && d.TrnPurchaseRequest.IsLocked == true
                                         select new
                                         {
                                             PR = d.TrnPurchaseRequest.PRNumber,
                                             Item = d.MstArticle.Article,
                                             Particulars = d.Particulars,
                                             Unit = d.MstUnit.Unit,
                                             Quantity = d.Quantity,
                                             Cost = d.Cost,
                                             Amount = d.Amount
                                         };

                if (purchaseRequestItems.Any())
                {
                    PdfPTable tablePurchaseRequestLines = new PdfPTable(7);
                    float[] widthscellsPRLines = new float[] { 50f, 100f, 70f, 200f, 150f, 100f, 100f };
                    tablePurchaseRequestLines.SetWidths(widthscellsPRLines);
                    tablePurchaseRequestLines.WidthPercentage = 100;
                    tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase("No.", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Quantity", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Unit", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Item", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Particulars", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Price", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    Decimal totalAmount = 0;
                    Int32 count = 0;

                    foreach (var purchaseRequestItem in purchaseRequestItems)
                    {
                        count += 1;
                        totalAmount += purchaseRequestItem.Amount;

                        tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase(count.ToString("#,##0"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase(purchaseRequestItem.Quantity.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase(purchaseRequestItem.Unit, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase(purchaseRequestItem.Item, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase(purchaseRequestItem.Particulars, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase(purchaseRequestItem.Cost.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase(purchaseRequestItem.Amount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                    }

                    tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase("Total", fontArial11Bold)) { Colspan = 6, HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    tablePurchaseRequestLines.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tablePurchaseRequestLines);

                    document.Add(spaceTable);
                }

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
                tableUsers.AddCell(new PdfPCell(new Phrase("Requested by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 50f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 50f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 50f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 50f });
                tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(requestedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
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