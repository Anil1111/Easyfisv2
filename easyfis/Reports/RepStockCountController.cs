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
    public class RepStockCountController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =================
        // Stock Count - PDF
        // =================
        [Authorize]
        public ActionResult StockCount(Int32 StockCountId)
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

            var stockCount = from d in db.TrnStockCounts where d.Id == StockCountId && d.IsLocked == true select d;
            if (stockCount.Any())
            {
                var identityUserId = User.Identity.GetUserId();
                var currentUser = from d in db.MstUsers where d.UserId == identityUserId select d;

                var currentCompanyId = currentUser.FirstOrDefault().CompanyId;
                var currentBranchId = currentUser.FirstOrDefault().BranchId;
                var currentIsIncludeCostStockReports = currentUser.FirstOrDefault().IsIncludeCostStockReports;

                var currentCompany = from d in db.MstCompanies where d.Id == Convert.ToInt32(currentCompanyId) select d;
                var currentBranch = from d in db.MstBranches where d.Id == Convert.ToInt32(currentBranchId) select d;

                String companyName = currentCompany.FirstOrDefault().Company;
                String companyTaxNumber = currentCompany.FirstOrDefault().TaxNumber;
                String companyAddress = currentCompany.FirstOrDefault().Address;
                String companyContactNumber = currentCompany.FirstOrDefault().ContactNumber;
                String branchName = currentBranch.FirstOrDefault().Branch;
                String branchCode = currentBranch.FirstOrDefault().BranchCode;

                String reprinted = "";
                if (stockCount.FirstOrDefault().IsPrinted)
                {
                    reprinted = "Reprinted";
                }

                PdfPTable headerPage = new PdfPTable(2);
                headerPage.SetWidths(new float[] { 100f, 75f });
                headerPage.WidthPercentage = 100;
                headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
                headerPage.AddCell(new PdfPCell(new Phrase("Stock Count", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyTaxNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(branchName, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyAddress, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyContactNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(reprinted, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });

                document.Add(headerPage);
                document.Add(line);

                String particulars = stockCount.FirstOrDefault().Particulars;
                String SCNumber = stockCount.FirstOrDefault().SCNumber;
                String SCDate = stockCount.FirstOrDefault().SCDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String preparedBy = stockCount.FirstOrDefault().MstUser3.FullName;
                String checkedBy = stockCount.FirstOrDefault().MstUser1.FullName;
                String approvedBy = stockCount.FirstOrDefault().MstUser.FullName;

                PdfPTable tblStockCount = new PdfPTable(4);
                tblStockCount.SetWidths(new float[] { 40f, 150f, 70f, 50f });
                tblStockCount.WidthPercentage = 100;
                tblStockCount.AddCell(new PdfPCell(new Phrase("Particulars", fontArial11Bold)) { Rowspan = 2, Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblStockCount.AddCell(new PdfPCell(new Phrase(particulars, fontArial11)) { Rowspan = 2, Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblStockCount.AddCell(new PdfPCell(new Phrase("No.", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockCount.AddCell(new PdfPCell(new Phrase(SCNumber, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockCount.AddCell(new PdfPCell(new Phrase("Date", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockCount.AddCell(new PdfPCell(new Phrase(SCDate, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                document.Add(tblStockCount);

                PdfPTable spaceTable = new PdfPTable(1);
                spaceTable.SetWidths(new float[] { 100f });
                spaceTable.WidthPercentage = 100;
                spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });

                document.Add(spaceTable);

                var stockCountItems = from d in stockCount.FirstOrDefault().TrnStockCountItems select d;
                if (stockCountItems.Any())
                {
                    var numberOfTableColumns = 4;
                    var widthscellsPOLines = new float[] { 100f, 70f, 150f, 200f };

                    PdfPTable tblStockInItems = new PdfPTable(numberOfTableColumns);
                    tblStockInItems.SetWidths(widthscellsPOLines);
                    tblStockInItems.WidthPercentage = 100;
                    tblStockInItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblStockInItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblStockInItems.AddCell(new PdfPCell(new Phrase("Code", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblStockInItems.AddCell(new PdfPCell(new Phrase("Item", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    foreach (var stockCountItem in stockCountItems)
                    {
                        tblStockInItems.AddCell(new PdfPCell(new Phrase(stockCountItem.Quantity.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblStockInItems.AddCell(new PdfPCell(new Phrase(stockCountItem.MstArticle.MstUnit.Unit, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblStockInItems.AddCell(new PdfPCell(new Phrase(stockCountItem.MstArticle.ArticleCode, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblStockInItems.AddCell(new PdfPCell(new Phrase(stockCountItem.MstArticle.Article, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                    }

                    document.Add(tblStockInItems);
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

                if (!stockCount.FirstOrDefault().IsPrinted)
                {
                    stockCount.FirstOrDefault().IsPrinted = true;
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