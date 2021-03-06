﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace easyfis.Reports
{
    public class RepBIRCASAuditTrailController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =====================
        // Preview and Print PDF
        // =====================
        [Authorize]
        public ActionResult BIRCASAuditTrail(String StartDate, String EndDate, String CompanyId, String BranchId)
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
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial12 = FontFactory.GetFont("Arial", 12);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);
            Font fontArial11 = FontFactory.GetFont("Arial", 11);
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
            header.AddCell(new PdfPCell(new Phrase("Audit Trail", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
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
            var auditTrails = from d in db.SysAuditTrails.OrderByDescending(d => d.AuditDate)
                              where d.AuditDate >= Convert.ToDateTime(StartDate)
                              && d.AuditDate <= Convert.ToDateTime(EndDate).AddHours(24)
                              select d;

            if (auditTrails.Any())
            {
                PdfPTable data = new PdfPTable(6);
                data.SetWidths(new float[] { 60f, 70f, 100f, 100f, 130f, 130f });
                data.WidthPercentage = 100;
                data.AddCell(new PdfPCell(new Phrase("Time Stamp", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("User", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Page", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Activity", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("Old Object", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase("New Object", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 8f, PaddingLeft = 5f, PaddingRight = 5f });

                foreach (var auditTrail in auditTrails)
                {
                    data.AddCell(new PdfPCell(new Phrase(auditTrail.AuditDate.ToString("MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture), fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(auditTrail.MstUser.FullName, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(auditTrail.Entity, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(auditTrail.Activity, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(auditTrail.OldObject, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(auditTrail.NewObject, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                }

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