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
    public class RepArticlePriceController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===================
        // Article Price - PDF
        // ===================
        [Authorize]
        public ActionResult ArticlePrice(Int32 articlePriceId)
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
            headerPage.AddCell(new PdfPCell(new Phrase("Item Price", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
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

            // ==================
            // Get Article Prices
            // ==================
            var articlePrices = from d in db.TrnArticlePrices
                                where d.Id == articlePriceId
                                select d;

            if (articlePrices.Any())
            {
                String particulars = articlePrices.FirstOrDefault().Particulars;
                String IPNumber = articlePrices.FirstOrDefault().IPNumber;
                String IPDate = articlePrices.FirstOrDefault().IPDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String preparedBy = articlePrices.FirstOrDefault().MstUser.FullName;
                String checkedBy = articlePrices.FirstOrDefault().MstUser1.FullName;
                String approvedBy = articlePrices.FirstOrDefault().MstUser2.FullName;

                PdfPTable tableArticlePrices = new PdfPTable(4);
                float[] widthscellsTablePurchaseOrder = new float[] { 40f, 150f, 70f, 50f };
                tableArticlePrices.SetWidths(widthscellsTablePurchaseOrder);
                tableArticlePrices.WidthPercentage = 100;

                tableArticlePrices.AddCell(new PdfPCell(new Phrase("Particulars", fontArial11Bold)) { Rowspan = 2, Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tableArticlePrices.AddCell(new PdfPCell(new Phrase(particulars, fontArial11)) { Rowspan = 2, Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tableArticlePrices.AddCell(new PdfPCell(new Phrase("No.", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tableArticlePrices.AddCell(new PdfPCell(new Phrase(IPNumber, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tableArticlePrices.AddCell(new PdfPCell(new Phrase("Date", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tableArticlePrices.AddCell(new PdfPCell(new Phrase(IPDate, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                document.Add(tableArticlePrices);

                document.Add(spaceTable);

                // =======================
                // Get Article Price Items
                // =======================
                var articlePriceItems = from d in db.TrnArticlePriceItems
                                        where d.ArticlePriceId == articlePriceId
                                        select new
                                        {
                                            ItemCode = d.MstArticle.ManualArticleCode,
                                            ItemDescription = d.MstArticle.Article,
                                            d.Price,
                                            d.TriggerQuantity
                                        };

                if (articlePriceItems.Any())
                {
                    PdfPTable tableArticlePriceItems = new PdfPTable(4);
                    float[] widthscellsPOLines = new float[] { 50f, 100f, 40f, 40f };
                    tableArticlePriceItems.SetWidths(widthscellsPOLines);
                    tableArticlePriceItems.WidthPercentage = 100;
                    tableArticlePriceItems.AddCell(new PdfPCell(new Phrase("Code", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tableArticlePriceItems.AddCell(new PdfPCell(new Phrase("Description", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tableArticlePriceItems.AddCell(new PdfPCell(new Phrase("Price", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tableArticlePriceItems.AddCell(new PdfPCell(new Phrase("Trigger Qty.", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    foreach (var articlePriceItem in articlePriceItems)
                    {
                        tableArticlePriceItems.AddCell(new PdfPCell(new Phrase(articlePriceItem.ItemCode, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableArticlePriceItems.AddCell(new PdfPCell(new Phrase(articlePriceItem.ItemDescription, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableArticlePriceItems.AddCell(new PdfPCell(new Phrase(articlePriceItem.Price.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableArticlePriceItems.AddCell(new PdfPCell(new Phrase(articlePriceItem.TriggerQuantity.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                    }

                    document.Add(tableArticlePriceItems);
                    document.Add(spaceTable);
                }

                // ==============
                // User Signature
                // ==============
                PdfPTable tableUsers = new PdfPTable(3);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f };
                tableUsers.WidthPercentage = 100;
                tableUsers.SetWidths(widthsCellsTableUsers);
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 50f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 50f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 50f });
                tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                document.Add(tableUsers);
            }

            // ============
            // Document End
            // ============
            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}