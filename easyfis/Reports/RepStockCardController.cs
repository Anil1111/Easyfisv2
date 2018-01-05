using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace easyfis.Reports
{
    public class RepStockCardController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =====================
        // Stock Card Report PDF
        // =====================
        [Authorize]
        public ActionResult StockCard(String StartDate, String EndDate, Int32 CompanyId, Int32 BranchId, Int32 ItemId)
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
            var branch = (from d in db.MstBranches where d.Id == Convert.ToInt32(BranchId) select d.Branch).FirstOrDefault();

            // ===========
            // Header Page
            // ===========
            PdfPTable headerPage = new PdfPTable(2);
            float[] widthsCellsHeaderPage = new float[] { 100f, 75f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;
            headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
            headerPage.AddCell(new PdfPCell(new Phrase("Stock Card", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(address, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase("Date From " + Convert.ToDateTime(StartDate).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture) + " to " + Convert.ToDateTime(EndDate).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(contactNo, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            document.Add(headerPage);
            document.Add(line);

            var stockCardBeginningBalance = from d in db.TrnInventories
                                            where d.InventoryDate < Convert.ToDateTime(StartDate)
                                            && d.MstBranch.CompanyId == Convert.ToInt32(CompanyId)
                                            && d.BranchId == Convert.ToInt32(BranchId)
                                            && d.MstArticle.IsInventory == true
                                            && d.ArticleId == Convert.ToInt32(ItemId)
                                            select new
                                            {
                                                Document = "Beginning Balance",
                                                Branch = d.MstBranch.Branch,
                                                InventoryDate = Convert.ToDateTime(StartDate),
                                                Quantity = d.Quantity,
                                                InQuantity = d.QuantityIn,
                                                OutQuantity = d.QuantityOut,
                                                BalanceQuantity = d.Quantity,
                                                Unit = d.MstArticle.MstUnit.Unit,
                                                Amount = d.Amount
                                            };

            var groupedStockCardBeginningBalance = from d in stockCardBeginningBalance
                                                   group d by new
                                                   {
                                                       Document = d.Document,
                                                       Branch = d.Branch,
                                                       InventoryDate = d.InventoryDate,
                                                       Unit = d.Unit
                                                   } into g
                                                   select new
                                                   {
                                                       Document = g.Key.Document,
                                                       Branch = g.Key.Branch,
                                                       InventoryDate = g.Key.InventoryDate,
                                                       InQuantity = g.Sum(d => d.InQuantity),
                                                       OutQuantity = g.Sum(d => d.OutQuantity),
                                                       BalanceQuantity = g.Sum(d => d.BalanceQuantity),
                                                       Unit = g.Key.Unit,
                                                       Cost = g.Sum(d => d.InQuantity) > 0 ? g.Sum(d => d.Amount) / g.Sum(d => d.InQuantity) : 0,
                                                       Amount = g.Sum(d => d.InQuantity) > 0 ? g.Sum(d => d.Amount) : 0
                                                   };

            var stockCardCurrentBalance = from d in db.TrnInventories
                                          where d.InventoryDate >= Convert.ToDateTime(StartDate)
                                          && d.InventoryDate <= Convert.ToDateTime(EndDate)
                                          && d.MstBranch.CompanyId == Convert.ToInt32(CompanyId)
                                          && d.BranchId == Convert.ToInt32(BranchId)
                                          && d.MstArticle.IsInventory == true
                                          && d.ArticleId == Convert.ToInt32(ItemId)
                                          select new
                                          {
                                              Document = "Current",
                                              BranchCode = d.MstBranch.BranchCode,
                                              Branch = d.MstBranch.Branch,
                                              InventoryDate = d.InventoryDate,
                                              Quantity = d.Quantity,
                                              InQuantity = d.QuantityIn,
                                              OutQuantity = d.QuantityOut,
                                              BalanceQuantity = d.Quantity,
                                              Unit = d.MstArticle.MstUnit.Unit,
                                              Amount = d.Amount,
                                              RRId = d.RRId,
                                              RRNumber = d.TrnReceivingReceipt.RRNumber,
                                              SIId = d.SIId,
                                              SINumber = d.TrnSalesInvoice.SINumber,
                                              INId = d.INId,
                                              INNumber = d.TrnStockIn.INNumber,
                                              OTId = d.OTId,
                                              OTNumber = d.TrnStockOut.OTNumber,
                                              STId = d.STId,
                                              STNumber = d.TrnStockTransfer.STNumber,
                                              ManualNo = d.RRId != null ? d.TrnReceivingReceipt.ManualRRNumber : d.SIId != null ? d.TrnSalesInvoice.ManualSINumber : d.INId != null ? d.TrnStockIn.ManualINNumber : d.OTId != null ? d.TrnStockOut.ManualOTNumber : d.STId != null ? d.TrnStockTransfer.ManualSTNumber : " "
                                          };

            List<Entities.RepStockCardReport> listStockCardInventory = new List<Entities.RepStockCardReport>();

            Decimal beginningQuantity = 0;

            if (groupedStockCardBeginningBalance.Any())
            {
                var begBalance = groupedStockCardBeginningBalance.FirstOrDefault();

                beginningQuantity = begBalance.InQuantity > 0 ? begBalance.BalanceQuantity : 0;

                listStockCardInventory.Add(new Entities.RepStockCardReport()
                {
                    Document = begBalance.Document,
                    BranchCode = " ",
                    Branch = begBalance.Branch,
                    InventoryDate = begBalance.InventoryDate.ToShortDateString(),
                    InQuantity = begBalance.InQuantity,
                    OutQuantity = begBalance.OutQuantity,
                    BalanceQuantity = begBalance.BalanceQuantity,
                    RunningQuantity = begBalance.BalanceQuantity,
                    Unit = begBalance.Unit,
                    Cost = begBalance.Cost,
                    Amount = begBalance.Amount,
                    RRId = null,
                    RRNumber = "Beginning Balance",
                    SIId = null,
                    SINumber = "Beginning Balance",
                    INId = null,
                    INNumber = "Beginning Balance",
                    OTId = null,
                    OTNumber = "Beginning Balance",
                    STId = null,
                    STNumber = "Beginning Balance",
                    ManualNumber = " "
                });

                beginningQuantity = begBalance.BalanceQuantity;
            }

            Decimal runningQuantity = 0, cost = 0, amount = 0;

            if (stockCardCurrentBalance.Any())
            {
                foreach (var curBalance in stockCardCurrentBalance)
                {
                    runningQuantity = (beginningQuantity + curBalance.InQuantity) - curBalance.OutQuantity;

                    cost = curBalance.Quantity != 0 ? curBalance.Amount / curBalance.Quantity : 0;
                    amount = curBalance.InQuantity > 0 ? curBalance.Amount : 0;

                    listStockCardInventory.Add(new Entities.RepStockCardReport()
                    {
                        Document = curBalance.Document,
                        BranchCode = curBalance.BranchCode,
                        Branch = curBalance.Branch,
                        InventoryDate = curBalance.InventoryDate.ToShortDateString(),
                        InQuantity = curBalance.InQuantity,
                        OutQuantity = curBalance.OutQuantity,
                        BalanceQuantity = curBalance.BalanceQuantity,
                        RunningQuantity = runningQuantity,
                        Unit = curBalance.Unit,
                        Cost = cost,
                        Amount = amount,
                        RRId = curBalance.RRId,
                        RRNumber = curBalance.RRNumber,
                        SIId = curBalance.SIId,
                        SINumber = curBalance.SINumber,
                        INId = curBalance.INId,
                        INNumber = curBalance.INNumber,
                        OTId = curBalance.OTId,
                        OTNumber = curBalance.OTNumber,
                        STId = curBalance.STId,
                        STNumber = curBalance.STNumber,
                        ManualNumber = curBalance.ManualNo
                    });

                    beginningQuantity = runningQuantity;
                }
            }

            if (listStockCardInventory.Any())
            {
                // ============
                // Branch Title
                // ============
                PdfPTable branchTitle = new PdfPTable(1);
                float[] widthCellsBranchTitle = new float[] { 100f };
                branchTitle.SetWidths(widthCellsBranchTitle);
                branchTitle.WidthPercentage = 100;
                PdfPCell branchHeaderColspan = (new PdfPCell(new Phrase(branch, fontArial12Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 10f });
                branchTitle.AddCell(branchHeaderColspan);
                document.Add(branchTitle);

                // ==========
                // Item Title 
                // ==========
                var item = from d in db.MstArticles
                           where d.Id == Convert.ToInt32(ItemId)
                           && d.IsLocked == true
                           select d;

                if (item.Any())
                {
                    String itemName = item.FirstOrDefault().Article;

                    PdfPTable itemTitle = new PdfPTable(1);
                    float[] widthCellsItemTitle = new float[] { 100f };
                    itemTitle.SetWidths(widthCellsItemTitle);
                    itemTitle.WidthPercentage = 100;
                    itemTitle.AddCell(new PdfPCell(new Phrase(itemName, fontArial12Bold)) { Border = 0, HorizontalAlignment = 0, PaddingBottom = 14f });
                    document.Add(itemTitle);
                }

                // ====
                // Data
                // ====
                PdfPTable data = new PdfPTable(10);
                float[] widthsCellsData = new float[] { 30f, 18f, 30f, 20f, 20f, 20f, 20f, 15f, 25f, 25f };
                data.SetWidths(widthsCellsData);
                data.WidthPercentage = 100;
                data.AddCell(new PdfPCell(new Phrase("Document", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, BackgroundColor = BaseColor.LIGHT_GRAY });
                data.AddCell(new PdfPCell(new Phrase("Date", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, BackgroundColor = BaseColor.LIGHT_GRAY });
                data.AddCell(new PdfPCell(new Phrase("Manual No.", fontArial11Bold)) { HorizontalAlignment = 1, Rowspan = 2, PaddingTop = 3f, BackgroundColor = BaseColor.LIGHT_GRAY });
                data.AddCell(new PdfPCell(new Phrase("Quantity", fontArial11Bold)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                data.AddCell(new PdfPCell(new Phrase("Unit", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, Rowspan = 2, BackgroundColor = BaseColor.LIGHT_GRAY });
                data.AddCell(new PdfPCell(new Phrase("Cost", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, Rowspan = 2, BackgroundColor = BaseColor.LIGHT_GRAY });
                data.AddCell(new PdfPCell(new Phrase("Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, Rowspan = 2, BackgroundColor = BaseColor.LIGHT_GRAY });
                data.AddCell(new PdfPCell(new Phrase("In", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                data.AddCell(new PdfPCell(new Phrase("Out", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                data.AddCell(new PdfPCell(new Phrase("Balance", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });
                data.AddCell(new PdfPCell(new Phrase("Running", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 5f, BackgroundColor = BaseColor.LIGHT_GRAY });

                Decimal TotalIn = 0;
                Decimal TotalOut = 0;
                Decimal TotalBalance = 0;
                Decimal TotalAmount = 0;

                foreach (var inventory in listStockCardInventory)
                {
                    String documentRef = " ";

                    if (inventory.RRId != null)
                    {
                        documentRef = "RR-" + inventory.BranchCode + "-" + inventory.RRNumber;
                    } 
                    else if (inventory.SIId != null)
                    {
                        documentRef = "SI-" + inventory.BranchCode + "-" + inventory.SINumber;
                    }
                    else if (inventory.INId != null)
                    {
                        documentRef = "IN-" + inventory.BranchCode + "-" + inventory.INNumber;
                    }
                    else if (inventory.OTId != null)
                    {
                        documentRef = "OT-" + inventory.BranchCode + "-" + inventory.OTNumber;
                    }
                    else if (inventory.STId != null)
                    {
                        documentRef = "ST-" + inventory.BranchCode + "-" + inventory.STNumber;
                    }
                    else
                    {
                        documentRef = "Beginning Balance";
                    }

                    data.AddCell(new PdfPCell(new Phrase(documentRef, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(inventory.InventoryDate, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(inventory.ManualNumber, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(inventory.InQuantity.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(inventory.OutQuantity.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(inventory.BalanceQuantity.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(inventory.RunningQuantity.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(inventory.Unit, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(inventory.Cost.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                    data.AddCell(new PdfPCell(new Phrase(inventory.Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });

                    TotalIn += inventory.InQuantity;
                    TotalOut += inventory.OutQuantity;
                    TotalBalance += inventory.BalanceQuantity;
                    TotalAmount += inventory.Amount;
                }

                data.AddCell(new PdfPCell(new Phrase("TOTAL", fontArial9Bold)) { Colspan = 3, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase(TotalIn.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase(TotalOut.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase(TotalBalance.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Colspan = 3, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                data.AddCell(new PdfPCell(new Phrase(TotalAmount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 5f, PaddingLeft = 5f, PaddingRight = 5f });

                document.Add(data);
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