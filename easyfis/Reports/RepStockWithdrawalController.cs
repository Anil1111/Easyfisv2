﻿using iTextSharp.text;
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

            var stockWithdrawal = from d in db.TrnStockWithdrawals where d.Id == StockWithdrawalId && d.IsLocked == true select d;
            if (stockWithdrawal.Any())
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
                if (stockWithdrawal.FirstOrDefault().IsPrinted)
                {
                    reprinted = "Reprinted";
                }

                PdfPTable headerPage = new PdfPTable(2);
                headerPage.SetWidths(new float[] { 100f, 75f });
                headerPage.WidthPercentage = 100;
                headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
                headerPage.AddCell(new PdfPCell(new Phrase("Stock Withdrawal", fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyTaxNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(branchName, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyAddress, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyContactNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(reprinted, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });

                document.Add(headerPage);
                document.Add(line);

                String fromBranch = stockWithdrawal.FirstOrDefault().MstBranch.Branch;
                String SIBranch = stockWithdrawal.FirstOrDefault().MstBranch1.Branch;
                String SINumber = stockWithdrawal.FirstOrDefault().TrnSalesInvoice.SINumber;
                String remarks = stockWithdrawal.FirstOrDefault().Remarks;
                String contactPerson = stockWithdrawal.FirstOrDefault().ContactPerson;
                String contacNumber = stockWithdrawal.FirstOrDefault().ContactNumber;
                String contactAddress = stockWithdrawal.FirstOrDefault().Address;
                String SWNumber = stockWithdrawal.FirstOrDefault().SWNumber;
                String SWDate = stockWithdrawal.FirstOrDefault().SWDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture); ;
                String preparedBy = stockWithdrawal.FirstOrDefault().MstUser.FullName;
                String checkedBy = stockWithdrawal.FirstOrDefault().MstUser1.FullName;
                String approvedBy = stockWithdrawal.FirstOrDefault().MstUser2.FullName;
                String receivedBy = stockWithdrawal.FirstOrDefault().MstUser5.FullName;

                PdfPTable tblStockWithdrawal = new PdfPTable(4);
                tblStockWithdrawal.SetWidths(new float[] { 40f, 150f, 70f, 70f });
                tblStockWithdrawal.WidthPercentage = 100;
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase("From Branch:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase(fromBranch, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase("No.:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase("SW-" + stockWithdrawal.FirstOrDefault().MstBranch.BranchCode + "-" + SWNumber, fontArial13Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase("SI Branch:", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase(SIBranch, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase("Date:", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase(SWDate, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase("Remarks:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase(remarks, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase("SI No.:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase(SINumber, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase("Contact Person:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase(contactPerson, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase("", fontArial11Bold)) { Colspan = 2, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase("Contact No.:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase(contacNumber, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase("", fontArial11Bold)) { Colspan = 2, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase("Address:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase(contactAddress, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tblStockWithdrawal.AddCell(new PdfPCell(new Phrase("", fontArial11Bold)) { Colspan = 2, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                document.Add(tblStockWithdrawal);

                PdfPTable spaceTable = new PdfPTable(1);
                spaceTable.SetWidths(new float[] { 100f });
                spaceTable.WidthPercentage = 100;
                spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });

                document.Add(spaceTable);

                var stockWithdrawalItems = from d in stockWithdrawal.FirstOrDefault().TrnStockWithdrawalItems select d;
                if (stockWithdrawalItems.Any())
                {
                    var numberOfTableColumns = 4;
                    float[] widthscellsPOLines = new float[] { 100f, 70f, 150f, 200f };

                    if (currentIsIncludeCostStockReports)
                    {
                        numberOfTableColumns = 6;
                        widthscellsPOLines = new float[] { 100f, 70f, 150f, 200f, 100f, 100f };
                    }

                    PdfPTable tblStockWithdrawalItems = new PdfPTable(numberOfTableColumns);
                    tblStockWithdrawalItems.SetWidths(widthscellsPOLines);
                    tblStockWithdrawalItems.WidthPercentage = 100;
                    tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Code", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Item", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    if (currentIsIncludeCostStockReports)
                    {
                        tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Cost", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                        tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    }

                    Decimal totalAmount = 0;

                    foreach (var stockWithdrawalItem in stockWithdrawalItems)
                    {
                        string SKUCode = stockWithdrawalItem.MstArticle.ManualArticleOldCode;
                        if (stockWithdrawalItem.MstArticle.ManualArticleOldCode.Equals("NA") || stockWithdrawalItem.MstArticle.ManualArticleOldCode.Equals("na"))
                        {
                            SKUCode = " ";
                        }

                        tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(stockWithdrawalItem.Quantity.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(stockWithdrawalItem.MstUnit.Unit, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(stockWithdrawalItem.MstArticle.ArticleCode, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(stockWithdrawalItem.MstArticle.Article + " " + SKUCode, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });

                        if (currentIsIncludeCostStockReports)
                        {
                            tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(stockWithdrawalItem.Cost.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                            tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(stockWithdrawalItem.Amount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        }

                        totalAmount += stockWithdrawalItem.Amount;
                    }

                    if (currentIsIncludeCostStockReports)
                    {
                        tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase("Total", fontArial11Bold)) { Colspan = 5, HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblStockWithdrawalItems.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    }

                    document.Add(tblStockWithdrawalItems);
                    document.Add(spaceTable);
                }

                PdfPTable tblSignatures = new PdfPTable(4);
                tblSignatures.SetWidths(new float[] { 100f, 100f, 100f, 100f });
                tblSignatures.WidthPercentage = 100;
                tblSignatures.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase("Checked by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase("Approved by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase("Received by", fontArial11Bold)) { PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(" ")) { PaddingBottom = 30f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSignatures.AddCell(new PdfPCell(new Phrase(receivedBy, fontArial11)) { HorizontalAlignment = 1, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                document.Add(tblSignatures);

                if (!stockWithdrawal.FirstOrDefault().IsPrinted)
                {
                    stockWithdrawal.FirstOrDefault().IsPrinted = true;
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