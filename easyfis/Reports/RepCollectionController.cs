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
    public class RepCollectionController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =================
        // Collection - PDF
        // ================
        [Authorize]
        public ActionResult Collection(Int32 CollectonId)
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

            var identityUserId = User.Identity.GetUserId();
            var currentUser = from d in db.MstUsers where d.UserId == identityUserId select d;

            var currentCompanyId = currentUser.FirstOrDefault().CompanyId;
            var currentBranchId = currentUser.FirstOrDefault().BranchId;
            var currentDefaultOfficialReceiptName = currentUser.FirstOrDefault().OfficialReceiptName;

            var currentCompany = from d in db.MstCompanies where d.Id == Convert.ToInt32(currentCompanyId) select d;
            var currentBranch = from d in db.MstBranches where d.Id == Convert.ToInt32(currentBranchId) select d;

            String companyName = currentCompany.FirstOrDefault().Company;
            String companyTaxNumber = currentCompany.FirstOrDefault().TaxNumber;
            String companyAddress = currentCompany.FirstOrDefault().Address;
            String companyContactNumber = currentCompany.FirstOrDefault().ContactNumber;
            String branchName = currentBranch.FirstOrDefault().Branch;

            PdfPTable headerPage = new PdfPTable(2);
            headerPage.SetWidths(new float[] { 100f, 75f });
            headerPage.WidthPercentage = 100;
            headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
            headerPage.AddCell(new PdfPCell(new Phrase(currentDefaultOfficialReceiptName, fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(companyTaxNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase(branchName, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(companyAddress, fontArial11)) { Border = 0, PaddingTop = 5f });
            headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
            headerPage.AddCell(new PdfPCell(new Phrase(companyContactNumber, fontArial11)) { Border = 0, PaddingTop = 5f, Colspan = 2 });
            document.Add(headerPage);

            document.Add(line);

            var collection = from d in db.TrnCollections where d.Id == CollectonId && d.IsLocked == true select d;
            if (collection.Any())
            {
                String customer = collection.FirstOrDefault().MstArticle.Article;
                String collectionNo = collection.FirstOrDefault().ORNumber;
                String TIN = collection.FirstOrDefault().MstArticle.TaxNumber;
                String collectionDate = collection.FirstOrDefault().ORDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String address = collection.FirstOrDefault().MstArticle.Address;
                String documentReference = collection.FirstOrDefault().ManualORNumber;
                String businessStyle = collection.FirstOrDefault().MstArticle.MstArticleGroup.ArticleGroup;
                String salesPerson = collection.FirstOrDefault().MstUser4.FullName;
                String salesRemarks = collection.FirstOrDefault().MstArticle.Particulars;
                String preparedBy = collection.FirstOrDefault().MstUser3.FullName;
                String checkedBy = collection.FirstOrDefault().MstUser.FullName;
                String approvedBy = collection.FirstOrDefault().MstUser1.FullName;

                PdfPTable tblCollection = new PdfPTable(4);
                tblCollection.SetWidths(new float[] { 40f, 150f, 70f, 50f });
                tblCollection.WidthPercentage = 100;
                tblCollection.AddCell(new PdfPCell(new Phrase("Customer: ", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase(customer, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase("No.:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblCollection.AddCell(new PdfPCell(new Phrase(collectionNo, fontArial13Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblCollection.AddCell(new PdfPCell(new Phrase("TIN:  ", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase(TIN, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase("Date: ", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblCollection.AddCell(new PdfPCell(new Phrase(collectionDate, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblCollection.AddCell(new PdfPCell(new Phrase("Address: ", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase(address, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase("Document Ref: ", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblCollection.AddCell(new PdfPCell(new Phrase(documentReference, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblCollection.AddCell(new PdfPCell(new Phrase("Business Style: ", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase(businessStyle, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, Colspan = 3 });
                tblCollection.AddCell(new PdfPCell(new Phrase("Remarks: ", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase(salesRemarks, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, Colspan = 3 });
                document.Add(tblCollection);

                PdfPTable spaceTable = new PdfPTable(1);
                spaceTable.SetWidths(new float[] { 100f });
                spaceTable.WidthPercentage = 100;
                spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });
                document.Add(spaceTable);

                var collectionLines = from d in db.TrnCollectionLines
                                      where d.ORId == CollectonId
                                      && d.TrnCollection.IsLocked == true
                                      select new
                                      {
                                          Id = d.Id,
                                          ORId = d.ORId,
                                          OR = d.TrnCollection.ORNumber,
                                          ORDate = d.TrnCollection.ORDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture),
                                          Customer = d.TrnCollection.MstArticle.Article,
                                          BranchId = d.BranchId,
                                          Branch = d.MstBranch.Branch,
                                          AccountId = d.AccountId,
                                          Account = d.MstAccount.Account,
                                          ArticleId = d.ArticleId,
                                          Article = d.MstArticle.Article,
                                          SIId = d.SIId,
                                          SI = d.TrnSalesInvoice.SINumber,
                                          Particulars = d.Particulars,
                                          Amount = d.Amount,
                                          PayTypeId = d.PayTypeId,
                                          PayType = d.MstPayType.PayType,
                                          CheckNumber = d.CheckNumber,
                                          CheckDate = d.CheckDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture),
                                          CheckBank = d.CheckBank,
                                          DepositoryBankId = d.DepositoryBankId,
                                          DepositoryBank = d.MstArticle1.Article,
                                          IsClear = d.IsClear
                                      };

                if (collectionLines.Any())
                {
                    PdfPTable tableCollectionLines = new PdfPTable(6);
                    float[] widthscellsPOLines = new float[] { 70f, 120f, 100f, 80f, 140f, 100f };
                    tableCollectionLines.SetWidths(widthscellsPOLines);
                    tableCollectionLines.WidthPercentage = 100;
                    tableCollectionLines.AddCell(new PdfPCell(new Phrase("SI Number", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tableCollectionLines.AddCell(new PdfPCell(new Phrase("Pay Type", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tableCollectionLines.AddCell(new PdfPCell(new Phrase("Check No.", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tableCollectionLines.AddCell(new PdfPCell(new Phrase("Check Date", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tableCollectionLines.AddCell(new PdfPCell(new Phrase("Check Bank / Branch", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tableCollectionLines.AddCell(new PdfPCell(new Phrase("Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    Decimal totalAmount = 0;

                    //List<Entities.TrnSalesInvoiceItem> salesInvoiceVATItems = new List<Entities.TrnSalesInvoiceItem>();

                    foreach (var collectionLine in collectionLines)
                    {
                        //var salesInvoice = from d in db.TrnSalesInvoices
                        //                   where d.Id == collectionLine.SIId
                        //                   select d;

                        //if (salesInvoice.Any())
                        //{
                        //    var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                        //                            where d.SIId == salesInvoice.FirstOrDefault().Id
                        //                            select d;

                        //    if (salesInvoiceItems.Any())
                        //    {
                        //        foreach (var salesInvoiceItem in salesInvoiceItems)
                        //        {
                        //            salesInvoiceVATItems.Add(new Entities.TrnSalesInvoiceItem()
                        //            {
                        //                VAT = salesInvoiceItem.MstTaxType.TaxType,
                        //                Amount = salesInvoiceItem.Amount,
                        //                VATAmount = salesInvoiceItem.VATAmount
                        //            });
                        //        }
                        //    }
                        //}

                        tableCollectionLines.AddCell(new PdfPCell(new Phrase(collectionLine.SI, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableCollectionLines.AddCell(new PdfPCell(new Phrase(collectionLine.PayType, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableCollectionLines.AddCell(new PdfPCell(new Phrase(collectionLine.CheckNumber, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableCollectionLines.AddCell(new PdfPCell(new Phrase(collectionLine.CheckDate, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableCollectionLines.AddCell(new PdfPCell(new Phrase(collectionLine.CheckBank, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableCollectionLines.AddCell(new PdfPCell(new Phrase(collectionLine.Amount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });

                        totalAmount += collectionLine.Amount;
                    }

                    tableCollectionLines.AddCell(new PdfPCell(new Phrase("Total", fontArial11Bold)) { Colspan = 5, HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableCollectionLines.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tableCollectionLines);

                    document.Add(spaceTable);

                    //// ============
                    //// VAT Analysis
                    //// ============
                    //var VATItems = from d in salesInvoiceVATItems
                    //               group d by new
                    //               {
                    //                   VAT = d.VAT
                    //               } into g
                    //               select new
                    //               {
                    //                   VAT = g.Key.VAT,
                    //                   Amount = g.Sum(d => d.Amount),
                    //                   VATAmount = g.Sum(d => d.VATAmount)
                    //               };

                    //if (VATItems.Any())
                    //{
                    //    PdfPTable tableVATAnalysis = new PdfPTable(3);
                    //    float[] widthsCellsVATItems = new float[] { 200f, 100f, 100f };
                    //    tableVATAnalysis.SetWidths(widthsCellsVATItems);
                    //    tableVATAnalysis.HorizontalAlignment = Element.ALIGN_LEFT;
                    //    tableVATAnalysis.WidthPercentage = 40;
                    //    tableVATAnalysis.AddCell(new PdfPCell(new Phrase("VAT", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    //    tableVATAnalysis.AddCell(new PdfPCell(new Phrase("Amount", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    //    tableVATAnalysis.AddCell(new PdfPCell(new Phrase("VAT Amount", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    //    Decimal totalVATAmount = 0;
                    //    Decimal totalVAT = 0;

                    //    foreach (var VATItem in VATItems)
                    //    {
                    //        tableVATAnalysis.AddCell(new PdfPCell(new Phrase(VATItem.VAT, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                    //        tableVATAnalysis.AddCell(new PdfPCell(new Phrase(VATItem.Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                    //        tableVATAnalysis.AddCell(new PdfPCell(new Phrase(VATItem.VATAmount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });

                    //        totalVATAmount += VATItem.Amount;
                    //        totalVAT += VATItem.VATAmount;
                    //    }

                    //    tableVATAnalysis.AddCell(new PdfPCell(new Phrase("Total", fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    //    tableVATAnalysis.AddCell(new PdfPCell(new Phrase(totalVATAmount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    //    tableVATAnalysis.AddCell(new PdfPCell(new Phrase(totalVAT.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                    //    // TODO: Option Settings for VAT Analysis Table
                    //    //document.Add(tableVATAnalysis);
                    //    //document.Add(spaceTable);
                    //}
                }

                PdfPTable tblSignatures = new PdfPTable(3);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f };
                tblSignatures.WidthPercentage = 100;
                tblSignatures.SetWidths(widthsCellsTableUsers);
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

                var collectionName = currentDefaultOfficialReceiptName.ToUpper();
                PdfPTable tblFooter = new PdfPTable(1);
                tblFooter.SetWidths(new float[] { 100f });
                tblFooter.WidthPercentage = 100;
                tblFooter.AddCell(new PdfPCell(new Phrase("THIS DOCUMENT IS NOT VALID FOR CLAIM OF INPUT TAXES. THIS " + collectionName + " SHALL BE VALID FOR FIVE (5) YEARS FROM THE DATE OF ATP.", fontArial9Italic)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 1 });
                document.Add(tblFooter);
            }

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}