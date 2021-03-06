﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace easyfis.Reports
{
    public class RepBIRCASPurchaseJournalController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =====================
        // Preview and Print PDF
        // =====================
        [Authorize]
        public ActionResult BIRCASPurchaseJournal(String StartDate, String EndDate, String CompanyId, String BranchId)
        {
            // ==============================
            // PDF Settings and Customization
            // ==============================
            MemoryStream workStream = new MemoryStream();
            Rectangle rectangle = new Rectangle(PageSize.A3);
            Document document = new Document(rectangle, 72, 72, 72, 72);
            document.SetMargins(30f, 30f, 30f, 30f);
            PdfWriter.GetInstance(document, workStream).CloseStream = false;

            // =============
            // Document Open
            // =============
            document.Open();

            // ============
            // Fonts Styles
            // ============
            Font fontArial17Bold = FontFactory.GetFont("Arial", 17, Font.BOLD);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);
            Font fontArial11 = FontFactory.GetFont("Arial", 11);

            // ====
            // Line 
            // ====
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 4.5F)));

            // ==============
            // Company Detail
            // ==============
            var company = from d in db.MstCompanies where d.Id == Convert.ToInt32(CompanyId) select d;

            var branchName = "All Branches";
            if (Convert.ToInt32(BranchId) != 0)
            {
                var branch = from d in db.MstBranches where d.Id == Convert.ToInt32(BranchId) select d;
                branchName = branch.FirstOrDefault().Branch;
            }

            // ===========
            // Header Page
            // ===========
            PdfPTable header = new PdfPTable(2);
            header.SetWidths(new float[] { 100f, 75f });
            header.WidthPercentage = 100;
            header.AddCell(new PdfPCell(new Phrase(company.FirstOrDefault().Company, fontArial17Bold)) { Border = 0 });
            header.AddCell(new PdfPCell(new Phrase("Purchase Journal", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
            header.AddCell(new PdfPCell(new Phrase(company.FirstOrDefault().Address, fontArial11)) { Border = 0, PaddingTop = 5f });
            header.AddCell(new PdfPCell(new Phrase(branchName, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            header.AddCell(new PdfPCell(new Phrase(company.FirstOrDefault().ContactNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
            header.AddCell(new PdfPCell(new Phrase("Date Printed: " + DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            header.AddCell(new PdfPCell(new Phrase("TIN: " + company.FirstOrDefault().TaxNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
            header.AddCell(new PdfPCell(new Phrase("", fontArial11)) { Border = 0 });
            document.Add(header);
            document.Add(line);

            // ==================
            // Date Range Filters
            // ==================
            PdfPTable dateRangeFilters = new PdfPTable(1);
            dateRangeFilters.SetWidths(new float[] { 100f });
            dateRangeFilters.WidthPercentage = 100;
            dateRangeFilters.AddCell(new PdfPCell(new Phrase("Date Start:  " + Convert.ToDateTime(StartDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), fontArial11)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 8f });
            dateRangeFilters.AddCell(new PdfPCell(new Phrase("Date End:   " + Convert.ToDateTime(EndDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), fontArial11)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 5f });
            document.Add(dateRangeFilters);

            // =====
            // Space
            // =====
            PdfPTable space = new PdfPTable(1);
            space.SetWidths(new float[] { 100f });
            space.WidthPercentage = 100;
            space.AddCell(new PdfPCell(new Phrase(" ", fontArial11)) { Border = 0, PaddingTop = 7f });
            document.Add(space);

            // ====
            // Data
            // ====
            IQueryable<Data.TrnReceivingReceiptItem> receivingReceiptItems;
            if (Convert.ToInt32(BranchId) != 0)
            {
                receivingReceiptItems = from d in db.TrnReceivingReceiptItems
                                        where d.TrnReceivingReceipt.MstBranch.CompanyId == Convert.ToInt32(CompanyId)
                                        && d.TrnReceivingReceipt.BranchId == Convert.ToInt32(BranchId)
                                        && d.TrnReceivingReceipt.RRDate >= Convert.ToDateTime(StartDate)
                                        && d.TrnReceivingReceipt.RRDate <= Convert.ToDateTime(EndDate)
                                        select d;
            }
            else
            {
                receivingReceiptItems = from d in db.TrnReceivingReceiptItems
                                        where d.TrnReceivingReceipt.MstBranch.CompanyId == Convert.ToInt32(CompanyId)
                                        && d.TrnReceivingReceipt.RRDate >= Convert.ToDateTime(StartDate)
                                        && d.TrnReceivingReceipt.RRDate <= Convert.ToDateTime(EndDate)
                                        select d;
            }

            if (receivingReceiptItems.Any())
            {
                PdfPTable data = new PdfPTable(10);
                data.SetWidths(new float[] { 50f, 65f, 60f, 50f, 50f, 55f, 50f, 50f, 50f, 50f });
                data.WidthPercentage = 100;
                data.AddCell(new PdfPCell(new Phrase("Date", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Tx No.", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Supplier", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Supplier TIN", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Address", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Doc. Ref. No.", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Total Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Discount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("VAT", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Net Purchase", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });

                Decimal totalAmount = 0;
                Decimal totalDiscount = 0;
                Decimal totalVAT = 0;
                Decimal totalNetPurchase = 0;

                foreach (var receivingReceiptItem in receivingReceiptItems)
                {
                    totalAmount += receivingReceiptItem.Amount;
                    totalDiscount += 0;
                    totalVAT += receivingReceiptItem.VATAmount;
                    totalNetPurchase += receivingReceiptItem.Amount;

                    data.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.TrnReceivingReceipt.RRDate.ToShortDateString(), fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase("RR-" + receivingReceiptItem.TrnReceivingReceipt.MstBranch.BranchCode + "-" + receivingReceiptItem.TrnReceivingReceipt.RRNumber, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.TrnReceivingReceipt.MstArticle.Article, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.TrnReceivingReceipt.MstArticle.TaxNumber, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.TrnReceivingReceipt.MstArticle.Address, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.TrnReceivingReceipt.ManualRRNumber, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.Amount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase("0.00", fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.VATAmount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(receivingReceiptItem.Amount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                }

                data.AddCell(new PdfPCell(new Phrase("TOTAL", fontArial11Bold)) { Colspan = 6, HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                data.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                data.AddCell(new PdfPCell(new Phrase(totalDiscount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                data.AddCell(new PdfPCell(new Phrase(totalVAT.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                data.AddCell(new PdfPCell(new Phrase(totalNetPurchase.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });

                document.Add(data);
            }

            // ==============
            // Document Close
            // ==============
            document.Close();

            // ================
            // Byte File Stream
            // ================
            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}