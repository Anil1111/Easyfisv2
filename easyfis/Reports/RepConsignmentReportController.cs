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
    public class RepConsignmentReportController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =====================
        // Preview and Print PDF
        // =====================
        [Authorize]
        public ActionResult ConsignmentReport(String StartDate, String EndDate, String CompanyId, String BranchId, String SupplierId)
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
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);

            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 4.5F)));

            // ==============
            // Company Detail
            // ==============
            var companyName = (from d in db.MstCompanies where d.Id == Convert.ToInt32(CompanyId) select d.Company).FirstOrDefault();
            var branch = (from d in db.MstBranches where d.Id == Convert.ToInt32(BranchId) select d.Branch).FirstOrDefault();
            var address = (from d in db.MstCompanies where d.Id == Convert.ToInt32(CompanyId) select d.Address).FirstOrDefault();
            var contactNo = (from d in db.MstCompanies where d.Id == Convert.ToInt32(CompanyId) select d.ContactNumber).FirstOrDefault();
            var supplier = (from d in db.MstArticles where d.Id == Convert.ToInt32(SupplierId) select d.Article).FirstOrDefault();

            // ===========
            // Header Page
            // ===========
            PdfPTable headerPage = new PdfPTable(2);
            float[] widthsCellsHeaderPage = new float[] { 100f, 75f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;
            headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
            headerPage.AddCell(new PdfPCell(new Phrase("Consignment Items Report", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(address, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase("Date From " + Convert.ToDateTime(StartDate).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture) + " to " + Convert.ToDateTime(EndDate).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(contactNo, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            document.Add(headerPage);
            document.Add(line);

            // ========================
            // Sales Invoice Items Data
            // ========================
            var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                    where d.MstArticle.DefaultSupplierId == Convert.ToInt32(SupplierId)
                                    && d.TrnSalesInvoice.SIDate >= Convert.ToDateTime(StartDate)
                                    && d.TrnSalesInvoice.SIDate <= Convert.ToDateTime(EndDate)
                                    && d.TrnSalesInvoice.MstBranch.CompanyId == Convert.ToInt32(CompanyId)
                                    && d.TrnSalesInvoice.BranchId == Convert.ToInt32(BranchId)
                                    select new
                                    {
                                        ItemId = d.ItemId,
                                        ItemManualArticleOldCode = d.MstArticle.ManualArticleOldCode,
                                        ItemCode = d.MstArticle.ManualArticleCode,
                                        ItemDescription = d.MstArticle.Article,
                                        UnitId = d.MstArticle.UnitId,
                                        Unit = d.MstArticle.MstUnit.Unit,
                                        Quantity = d.Quantity,
                                        Amount = ((d.NetPrice * (d.MstArticle.ConsignmentCostPercentage / 100)) + d.MstArticle.ConsignmentCostValue) * d.Quantity
                                    };

            if (salesInvoiceItems.Any())
            {
                var consignmentItems = from d in salesInvoiceItems
                                       group d by new
                                       {
                                           ItemId = d.ItemId,
                                           ItemManualArticleOldCode = d.ItemManualArticleOldCode,
                                           ItemCode = d.ItemCode,
                                           ItemDescription = d.ItemDescription,
                                           UnitId = d.UnitId,
                                           Unit = d.Unit
                                       } into g
                                       select new
                                       {
                                           ItemId = g.Key.ItemId,
                                           ItemManualArticleOldCode = g.Key.ItemManualArticleOldCode,
                                           ItemCode = g.Key.ItemCode,
                                           ItemDescription = g.Key.ItemDescription,
                                           UnitId = g.Key.UnitId,
                                           Unit = g.Key.Unit,
                                           Quantity = g.Sum(d => d.Quantity),
                                           Cost = g.Sum(d => d.Amount) / g.Sum(d => d.Quantity),
                                           Amount = g.Sum(d => d.Amount)
                                       };

                if (consignmentItems.Any())
                {
                    // ============
                    // Branch Title
                    // ============
                    PdfPTable branchTitle = new PdfPTable(1);
                    float[] widthCellsBranchTitle = new float[] { 100f };
                    branchTitle.SetWidths(widthCellsBranchTitle);
                    branchTitle.WidthPercentage = 100;
                    PdfPCell branchHeaderColspan = (new PdfPCell(new Phrase(branch, fontArial12Bold)) { HorizontalAlignment = 0, PaddingTop = 10f, Border = 0 });
                    branchTitle.AddCell(branchHeaderColspan);
                    document.Add(branchTitle);

                    // ========
                    // Supplier
                    // ========
                    PdfPTable supplierTitle = new PdfPTable(1);
                    float[] widthCellsSupplierTitle = new float[] { 100f };
                    supplierTitle.SetWidths(widthCellsSupplierTitle);
                    supplierTitle.WidthPercentage = 100;
                    PdfPCell supplierTitleColspan = (new PdfPCell(new Phrase(supplier, fontArial12Bold)) { HorizontalAlignment = 0, PaddingTop = 5f, PaddingBottom = 14f, Border = 0 });
                    supplierTitle.AddCell(supplierTitleColspan);
                    document.Add(supplierTitle);

                    // ====
                    // Data
                    // ====
                    PdfPTable data = new PdfPTable(7);
                    float[] widthsCellsData = new float[] { 25f, 25f, 35f, 15f, 20f, 20f, 20f };
                    data.SetWidths(widthsCellsData);
                    data.WidthPercentage = 100;
                    data.AddCell(new PdfPCell(new Phrase("SKU", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Barcode", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Description", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Unit", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Quantity", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Cost", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });

                    Decimal total = 0;
                    foreach (var consignmentItem in consignmentItems)
                    {
                        data.AddCell(new PdfPCell(new Phrase(consignmentItem.ItemManualArticleOldCode, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingRight = 5f, PaddingLeft = 5f });
                        data.AddCell(new PdfPCell(new Phrase(consignmentItem.ItemCode, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingRight = 5f, PaddingLeft = 5f });
                        data.AddCell(new PdfPCell(new Phrase(consignmentItem.ItemDescription, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingRight = 5f, PaddingLeft = 5f });
                        data.AddCell(new PdfPCell(new Phrase(consignmentItem.Unit, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingRight = 5f, PaddingLeft = 5f });
                        data.AddCell(new PdfPCell(new Phrase(consignmentItem.Quantity.ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingRight = 5f, PaddingLeft = 5f });
                        data.AddCell(new PdfPCell(new Phrase(consignmentItem.Cost.ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingRight = 5f, PaddingLeft = 5f });
                        data.AddCell(new PdfPCell(new Phrase(consignmentItem.Amount.ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingRight = 5f, PaddingLeft = 5f });

                        total += consignmentItem.Amount;
                    }

                    // =====
                    // Total
                    // =====
                    data.AddCell(new PdfPCell(new Phrase("Total", fontArial10Bold)) { Colspan = 6, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingRight = 10f, PaddingLeft = 10f });
                    data.AddCell(new PdfPCell(new Phrase(total.ToString("#,##0.00"), fontArial10Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingRight = 5f, PaddingLeft = 5f });
                    document.Add(data);
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