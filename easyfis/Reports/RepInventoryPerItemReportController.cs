﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;


namespace easyfis.Reports
{
    public class RepInventoryPerItemReportController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ====================
        // Inventory Report PDF
        // ====================
        [Authorize]
        public ActionResult InventoryPerItemReport(Int32 CompanyId, Int32 ItemId)
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

            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 4.5F)));

            // ==============
            // Company Detail
            // ==============
            var companyName = (from d in db.MstCompanies where d.Id == Convert.ToInt32(CompanyId) select d.Company).FirstOrDefault();
            var address = (from d in db.MstCompanies where d.Id == Convert.ToInt32(CompanyId) select d.Address).FirstOrDefault();
            var contactNo = (from d in db.MstCompanies where d.Id == Convert.ToInt32(CompanyId) select d.ContactNumber).FirstOrDefault();

            // ===========
            // Header Page
            // ===========
            PdfPTable headerPage = new PdfPTable(2);
            float[] widthsCellsHeaderPage = new float[] { 100f, 75f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;
            headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
            headerPage.AddCell(new PdfPCell(new Phrase("Inventory Report Per Item", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(address, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase(" ", fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(contactNo, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            document.Add(headerPage);
            document.Add(line);

            // ====
            // Data
            // ====

            var unionInventories = (from d in db.TrnInventories
                                    where d.MstArticleInventory.ArticleId == Convert.ToInt32(ItemId)
                                    && d.MstArticleInventory.MstBranch.CompanyId == Convert.ToInt32(CompanyId)
                                    && d.MstArticleInventory.MstArticle.IsInventory == true
                                    select new Models.MstArticleInventory
                                    {
                                        Id = d.Id,
                                        Document = "Beginning Balance",
                                        BranchId = d.BranchId,
                                        Branch = d.MstBranch.Branch,
                                        InventoryCode = d.MstArticleInventory.InventoryCode,
                                        Quantity = d.MstArticleInventory.Quantity,
                                        Cost = d.MstArticleInventory.Cost,
                                        Amount = d.MstArticleInventory.Amount,
                                        UnitId = d.MstArticleInventory.MstArticle.MstUnit.Id,
                                        Unit = d.MstArticleInventory.MstArticle.MstUnit.Unit,
                                        BegQuantity = d.Quantity,
                                        InQuantity = d.QuantityIn,
                                        OutQuantity = d.QuantityOut,
                                        EndQuantity = d.Quantity,
                                        Category = d.MstArticle.Category,
                                        Price = d.MstArticle.Price
                                    }).Union(from d in db.TrnInventories
                                             where d.MstArticleInventory.ArticleId == Convert.ToInt32(ItemId)
                                             && d.MstArticleInventory.MstBranch.CompanyId == Convert.ToInt32(CompanyId)
                                             && d.MstArticleInventory.MstArticle.IsInventory == true
                                             select new Models.MstArticleInventory
                                             {
                                                 Id = d.Id,
                                                 Document = "Current",
                                                 BranchId = d.BranchId,
                                                 Branch = d.MstBranch.Branch,
                                                 InventoryCode = d.MstArticleInventory.InventoryCode,
                                                 Quantity = d.MstArticleInventory.Quantity,
                                                 Cost = d.MstArticleInventory.Cost,
                                                 Amount = d.MstArticleInventory.Amount,
                                                 UnitId = d.MstArticleInventory.MstArticle.MstUnit.Id,
                                                 Unit = d.MstArticleInventory.MstArticle.MstUnit.Unit,
                                                 BegQuantity = d.Quantity,
                                                 InQuantity = d.QuantityIn,
                                                 OutQuantity = d.QuantityOut,
                                                 EndQuantity = d.Quantity,
                                                 Category = d.MstArticle.Category,
                                                 Price = d.MstArticle.Price
                                             });

            if (unionInventories.Any())
            {
                var inventories = from d in unionInventories
                                  group d by new
                                  {
                                      BranchId = d.BranchId,
                                      Branch = d.Branch,
                                      InventoryCode = d.InventoryCode,
                                      Cost = d.Cost,
                                      UnitId = d.UnitId,
                                      Unit = d.Unit,
                                      Category = d.Category,
                                      Price = d.Price
                                  } into g
                                  select new
                                  {
                                      BranchId = g.Key.BranchId,
                                      Branch = g.Key.Branch,
                                      InventoryCode = g.Key.InventoryCode,
                                      Cost = g.Key.Cost,
                                      UnitId = g.Key.UnitId,
                                      Unit = g.Key.Unit,
                                      BegQuantity = g.Sum(d => d.Document == "Current" ? 0 : d.BegQuantity),
                                      InQuantity = g.Sum(d => d.Document == "Beginning Balance" ? 0 : d.InQuantity),
                                      OutQuantity = g.Sum(d => d.Document == "Beginning Balance" ? 0 : d.OutQuantity),
                                      EndQuantity = g.Sum(d => d.EndQuantity),
                                      Amount = g.Sum(d => d.Quantity * d.Cost),
                                      Category = g.Key.Category,
                                      Price = g.Key.Price
                                  };

                if (inventories.Any())
                {
                    var currentItem = from d in db.MstArticles
                                      where d.Id == ItemId
                                      && d.IsLocked == true
                                      select d;

                    var item = "";
                    if (currentItem.Any())
                    {
                        item = currentItem.FirstOrDefault().ManualArticleCode + " - " + currentItem.FirstOrDefault().Article;
                    }

                    // ============
                    // Item Title
                    // ============
                    PdfPTable itemTitle = new PdfPTable(1);
                    float[] widthCellsItemTitle = new float[] { 100f };
                    itemTitle.SetWidths(widthCellsItemTitle);
                    itemTitle.WidthPercentage = 100;
                    PdfPCell itemHeaderColspan = (new PdfPCell(new Phrase(item, fontArial12Bold)) { HorizontalAlignment = 0, PaddingTop = 10f, PaddingBottom = 14f, Border = 0 });
                    itemTitle.AddCell(itemHeaderColspan);
                    document.Add(itemTitle);

                    // ===========
                    // Data Tables
                    // ===========
                    PdfPTable data = new PdfPTable(10);
                    float[] widthsCellsData = new float[] { 35f, 20f, 10f, 10f, 10f, 16f, 16f, 16f, 16f, 16f };
                    data.SetWidths(widthsCellsData);
                    data.WidthPercentage = 100;
                    data.AddCell(new PdfPCell(new Phrase("Branch", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Code", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Unit", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Price", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Cost", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Quantity", fontArial11Bold)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, Rowspan = 2, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Beg", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("In", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("Out", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                    data.AddCell(new PdfPCell(new Phrase("End", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });

                    Decimal totalAmount = 0;
                    Decimal count = 0;
                    Decimal quantityVariance = 0;
                    Decimal varianceAmount = 0;
                    Decimal totalTotalAmount = 0;
                    Decimal totalVarianceAmount = 0;

                    // =============
                    // Populate Data
                    // =============
                    foreach (var inventory in inventories)
                    {
                        totalAmount = inventory.Cost * inventory.EndQuantity;
                        count = 0;
                        quantityVariance = inventory.EndQuantity - count;
                        varianceAmount = inventory.Cost * quantityVariance;

                        data.AddCell(new PdfPCell(new Phrase(inventory.Branch, fontArial9)) { HorizontalAlignment = 0, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase(inventory.InventoryCode, fontArial9)) { HorizontalAlignment = 0, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase(inventory.Unit, fontArial9)) { HorizontalAlignment = 0, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase(inventory.Price.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase(inventory.Cost.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase(inventory.BegQuantity.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase(inventory.InQuantity.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase(inventory.OutQuantity.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase(inventory.EndQuantity.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                        data.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });

                        totalTotalAmount = totalTotalAmount + (inventory.Cost * inventory.EndQuantity);
                        quantityVariance = inventory.EndQuantity - count;
                        totalVarianceAmount = totalVarianceAmount + (inventory.Cost * quantityVariance);
                    }

                    data.AddCell(new PdfPCell(new Phrase("Total", fontArial10Bold)) { Border = 0, Colspan = 8, HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 5f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(totalTotalAmount.ToString("#,##0.00"), fontArial10Bold)) { Colspan = 2, Border = 0, HorizontalAlignment = 2, Rowspan = 2, PaddingTop = 5f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });

                    document.Add(data);
                }
            }

            // Document End
            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}