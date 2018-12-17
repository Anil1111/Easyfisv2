using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;

namespace easyfis.Integration.POSIntegration.ApiControllers
{
    public class ApiPOSIntegrationTrnSalesInvoiceController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========
        // BUSINESS
        // ========
        private Business.Inventory inventory = new Business.Inventory();
        private Business.Journal journal = new Business.Journal();

        // ============================
        // Zero Fill - Document Numbers
        // ============================
        public String ZeroFill(Int32 number, Int32 length)
        {
            var result = number.ToString();
            var pad = length - result.Length;
            while (pad > 0)
            {
                result = '0' + result;
                pad--;
            }

            return result;
        }

        // ===================================
        // ADD Sales Invoice (POS Integration)
        // ===================================
        [HttpPost]
        [Route("api/add/POSIntegration/salesInvoice")]
        public HttpResponseMessage AddSalesInvoicePOSIntegration(Entities.POSIntegrationTrnSalesInvoice POSIntegrationTrnSalesInvoiceObject)
        {
            try
            {
                var branch = from d in db.MstBranches where d.BranchCode == POSIntegrationTrnSalesInvoiceObject.BranchCode select d;
                if (branch.Any())
                {
                    Int32 currentBranchId = branch.FirstOrDefault().Id;

                    var SINumberResult = "0000000001";
                    var lastSalesInvoice = from d in db.TrnSalesInvoices.OrderByDescending(d => d.Id)
                                           where d.BranchId == currentBranchId
                                           select d;

                    if (lastSalesInvoice.Any())
                    {
                        var SINumber = Convert.ToInt32(lastSalesInvoice.FirstOrDefault().SINumber) + 0000000001;
                        SINumberResult = ZeroFill(SINumber, 10);
                    }

                    Boolean customerExist = false;
                    var customers = from d in db.MstArticles where d.ManualArticleCode == POSIntegrationTrnSalesInvoiceObject.CustomerManualArticleCode && d.ArticleTypeId == 2 select d;
                    if (customers.Any()) { customerExist = true; }

                    Boolean userExist = false;
                    var users = from d in db.MstUsers where d.UserName == POSIntegrationTrnSalesInvoiceObject.CreatedBy select d;
                    if (users.Any()) { userExist = true; }

                    Boolean termExist = false;
                    var terms = from d in db.MstTerms where d.Term == POSIntegrationTrnSalesInvoiceObject.Term select d;
                    if (terms.Any()) { termExist = true; }

                    if (customerExist)
                    {
                        if (userExist)
                        {
                            if (termExist)
                            {
                                Data.TrnSalesInvoice addSalesInvoice = new Data.TrnSalesInvoice
                                {
                                    BranchId = currentBranchId,
                                    SINumber = SINumberResult,
                                    SIDate = Convert.ToDateTime(POSIntegrationTrnSalesInvoiceObject.SIDate),
                                    CustomerId = customers.FirstOrDefault().Id,
                                    TermId = terms.FirstOrDefault().Id,
                                    DocumentReference = POSIntegrationTrnSalesInvoiceObject.DocumentReference,
                                    ManualSINumber = POSIntegrationTrnSalesInvoiceObject.ManualSINumber,
                                    Remarks = POSIntegrationTrnSalesInvoiceObject.Remarks,
                                    Amount = 0,
                                    PaidAmount = 0,
                                    AdjustmentAmount = 0,
                                    BalanceAmount = 0,
                                    SoldById = users.FirstOrDefault().Id,
                                    PreparedById = users.FirstOrDefault().Id,
                                    CheckedById = users.FirstOrDefault().Id,
                                    ApprovedById = users.FirstOrDefault().Id,
                                    Status = null,
                                    IsCancelled = false,
                                    IsPrinted = false,
                                    IsLocked = true,
                                    CreatedById = users.FirstOrDefault().Id,
                                    CreatedDateTime = DateTime.Now,
                                    UpdatedById = users.FirstOrDefault().Id,
                                    UpdatedDateTime = DateTime.Now
                                };

                                db.TrnSalesInvoices.InsertOnSubmit(addSalesInvoice);
                                db.SubmitChanges();

                                foreach (var salesInvoiceItem in POSIntegrationTrnSalesInvoiceObject.ListPOSIntegrationTrnSalesInvoiceItem.ToList())
                                {
                                    Boolean itemExist = false;
                                    var items = from d in db.MstArticles where d.ManualArticleCode == salesInvoiceItem.ItemManualArticleCode && d.ArticleTypeId == 1 select d;
                                    if (items.Any()) { itemExist = true; }

                                    if (itemExist)
                                    {
                                        Int32? itemInventoryId = null;
                                        var articleInventory = from d in db.MstArticleInventories where d.BranchId == currentBranchId && d.ArticleId == items.FirstOrDefault().Id select d;
                                        if (articleInventory.Any()) { itemInventoryId = articleInventory.FirstOrDefault().Id; }

                                        Boolean unitExist = false;
                                        var units = from d in db.MstUnits where d.Unit == salesInvoiceItem.Unit select d;
                                        if (units.Any()) { unitExist = true; }

                                        Boolean discountExist = false;
                                        var discounts = from d in db.MstDiscounts where d.Discount == salesInvoiceItem.Discount select d;
                                        if (discounts.Any()) { discountExist = true; }

                                        Boolean taxExist = false;
                                        var taxes = from d in db.MstTaxTypes where d.TaxType == salesInvoiceItem.VAT select d;
                                        if (taxes.Any()) { taxExist = true; }

                                        if (unitExist)
                                        {
                                            if (discountExist)
                                            {
                                                if (taxExist)
                                                {
                                                    var conversionUnit = from d in db.MstArticleUnits
                                                                         where d.ArticleId == items.FirstOrDefault().Id
                                                                         && d.UnitId == items.FirstOrDefault().UnitId
                                                                         select d;

                                                    if (conversionUnit.Any())
                                                    {
                                                        Decimal baseQuantity = salesInvoiceItem.Quantity * 1;
                                                        if (conversionUnit.FirstOrDefault().Multiplier > 0)
                                                        {
                                                            baseQuantity = salesInvoiceItem.Quantity * (1 / conversionUnit.FirstOrDefault().Multiplier);
                                                        }

                                                        Decimal basePrice = salesInvoiceItem.Amount;
                                                        if (baseQuantity > 0)
                                                        {
                                                            basePrice = salesInvoiceItem.Amount / baseQuantity;
                                                        }

                                                        Data.TrnSalesInvoiceItem addSaleInvoiceItem = new Data.TrnSalesInvoiceItem
                                                        {
                                                            SIId = addSalesInvoice.Id,
                                                            ItemId = items.FirstOrDefault().Id,
                                                            ItemInventoryId = itemInventoryId,
                                                            Particulars = salesInvoiceItem.Particulars,
                                                            UnitId = units.FirstOrDefault().Id,
                                                            Quantity = salesInvoiceItem.Quantity,
                                                            Price = salesInvoiceItem.Price,
                                                            DiscountId = discounts.FirstOrDefault().Id,
                                                            DiscountRate = discounts.FirstOrDefault().DiscountRate,
                                                            DiscountAmount = salesInvoiceItem.DiscountAmount,
                                                            NetPrice = salesInvoiceItem.NetPrice,
                                                            Amount = salesInvoiceItem.Amount,
                                                            VATId = taxes.FirstOrDefault().Id,
                                                            VATPercentage = taxes.FirstOrDefault().TaxRate,
                                                            VATAmount = (salesInvoiceItem.Amount / (1 + (taxes.FirstOrDefault().TaxRate / 100))) * (taxes.FirstOrDefault().TaxRate / 100),
                                                            BaseUnitId = items.FirstOrDefault().UnitId,
                                                            BaseQuantity = baseQuantity,
                                                            BasePrice = basePrice,
                                                            SalesItemTimeStamp = Convert.ToDateTime(salesInvoiceItem.SalesItemTimeStamp)
                                                        };

                                                        db.TrnSalesInvoiceItems.InsertOnSubmit(addSaleInvoiceItem);
                                                        db.SubmitChanges();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                var salesInvoice = from d in db.TrnSalesInvoices where d.Id == addSalesInvoice.Id select d;
                                if (salesInvoice.Any())
                                {
                                    var salesInvoiceItems = from d in db.TrnSalesInvoiceItems where d.SIId == addSalesInvoice.Id select d;

                                    Decimal totalSalesInvoiceItemAmount = 0;
                                    if (salesInvoiceItems.Any())
                                    {
                                        totalSalesInvoiceItemAmount = salesInvoiceItems.Sum(d => d.Amount);
                                    }

                                    var updateSalesInvoiceAmount = salesInvoice.FirstOrDefault();
                                    updateSalesInvoiceAmount.Amount = totalSalesInvoiceItemAmount;
                                    updateSalesInvoiceAmount.BalanceAmount = totalSalesInvoiceItemAmount;
                                    db.SubmitChanges();
                                }

                                var salesInvoiceForBusiness = from d in db.TrnSalesInvoices where d.Id == Convert.ToInt32(addSalesInvoice.Id) select d;
                                if (salesInvoiceForBusiness.Any())
                                {
                                    inventory.InsertSalesInvoiceInventory(Convert.ToInt32(salesInvoiceForBusiness.FirstOrDefault().Id));
                                    journal.InsertSalesInvoiceJournal(Convert.ToInt32(salesInvoiceForBusiness.FirstOrDefault().Id));
                                }

                                return Request.CreateResponse(HttpStatusCode.OK, addSalesInvoice.SINumber);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.BadRequest, "Easyfis: Term Not Exist!");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Easyfis: User Not Exist!");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Easyfis: Customer Not Exist!");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Easyfis: Branch Not Exist!");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }
    }
}
