using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace easyfis.Reports
{
    public class RepBIRCASInventoryJournalController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =====================
        // Preview and Print PDF
        // =====================
        [Authorize]
        public ActionResult BIRCASInventoryJournal(String StartDate, String EndDate, String CompanyId, String BranchId)
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
            header.AddCell(new PdfPCell(new Phrase("Inventory Journal", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
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
            IQueryable<Data.TrnInventory> inventories;
            if (Convert.ToInt32(BranchId) != 0)
            {
                inventories = from d in db.TrnInventories
                              where d.MstBranch.CompanyId == Convert.ToInt32(CompanyId)
                              && d.BranchId == Convert.ToInt32(BranchId)
                              && d.InventoryDate >= Convert.ToDateTime(StartDate)
                              && d.InventoryDate <= Convert.ToDateTime(EndDate)
                              select d;

            }
            else
            {
                inventories = from d in db.TrnInventories
                              where d.MstBranch.CompanyId == Convert.ToInt32(CompanyId)
                              && d.InventoryDate >= Convert.ToDateTime(StartDate)
                              && d.InventoryDate <= Convert.ToDateTime(EndDate)
                              select d;
            }

            if (inventories.Any())
            {
                PdfPTable data = new PdfPTable(7);
                data.SetWidths(new float[] { 50f, 70f, 70f, 100f, 130f, 70f, 70f });
                data.WidthPercentage = 100;
                data.AddCell(new PdfPCell(new Phrase("Date", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Tx No.", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Account Code", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Account", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Item", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Debit Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Credit Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });

                Decimal totalDebitAmount = 0;
                Decimal totalCreditAmount = 0;

                foreach (var inventory in inventories)
                {
                    String referenceNumber = "0000000000";

                    if (inventory.RRId != null)
                    {
                        referenceNumber = "RR-" + inventory.TrnReceivingReceipt.MstBranch.BranchCode + "-" + inventory.TrnReceivingReceipt.RRNumber;
                    }
                    else if (inventory.SIId != null)
                    {
                        referenceNumber = "SI-" + inventory.TrnSalesInvoice.MstBranch.BranchCode + "-" + inventory.TrnSalesInvoice.SINumber;
                    }
                    else if (inventory.INId != null)
                    {
                        referenceNumber = "IN-" + inventory.TrnStockIn.MstBranch.BranchCode + "-" + inventory.TrnStockIn.INNumber;
                    }
                    else if (inventory.OTId != null)
                    {
                        referenceNumber = "OT-" + inventory.TrnStockOut.MstBranch.BranchCode + "-" + inventory.TrnStockOut.OTNumber;
                    }
                    else if (inventory.STId != null)
                    {
                        referenceNumber = "ST-" + inventory.TrnStockTransfer.MstBranch.BranchCode + "-" + inventory.TrnStockTransfer.STNumber;
                    }
                    else if (inventory.SWId != null)
                    {
                        referenceNumber = "SW-" + inventory.TrnStockWithdrawal.MstBranch.BranchCode + "-" + inventory.TrnStockWithdrawal.SWNumber;
                    }
                    else
                    {
                        referenceNumber = "??-00000-0000000000";
                    }

                    Decimal debitAmount = 0;
                    Decimal creditAmount = 0;

                    if (inventory.Quantity > 0)
                    {
                        debitAmount = inventory.Amount;
                    }
                    else
                    {
                        creditAmount = inventory.Amount;
                    }

                    totalDebitAmount += debitAmount;
                    totalCreditAmount += creditAmount;

                    data.AddCell(new PdfPCell(new Phrase(inventory.InventoryDate.ToShortDateString(), fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(referenceNumber, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(inventory.MstArticle.MstAccount.AccountCode, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(inventory.MstArticle.MstAccount.Account, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(inventory.MstArticle.Article, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(debitAmount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(creditAmount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                }

                data.AddCell(new PdfPCell(new Phrase("TOTAL", fontArial11Bold)) { Colspan = 5, HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                data.AddCell(new PdfPCell(new Phrase(totalDebitAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });
                data.AddCell(new PdfPCell(new Phrase(totalCreditAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 4f, PaddingBottom = 8f, PaddingRight = 5f, PaddingLeft = 5f });

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