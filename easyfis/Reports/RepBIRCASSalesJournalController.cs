﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace easyfis.Reports
{
    public class RepBIRCASSalesJournalController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ============================
        // Compute VATable Sales Amount
        // ============================
        public Decimal ComputeVATableSales(String taxType, Decimal amount)
        {
            Decimal VATableSalesAmount = 0;
            if (taxType.Equals("VAT Output"))
            {
                VATableSalesAmount = amount;
            }

            return VATableSalesAmount;
        }

        // ===============================
        // Compute VAT Exempt Sales Amount
        // ===============================
        public Decimal ComputeVATExemptSales(String taxType, Decimal taxRate, String discountType, Decimal price, Decimal quantity, Decimal amount)
        {
            Decimal VATExemptAmount = 0;
            if (taxType.Equals("VAT Exempt") && (discountType.Equals("Senior Citizen Discount") || discountType.Equals("PWD")))
            {
                VATExemptAmount = amount - ((taxRate / 100) * (price * quantity) / ((taxRate / 100) + 1));
            }

            return VATExemptAmount;
        }

        // ===============================
        // Compute Zero Rated Sales Amount
        // ===============================
        public Decimal ComputeZeroRatedSales(String taxType, Decimal amount)
        {
            Decimal zeroRatedAmount = 0;
            if (taxType.Equals("VAT Zero Rated"))
            {
                zeroRatedAmount = amount;
            }

            return 0;
        }

        // =====================
        // Preview and Print PDF
        // =====================
        [Authorize]
        public ActionResult BIRCASSalesJournal(String StartDate, String EndDate, String CompanyId, String BranchId, String DocumentReference)
        {
            // ==============================
            // PDF Settings and Customization
            // ==============================
            MemoryStream workStream = new MemoryStream();
            Rectangle rectangle = new Rectangle(PageSize.A3.Rotate());
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
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial12 = FontFactory.GetFont("Arial", 12);
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);

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
            header.AddCell(new PdfPCell(new Phrase("Sales Journal", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
            header.AddCell(new PdfPCell(new Phrase(company.FirstOrDefault().Address, fontArial10)) { Border = 0, PaddingTop = 5f });
            header.AddCell(new PdfPCell(new Phrase(branchName, fontArial10)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            header.AddCell(new PdfPCell(new Phrase(company.FirstOrDefault().ContactNumber, fontArial10)) { Border = 0, PaddingTop = 5f });
            header.AddCell(new PdfPCell(new Phrase("Date Printed: " + DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial10)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            header.AddCell(new PdfPCell(new Phrase("TIN: " + company.FirstOrDefault().TaxNumber, fontArial10)) { Border = 0, PaddingTop = 5f });
            header.AddCell(new PdfPCell(new Phrase("", fontArial10)) { Border = 0 });
            document.Add(header);
            document.Add(line);

            // ==================
            // Date Range Filters
            // ==================
            PdfPTable dateRangeFilters = new PdfPTable(1);
            dateRangeFilters.SetWidths(new float[] { 100f });
            dateRangeFilters.WidthPercentage = 100;
            dateRangeFilters.AddCell(new PdfPCell(new Phrase("Date Start:  " + Convert.ToDateTime(StartDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), fontArial10)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 8f });
            dateRangeFilters.AddCell(new PdfPCell(new Phrase("Date End:   " + Convert.ToDateTime(EndDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), fontArial10)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 5f });
            document.Add(dateRangeFilters);

            // =====
            // Space
            // =====
            PdfPTable space = new PdfPTable(1);
            space.SetWidths(new float[] { 100f });
            space.WidthPercentage = 100;
            space.AddCell(new PdfPCell(new Phrase(" ", fontArial10)) { Border = 0, PaddingTop = 7f });
            document.Add(space);

            // ====
            // Data
            // ====
            IQueryable<Data.TrnSalesInvoiceItem> salesInvoiceItems;
            if (Convert.ToInt32(BranchId) != 0)
            {
                salesInvoiceItems = from d in db.TrnSalesInvoiceItems.OrderBy(d => d.TrnSalesInvoice.SINumber)
                                    where d.TrnSalesInvoice.MstBranch.CompanyId == Convert.ToInt32(CompanyId)
                                    && d.TrnSalesInvoice.BranchId == Convert.ToInt32(BranchId)
                                    && d.TrnSalesInvoice.SIDate >= Convert.ToDateTime(StartDate)
                                    && d.TrnSalesInvoice.SIDate <= Convert.ToDateTime(EndDate)
                                    && (d.TrnSalesInvoice.DocumentReference.Contains(DocumentReference)
                                    || d.TrnSalesInvoice.ManualSINumber.Contains(DocumentReference))
                                    select d;
            }
            else
            {
                salesInvoiceItems = from d in db.TrnSalesInvoiceItems.OrderBy(d => d.TrnSalesInvoice.SINumber)
                                    where d.TrnSalesInvoice.MstBranch.CompanyId == Convert.ToInt32(CompanyId)
                                    && d.TrnSalesInvoice.SIDate >= Convert.ToDateTime(StartDate)
                                    && d.TrnSalesInvoice.SIDate <= Convert.ToDateTime(EndDate)
                                    && (d.TrnSalesInvoice.DocumentReference.Contains(DocumentReference)
                                    || d.TrnSalesInvoice.ManualSINumber.Contains(DocumentReference))
                                    select d;
            }

            if (salesInvoiceItems.Any())
            {
                var groupedBranches = from d in salesInvoiceItems
                                      group d by new
                                      {
                                          d.TrnSalesInvoice.BranchId,
                                          d.TrnSalesInvoice.MstBranch.Branch
                                      } into g
                                      select g;

                if (groupedBranches.Any())
                {
                    Decimal overAllTotalVATableSalesAmount = 0;
                    Decimal overAllTotalVATExemptSalesAmount = 0;
                    Decimal overAllTotalZeroRatedSalesAmount = 0;
                    Decimal overAllTotalVATAmount = 0;
                    Decimal overAllTotalDiscountAmount = 0;
                    Decimal overAllTotalAmount = 0;

                    foreach (var groupedBranch in groupedBranches)
                    {
                        PdfPTable branchTitle = new PdfPTable(1);
                        branchTitle.SetWidths(new float[] { 100f });
                        branchTitle.WidthPercentage = 100;
                        branchTitle.AddCell(new PdfPCell(new Phrase(groupedBranch.Key.Branch, fontArial12Bold)) { Border = 0, HorizontalAlignment = 0, PaddingBottom = 5f });
                        document.Add(branchTitle);

                        PdfPTable data = new PdfPTable(14);
                        data.SetWidths(new float[] { 40f, 65f, 60f, 50f, 50f, 55f, 55f, 55f, 50f, 50f, 50f, 50f, 50f, 50f });
                        data.WidthPercentage = 100;
                        data.AddCell(new PdfPCell(new Phrase("Date", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase("Tx No.", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase("Customer", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase("TIN", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase("Address", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase("Doc. Ref. No.", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase("SI Ref. No.", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase("Item Code", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase("VATable Sales", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase("VAT Exempt Sales", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase("Zero Rated Sales", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase("VAT", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase("Discount", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase("Amount", fontArial10Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });

                        var salesInvoiceItemPerBranch = from d in salesInvoiceItems
                                                        where d.TrnSalesInvoice.BranchId == groupedBranch.Key.BranchId
                                                        select d;

                        if (salesInvoiceItemPerBranch.Any())
                        {
                            Decimal totalVATableSalesAmount = 0;
                            Decimal totalVATExemptSalesAmount = 0;
                            Decimal totalZeroRatedSalesAmount = 0;
                            Decimal totalVATAmount = 0;
                            Decimal totalDiscountAmount = 0;
                            Decimal totalAmount = 0;

                            foreach (var salesInvoiceItem in salesInvoiceItemPerBranch)
                            {
                                totalVATableSalesAmount += ComputeVATableSales(salesInvoiceItem.MstTaxType.TaxType, salesInvoiceItem.Amount);
                                totalVATExemptSalesAmount += ComputeVATExemptSales(salesInvoiceItem.MstTaxType.TaxType, salesInvoiceItem.MstTaxType.TaxRate, salesInvoiceItem.MstDiscount.Discount, salesInvoiceItem.Price, salesInvoiceItem.Quantity, salesInvoiceItem.Amount);
                                totalZeroRatedSalesAmount += ComputeZeroRatedSales(salesInvoiceItem.MstTaxType.TaxType, salesInvoiceItem.Amount);
                                totalVATAmount += salesInvoiceItem.VATAmount;
                                totalDiscountAmount += salesInvoiceItem.DiscountAmount;
                                totalAmount += salesInvoiceItem.Amount;

                                data.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.TrnSalesInvoice.SIDate.ToShortDateString(), fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                                data.AddCell(new PdfPCell(new Phrase("SI-" + salesInvoiceItem.TrnSalesInvoice.MstBranch.BranchCode + "-" + salesInvoiceItem.TrnSalesInvoice.SINumber, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.TrnSalesInvoice.MstArticle.Article, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.TrnSalesInvoice.MstArticle.TaxNumber, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.TrnSalesInvoice.MstArticle.Address, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.TrnSalesInvoice.DocumentReference, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.TrnSalesInvoice.ManualSINumber, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.MstArticle.ManualArticleCode, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                                data.AddCell(new PdfPCell(new Phrase(ComputeVATableSales(salesInvoiceItem.MstTaxType.TaxType, salesInvoiceItem.Amount).ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                                data.AddCell(new PdfPCell(new Phrase(ComputeVATExemptSales(salesInvoiceItem.MstTaxType.TaxType, salesInvoiceItem.MstTaxType.TaxRate, salesInvoiceItem.MstDiscount.Discount, salesInvoiceItem.Price, salesInvoiceItem.Quantity, salesInvoiceItem.Amount).ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                                data.AddCell(new PdfPCell(new Phrase(ComputeZeroRatedSales(salesInvoiceItem.MstTaxType.TaxType, salesInvoiceItem.Amount).ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.VATAmount.ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.DiscountAmount.ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                                data.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.Amount.ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                            }

                            data.AddCell(new PdfPCell(new Phrase("TOTAL", fontArial10Bold)) { Colspan = 8, HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                            data.AddCell(new PdfPCell(new Phrase(totalVATableSalesAmount.ToString("#,##0.00"), fontArial10Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                            data.AddCell(new PdfPCell(new Phrase(totalVATExemptSalesAmount.ToString("#,##0.00"), fontArial10Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                            data.AddCell(new PdfPCell(new Phrase(totalZeroRatedSalesAmount.ToString("#,##0.00"), fontArial10Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                            data.AddCell(new PdfPCell(new Phrase(totalVATAmount.ToString("#,##0.00"), fontArial10Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                            data.AddCell(new PdfPCell(new Phrase(totalDiscountAmount.ToString("#,##0.00"), fontArial10Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                            data.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial10Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });

                            document.Add(data);

                            overAllTotalVATableSalesAmount += totalVATableSalesAmount;
                            overAllTotalVATExemptSalesAmount += totalVATExemptSalesAmount;
                            overAllTotalZeroRatedSalesAmount += totalZeroRatedSalesAmount;
                            overAllTotalVATAmount += totalVATAmount;
                            overAllTotalDiscountAmount += totalDiscountAmount;
                            overAllTotalAmount += totalAmount;
                        }

                        document.Add(space);
                    }

                    PdfPTable overallTotalData = new PdfPTable(14);
                    overallTotalData.SetWidths(new float[] { 40f, 65f, 60f, 50f, 50f, 55f, 55f, 55f, 50f, 50f, 50f, 50f, 50f, 50f });
                    overallTotalData.WidthPercentage = 100;
                    overallTotalData.AddCell(new PdfPCell(new Phrase("OVERALL TOTAL", fontArial10Bold)) { Colspan = 8, HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                    overallTotalData.AddCell(new PdfPCell(new Phrase(overAllTotalVATableSalesAmount.ToString("#,##0.00"), fontArial10Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                    overallTotalData.AddCell(new PdfPCell(new Phrase(overAllTotalVATExemptSalesAmount.ToString("#,##0.00"), fontArial10Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                    overallTotalData.AddCell(new PdfPCell(new Phrase(overAllTotalZeroRatedSalesAmount.ToString("#,##0.00"), fontArial10Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                    overallTotalData.AddCell(new PdfPCell(new Phrase(overAllTotalVATAmount.ToString("#,##0.00"), fontArial10Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                    overallTotalData.AddCell(new PdfPCell(new Phrase(overAllTotalDiscountAmount.ToString("#,##0.00"), fontArial10Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                    overallTotalData.AddCell(new PdfPCell(new Phrase(overAllTotalAmount.ToString("#,##0.00"), fontArial10Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });

                    document.Add(overallTotalData);
                }
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