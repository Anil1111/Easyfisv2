using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace easyfis.Reports
{
    [Authorize]
    public class RepSalesController : Controller
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===================
        // Sales Invoice - PDF
        // ===================
        public ActionResult Sales(Int32 SalesId)
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

            var salesInvoice = from d in db.TrnSalesInvoices where d.Id == Convert.ToInt32(SalesId) && d.IsLocked == true select d;
            if (salesInvoice.Any())
            {
                var identityUserId = User.Identity.GetUserId();
                var currentUser = from d in db.MstUsers where d.UserId == identityUserId select d;

                var currentCompanyId = currentUser.FirstOrDefault().CompanyId;
                var currentBranchId = currentUser.FirstOrDefault().BranchId;
                var currentDefaultSalesInvoiceName = currentUser.FirstOrDefault().SalesInvoiceName;

                var currentCompany = from d in db.MstCompanies where d.Id == Convert.ToInt32(currentCompanyId) select d;
                var currentBranch = from d in db.MstBranches where d.Id == Convert.ToInt32(currentBranchId) select d;

                String companyName = currentCompany.FirstOrDefault().Company;
                String companyTaxNumber = currentCompany.FirstOrDefault().TaxNumber;
                String companyAddress = currentCompany.FirstOrDefault().Address;
                String companyContactNumber = currentCompany.FirstOrDefault().ContactNumber;
                String branchName = currentBranch.FirstOrDefault().Branch;
                String branchCode = currentBranch.FirstOrDefault().BranchCode;

                String reprinted = "";
                if (salesInvoice.FirstOrDefault().IsPrinted)
                {
                    reprinted = "Reprinted";
                }

                PdfPTable headerPage = new PdfPTable(2);
                headerPage.SetWidths(new float[] { 100f, 75f });
                headerPage.WidthPercentage = 100;
                headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial17Bold)) { Border = 0 });
                headerPage.AddCell(new PdfPCell(new Phrase(currentDefaultSalesInvoiceName, fontArial17Bold)) { Border = 0, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyTaxNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(branchName, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyAddress, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });
                headerPage.AddCell(new PdfPCell(new Phrase(companyContactNumber, fontArial11)) { Border = 0, PaddingTop = 5f });
                headerPage.AddCell(new PdfPCell(new Phrase(reprinted, fontArial11)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 2 });

                document.Add(headerPage);
                document.Add(line);

                String soldTo = salesInvoice.FirstOrDefault().MstArticle.Article;
                String salesNo = salesInvoice.FirstOrDefault().SINumber;
                String TIN = salesInvoice.FirstOrDefault().MstArticle.TaxNumber;
                String salesDate = salesInvoice.FirstOrDefault().SIDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                String address = salesInvoice.FirstOrDefault().MstArticle.Address;
                String documentReference = salesInvoice.FirstOrDefault().DocumentReference;
                String businessStyle = salesInvoice.FirstOrDefault().MstArticle.MstArticleGroup.ArticleGroup;
                String salesPerson = salesInvoice.FirstOrDefault().MstUser4.FullName;
                String salesRemarks = salesInvoice.FirstOrDefault().MstArticle.Particulars;
                String preparedBy = salesInvoice.FirstOrDefault().MstUser3.FullName;
                String checkedBy = salesInvoice.FirstOrDefault().MstUser1.FullName;
                String approvedBy = salesInvoice.FirstOrDefault().MstUser.FullName;

                PdfPTable tblSalesInvoice = new PdfPTable(4);
                tblSalesInvoice.SetWidths(new float[] { 40f, 150f, 70f, 70f });
                tblSalesInvoice.WidthPercentage = 100;
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase("Sold To: ", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase(soldTo, fontArial11)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase("No.:", fontArial11Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase("SI-" + salesInvoice.FirstOrDefault().MstBranch.BranchCode + "-" + salesNo, fontArial13Bold)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase("TIN:  ", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase(TIN, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase("Date: ", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase(salesDate, fontArial11)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase("Address: ", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase(address, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase("Doc. Ref. No.: ", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase(documentReference, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase("Business Style: ", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase(businessStyle, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, Colspan = 3 });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase("Sales Person: ", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase(salesPerson, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, Colspan = 3 });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase("Remarks: ", fontArial11Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f });
                tblSalesInvoice.AddCell(new PdfPCell(new Phrase(salesRemarks, fontArial11)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, Colspan = 3 });

                document.Add(tblSalesInvoice);

                PdfPTable spaceTable = new PdfPTable(1);
                spaceTable.SetWidths(new float[] { 100f });
                spaceTable.WidthPercentage = 100;
                spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });

                document.Add(spaceTable);

                var salesInvoiceItems = from d in salesInvoice.FirstOrDefault().TrnSalesInvoiceItems select d;
                if (salesInvoiceItems.Any())
                {
                    PdfPTable tblSalesInvoiceItems = new PdfPTable(6);
                    tblSalesInvoiceItems.SetWidths(new float[] { 80f, 70f, 170f, 150f, 100f, 100f });
                    tblSalesInvoiceItems.WidthPercentage = 100;
                    tblSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Item", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Particulars", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Price", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tblSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Amount", fontArial11Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    Decimal totalAmount = 0;
                    foreach (var salesInvoiceItem in salesInvoiceItems)
                    {
                        String particulars = "";
                        if (!salesInvoiceItem.Particulars.Equals("NA") || !salesInvoiceItem.Particulars.Equals("na")) { particulars = salesInvoiceItem.Particulars; }

                        String SKUCode = salesInvoiceItem.MstArticle.ManualArticleOldCode;
                        if (salesInvoiceItem.MstArticle.ManualArticleOldCode.Equals("NA") || salesInvoiceItem.MstArticle.ManualArticleOldCode.Equals("na")) { SKUCode = " "; }

                        tblSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.Quantity.ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.MstArticle.MstUnit.Unit, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.MstArticle.Article + " " + SKUCode, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(particulars, fontArial10)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.NetPrice.ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                        tblSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem.Amount.ToString("#,##0.00"), fontArial10)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });

                        if (salesInvoiceItem.MstDiscount.Discount.Equals("Senior Citizen Discount") || salesInvoiceItem.MstDiscount.Discount.Equals("PWD"))
                        {
                            totalAmount += salesInvoiceItem.Price * salesInvoiceItem.Quantity;
                        }
                        else
                        {
                            totalAmount += salesInvoiceItem.Amount;
                        }
                    }

                    document.Add(tblSalesInvoiceItems);

                    var vatItems = from d in salesInvoice.FirstOrDefault().TrnSalesInvoiceItems group d by new { d.MstTaxType.TaxType, d.MstDiscount.Discount } into g select g;
                    Decimal totalVATAmount = vatItems.Where(d => d.Key.Discount != "Senior Citizen Discount" || d.Key.Discount != "PWD").Sum(d => d.Sum(g => g.VATAmount));
                    Decimal totalVATAmountSenior = vatItems.Where(d => d.Key.Discount == "Senior Citizen Discount" || d.Key.Discount == "PWD").Sum(d => d.Sum(g => ((g.MstArticle.MstTaxType.TaxRate / 100) * (g.Price * g.Quantity) / ((g.MstArticle.MstTaxType.TaxRate / 100) + 1))));

                    Decimal totalAmountNetofVAT = totalAmount - (totalVATAmount + totalVATAmountSenior);

                    var vatableSalesItems = from d in salesInvoiceItems where d.MstTaxType.TaxType.Equals("VAT Output") select d;
                    var vatExemptItems = from d in salesInvoiceItems
                                         where d.MstTaxType.TaxType.Equals("VAT Exempt") && !(d.MstDiscount.Discount.Equals("Senior Citizen Discount") || d.MstDiscount.Discount.Equals("PWD"))
                                         select d;

                    var vatExemptItemsSenior = from d in salesInvoiceItems
                                               where d.MstTaxType.TaxType.Equals("VAT Exempt") && (d.MstDiscount.Discount.Equals("Senior Citizen Discount") || d.MstDiscount.Discount.Equals("PWD"))
                                               select d;

                    var vatExemptAmount = vatExemptItems.Sum(d => d.Amount) + vatExemptItemsSenior.Sum(g => totalAmount - ((g.MstArticle.MstTaxType.TaxRate / 100) * (g.Price * g.Quantity) / ((g.MstArticle.MstTaxType.TaxRate / 100) + 1)));

                    var vatZeroRatedItems = from d in salesInvoiceItems where d.MstTaxType.TaxType.Equals("VAT Zero Rated") select d;
                    var lessSCPWDDiscount = from d in salesInvoiceItems where d.MstDiscount.Discount.Equals("Senior Citizen Discount") || d.MstDiscount.Discount.Equals("PWD") select d;

                    var lessSCPWDDiscountAmount = lessSCPWDDiscount.Sum(d => d.DiscountAmount * d.Quantity);
                    var totalAmountDue = totalAmount;

                    var amountDue = totalAmountNetofVAT - lessSCPWDDiscountAmount;
                    if (lessSCPWDDiscountAmount > 0)
                    {
                        totalAmountDue = amountDue;
                    }

                    PdfPTable tblVATAnalysis = new PdfPTable(5);
                    tblVATAnalysis.SetWidths(new float[] { 130f, 50f, 30f, 70f, 30f });
                    tblVATAnalysis.WidthPercentage = 100;
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase("VATable Sales:", fontArial9)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase(vatableSalesItems.Sum(d => d.Amount).ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase("Total Sales (VAT Inclusive).:", fontArial9)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase("VAT Exempt Sales:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase(vatExemptAmount.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase("Less VAT:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase((totalVATAmount + totalVATAmountSenior).ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase("Zero Rated Sales:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase(vatZeroRatedItems.Sum(d => d.Amount).ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase("Amount Net of VAT:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase(totalAmountNetofVAT.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase("VAT Amount:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase(totalVATAmount.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase("Less SC/PWD Discount:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase(lessSCPWDDiscount.Sum(d => d.DiscountAmount * d.Quantity).ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, Colspan = 3, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase("TOTAL AMOUNT DUE:", fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    tblVATAnalysis.AddCell(new PdfPCell(new Phrase(totalAmountDue.ToString("#,##0.00"), fontArial11Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                    document.Add(tblVATAnalysis);
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
                tblSignatures.AddCell(new PdfPCell(new Phrase("Date Received:", fontArial11Bold)) { HorizontalAlignment = 0, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                document.Add(tblSignatures);

                var InvoiceName = currentDefaultSalesInvoiceName.ToUpper();
                PdfPTable tblFooter = new PdfPTable(1);
                tblFooter.SetWidths(new float[] { 100f });
                tblFooter.WidthPercentage = 100;
                tblFooter.AddCell(new PdfPCell(new Phrase("THE " + InvoiceName + " SHALL BE VALID FOR FIVE (5) YEARS FROM THE DATE OF ATP", fontArial9Italic)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = 1 });

                document.Add(tblFooter);

                if (!salesInvoice.FirstOrDefault().IsPrinted)
                {
                    salesInvoice.FirstOrDefault().IsPrinted = true;
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