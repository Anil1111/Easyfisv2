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

            var collection = from d in db.TrnCollections where d.Id == CollectonId && d.IsLocked == true select d;
            if (collection.Any())
            {
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
                String branchCode = currentBranch.FirstOrDefault().BranchCode;

                String reprinted = "";
                if (collection.FirstOrDefault().IsPrinted)
                {
                    reprinted = "Reprinted";
                }

                PdfPTable headerPage = new PdfPTable(2);
                headerPage.SetWidths(new float[] { 100f, 75f });
                headerPage.WidthPercentage = 100;
                headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
                headerPage.AddCell(new PdfPCell(new Phrase(currentDefaultOfficialReceiptName, fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyTaxNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(branchName, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyAddress, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyContactNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(reprinted, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });

                document.Add(headerPage);
                document.Add(line);


                String customer = collection.FirstOrDefault().MstArticle.Article;
                String collectionNo = collection.FirstOrDefault().ORNumber;
                String TIN = collection.FirstOrDefault().MstArticle.TaxNumber;
                String collectionDate = collection.FirstOrDefault().ORDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String address = collection.FirstOrDefault().MstArticle.Address;
                String manualORNumber = collection.FirstOrDefault().ManualORNumber;
                String businessStyle = collection.FirstOrDefault().MstArticle.MstArticleGroup.ArticleGroup;
                String salesPerson = collection.FirstOrDefault().MstUser4.FullName;
                String salesRemarks = collection.FirstOrDefault().MstArticle.Particulars;
                String preparedBy = collection.FirstOrDefault().MstUser3.FullName;
                String checkedBy = collection.FirstOrDefault().MstUser.FullName;
                String approvedBy = collection.FirstOrDefault().MstUser1.FullName;

                PdfPTable tblCollection = new PdfPTable(4);
                tblCollection.SetWidths(new float[] { 40f, 150f, 70f, 70f });
                tblCollection.WidthPercentage = 100;
                tblCollection.AddCell(new PdfPCell(new Phrase("Customer:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase(customer, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase("No.:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblCollection.AddCell(new PdfPCell(new Phrase("OR-" + collection.FirstOrDefault().MstBranch.BranchCode + "-" + collectionNo, fontArial13Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblCollection.AddCell(new PdfPCell(new Phrase("TIN:  ", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase(TIN, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase("Date:", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblCollection.AddCell(new PdfPCell(new Phrase(collectionDate, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblCollection.AddCell(new PdfPCell(new Phrase("Address:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase(address, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase("OR Ref. No.:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblCollection.AddCell(new PdfPCell(new Phrase(manualORNumber, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblCollection.AddCell(new PdfPCell(new Phrase("Business Style:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase(businessStyle, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, Colspan = 3 });
                tblCollection.AddCell(new PdfPCell(new Phrase("Remarks:", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblCollection.AddCell(new PdfPCell(new Phrase(salesRemarks, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, Colspan = 3 });

                document.Add(tblCollection);

                PdfPTable spaceTable = new PdfPTable(1);
                spaceTable.SetWidths(new float[] { 100f });
                spaceTable.WidthPercentage = 100;
                spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });

                document.Add(spaceTable);

                var collectionLines = from d in collection.FirstOrDefault().TrnCollectionLines select d;
                if (collectionLines.Any())
                {
                    PdfPTable tblCollectionLines = new PdfPTable(6);
                    tblCollectionLines.SetWidths(new float[] { 70f, 120f, 100f, 80f, 140f, 100f });
                    tblCollectionLines.WidthPercentage = 100;
                    tblCollectionLines.AddCell(new PdfPCell(new Phrase("SI Number", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblCollectionLines.AddCell(new PdfPCell(new Phrase("Pay Type", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblCollectionLines.AddCell(new PdfPCell(new Phrase("Check No.", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblCollectionLines.AddCell(new PdfPCell(new Phrase("Check Date", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblCollectionLines.AddCell(new PdfPCell(new Phrase("Check Bank / Branch", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblCollectionLines.AddCell(new PdfPCell(new Phrase("Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    Decimal totalAmount = 0;

                    foreach (var collectionLine in collectionLines)
                    {
                        String SINumber = " ", SIDate = " ";
                        if (collectionLine.SIId != null)
                        {
                            SINumber = collectionLine.TrnSalesInvoice.SINumber;
                            SIDate = collectionLine.TrnSalesInvoice.SIDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                        }

                        tblCollectionLines.AddCell(new PdfPCell(new Phrase(SINumber, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblCollectionLines.AddCell(new PdfPCell(new Phrase(SIDate, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblCollectionLines.AddCell(new PdfPCell(new Phrase(collectionLine.CheckNumber, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblCollectionLines.AddCell(new PdfPCell(new Phrase(collectionLine.CheckDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblCollectionLines.AddCell(new PdfPCell(new Phrase(collectionLine.CheckBank, fontArial11)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblCollectionLines.AddCell(new PdfPCell(new Phrase(collectionLine.Amount.ToString("#,##0.00"), fontArial11)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });

                        totalAmount += collectionLine.Amount;
                    }

                    tblCollectionLines.AddCell(new PdfPCell(new Phrase("Total", fontArial11Bold)) { Colspan = 5, HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                    tblCollectionLines.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial11Bold)) { HorizontalAlignment = 2, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                    document.Add(tblCollectionLines);
                    document.Add(spaceTable);
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

                if (!collection.FirstOrDefault().IsPrinted)
                {
                    collection.FirstOrDefault().IsPrinted = true;
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