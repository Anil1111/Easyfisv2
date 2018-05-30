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
    public class RepStockWithdrawalController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ======================
        // Stock Withdrawal - PDF
        // ======================
        [Authorize]
        public ActionResult StockWithdrawal(Int32 StockWithdrawalId)
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
            var currentIsIncludeCostStockReports = currentUser.FirstOrDefault().IsIncludeCostStockReports;

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
            headerPage.AddCell(new PdfPCell(new Phrase("Withdrawal Slip", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
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

            // ===================
            // Get Stock Withdrawals
            // ===================
            var stockWithdrawals = from d in db.TrnStockWithdrawals
                                   where d.Id == StockWithdrawalId
                                   && d.IsLocked == true
                                   select d;

            if (stockWithdrawals.Any())
            {
                String fromBranch = stockWithdrawals.FirstOrDefault().MstBranch.Branch;
                String SIBranch = stockWithdrawals.FirstOrDefault().MstBranch1.Branch;
                String SINumber = stockWithdrawals.FirstOrDefault().TrnSalesInvoice.SINumber;
                String remarks = stockWithdrawals.FirstOrDefault().Remarks;
                String contactPerson = stockWithdrawals.FirstOrDefault().ContactPerson;
                String contacNumber = stockWithdrawals.FirstOrDefault().ContactNumber;
                String contactAddress = stockWithdrawals.FirstOrDefault().Address;
                String SWNumber = stockWithdrawals.FirstOrDefault().SWNumber;
                String SWDate = stockWithdrawals.FirstOrDefault().SWDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture); ;
                String preparedBy = stockWithdrawals.FirstOrDefault().MstUser3.FullName;
                String checkedBy = stockWithdrawals.FirstOrDefault().MstUser1.FullName;
                String approvedBy = stockWithdrawals.FirstOrDefault().MstUser.FullName;

                PdfPTable tableStockWithdrawals = new PdfPTable(4);
                float[] widthscellsTablePurchaseOrder = new float[] { 40f, 150f, 70f, 50f };
                tableStockWithdrawals.SetWidths(widthscellsTablePurchaseOrder);
                tableStockWithdrawals.WidthPercentage = 100;

                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase("From Branch", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase(fromBranch, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase("No.", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase(SWNumber, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase("SI Branch", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase(SIBranch, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase("Date", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase(SWDate, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase("Remarks", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase(remarks, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase("SI No.", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase(SINumber, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase("Contact Person", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase(contactPerson, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase("", fontArial11Bold)) { Colspan = 2, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase("Contact No.", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase(contacNumber, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase("", fontArial11Bold)) { Colspan = 2, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase("Address", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase(contactAddress, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableStockWithdrawals.AddCell(new PdfPCell(new Phrase("", fontArial11Bold)) { Colspan = 2, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                document.Add(tableStockWithdrawals);

                document.Add(spaceTable);

                // ========================
                // Get Stock Withdrawal Items
                // ========================
                var stockWithdrawalItems = from d in db.TrnStockWithdrawalItems
                                           where d.SWId == StockWithdrawalId
                                           && d.TrnStockWithdrawal.IsLocked == true
                                           select new
                                           {
                                               Id = d.Id,
                                               SWId = d.SWId,
                                               SW = d.TrnStockWithdrawal.SWNumber,
                                               ItemId = d.ItemId,
                                               SKUCode = d.MstArticle.ManualArticleOldCode,
                                               ItemCode = d.MstArticle.ManualArticleCode,
                                               Item = d.MstArticle.Article,
                                               ItemInventoryId = d.ItemInventoryId,
                                               ItemInventory = d.MstArticleInventory.InventoryCode,
                                               Particulars = d.Particulars,
                                               UnitId = d.UnitId,
                                               Unit = d.MstUnit1.Unit,
                                               Quantity = d.Quantity,
                                               Cost = d.Cost,
                                               Amount = d.Amount,
                                               BaseUnitId = d.BaseUnitId,
                                               BaseUnit = d.MstUnit.Unit,
                                               BaseQuantity = d.BaseQuantity,
                                               BaseCost = d.BaseCost
                                           };

                if (stockWithdrawalItems.Any())
                {
                    var numberOfTableColumns = 4;
                    float[] widthscellsPOLines = new float[] { 100f, 70f, 150f, 200f };

                    if (currentIsIncludeCostStockReports)
                    {
                        numberOfTableColumns = 6;
                        widthscellsPOLines = new float[] { 100f, 70f, 150f, 200f, 100f, 100f };
                    }

                    PdfPTable tableStockWithdrawalItems = new PdfPTable(numberOfTableColumns);
                    tableStockWithdrawalItems.SetWidths(widthscellsPOLines);
                    tableStockWithdrawalItems.WidthPercentage = 100;
                    tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Code", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Item", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    if (currentIsIncludeCostStockReports)
                    {
                        tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Cost", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                        tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    }

                    Decimal totalAmount = 0;

                    foreach (var stockWithdrawalItem in stockWithdrawalItems)
                    {
                        string SKUCode = stockWithdrawalItem.SKUCode;
                        if (stockWithdrawalItem.SKUCode.Equals("NA") || stockWithdrawalItem.SKUCode.Equals("na"))
                        {
                            SKUCode = " ";
                        }

                        tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(stockWithdrawalItem.Quantity.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(stockWithdrawalItem.Unit, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(stockWithdrawalItem.ItemCode, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(stockWithdrawalItem.Item + " " + SKUCode, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });

                        if (currentIsIncludeCostStockReports)
                        {
                            tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(stockWithdrawalItem.Cost.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(stockWithdrawalItem.Amount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        }

                        totalAmount += stockWithdrawalItem.Amount;
                    }

                    if (currentIsIncludeCostStockReports)
                    {
                        tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Total", fontArial11Bold)) { Colspan = 5, HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    }

                    document.Add(tableStockWithdrawalItems);

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

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}