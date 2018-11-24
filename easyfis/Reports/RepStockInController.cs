using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace easyfis.Views.Software
{
    public class RepStockInController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==============
        // Stock In - PDF
        // ==============
        [Authorize]
        public ActionResult StockIn(Int32 StockInId)
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

            var stockIn = from d in db.TrnStockIns where d.Id == StockInId && d.IsLocked == true select d;
            if (stockIn.Any())
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
                if (stockIn.FirstOrDefault().IsPrinted)
                {
                    reprinted = "Reprinted";
                }

                PdfPTable headerPage = new PdfPTable(2);
                headerPage.SetWidths(new float[] { 100f, 75f });
                headerPage.WidthPercentage = 100;
                headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
                headerPage.AddCell(new PdfPCell(new Phrase("Stock In", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyTaxNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(branchName, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyAddress, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyContactNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(reprinted, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });

                document.Add(headerPage);
                document.Add(line);

                String account = stockIn.FirstOrDefault().MstAccount.Account;
                String particulars = stockIn.FirstOrDefault().Particulars;
                String INNumber = stockIn.FirstOrDefault().INNumber;
                String INDate = stockIn.FirstOrDefault().INDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String preparedBy = stockIn.FirstOrDefault().MstUser3.FullName;
                String checkedBy = stockIn.FirstOrDefault().MstUser1.FullName;
                String approvedBy = stockIn.FirstOrDefault().MstUser.FullName;

                PdfPTable tblStockIn = new PdfPTable(4);
                tblStockIn.SetWidths(new float[] { 40f, 150f, 70f, 70f });
                tblStockIn.WidthPercentage = 100;
                tblStockIn.AddCell(new PdfPCell(new Phrase("Account:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblStockIn.AddCell(new PdfPCell(new Phrase(account, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblStockIn.AddCell(new PdfPCell(new Phrase("No.:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockIn.AddCell(new PdfPCell(new Phrase("IN-" + stockIn.FirstOrDefault().MstBranch.BranchCode + "-" + INNumber, fontArial13Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockIn.AddCell(new PdfPCell(new Phrase("Particulars:", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblStockIn.AddCell(new PdfPCell(new Phrase(particulars, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblStockIn.AddCell(new PdfPCell(new Phrase("Date:", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockIn.AddCell(new PdfPCell(new Phrase(INDate, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                document.Add(tblStockIn);

                PdfPTable spaceTable = new PdfPTable(1);
                spaceTable.SetWidths(new float[] { 100f });
                spaceTable.WidthPercentage = 100;
                spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });

                document.Add(spaceTable);

                var stockInItems = from d in stockIn.FirstOrDefault().TrnStockInItems select d;
                if (stockInItems.Any())
                {
                    var numberOfTableColumns = 4;
                    float[] widthscellsPOLines = new float[] { 100f, 70f, 150f, 200f };

                    if (currentIsIncludeCostStockReports)
                    {
                        numberOfTableColumns = 6;
                        widthscellsPOLines = new float[] { 100f, 70f, 150f, 200f, 100f, 100f };
                    }

                    PdfPTable tblStockInItems = new PdfPTable(numberOfTableColumns);
                    tblStockInItems.SetWidths(widthscellsPOLines);
                    tblStockInItems.WidthPercentage = 100;
                    tblStockInItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblStockInItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblStockInItems.AddCell(new PdfPCell(new Phrase("Code", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblStockInItems.AddCell(new PdfPCell(new Phrase("Item", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    if (currentIsIncludeCostStockReports)
                    {
                        tblStockInItems.AddCell(new PdfPCell(new Phrase("Cost", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                        tblStockInItems.AddCell(new PdfPCell(new Phrase("Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    }

                    Decimal totalAmount = 0;

                    foreach (var stockInItem in stockInItems)
                    {
                        string SKUCode = stockInItem.MstArticle.ManualArticleOldCode;
                        if (stockInItem.MstArticle.ManualArticleOldCode.Equals("NA") || stockInItem.MstArticle.ManualArticleOldCode.Equals("na"))
                        {
                            SKUCode = " ";
                        }

                        tblStockInItems.AddCell(new PdfPCell(new Phrase(stockInItem.Quantity.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblStockInItems.AddCell(new PdfPCell(new Phrase(stockInItem.MstUnit.Unit, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblStockInItems.AddCell(new PdfPCell(new Phrase(stockInItem.MstArticle.ArticleCode, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblStockInItems.AddCell(new PdfPCell(new Phrase(stockInItem.MstArticle.Article + " " + SKUCode, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });

                        if (currentIsIncludeCostStockReports)
                        {
                            tblStockInItems.AddCell(new PdfPCell(new Phrase(stockInItem.Cost.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                            tblStockInItems.AddCell(new PdfPCell(new Phrase(stockInItem.Amount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        }

                        totalAmount += stockInItem.Amount;
                    }

                    if (currentIsIncludeCostStockReports)
                    {
                        tblStockInItems.AddCell(new PdfPCell(new Phrase("Total", fontArial11Bold)) { Colspan = 5, HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblStockInItems.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
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

                if (!stockIn.FirstOrDefault().IsPrinted)
                {
                    stockIn.FirstOrDefault().IsPrinted = true;
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