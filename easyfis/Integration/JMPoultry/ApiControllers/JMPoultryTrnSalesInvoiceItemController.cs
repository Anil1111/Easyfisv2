using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;

namespace easyfis.Integration.JMPoultry.ApiControllers
{
    public class JMPoultryIntegrationTrnSalesInvoiceItemController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =======================================
        // Add Sales Invoice Item (CSV Integrator)
        // =======================================
        [HttpPost, Route("api/add/CSVIntegrator/salesInvoiceItem")]
        public HttpResponseMessage AddCSVIntegratorSalesInvoiceItem(List<Entities.JMPoultryIntegrationTrnSalesInvoiceItem> objSalesInvoiceItems)
        {
            try
            {
                if (objSalesInvoiceItems.Any())
                {
                    List<Int32> listSalesInvoiceIds = new List<Int32>();

                    foreach (var objSalesInvoiceItem in objSalesInvoiceItems)
                    {
                        Boolean salesInvoiceExists = false;
                        var salesInvoice = from d in db.TrnSalesInvoices where d.MstBranch.BranchCode.Equals(objSalesInvoiceItem.BranchCode) && d.ManualSINumber.Equals(objSalesInvoiceItem.ManualSINumber) select d;
                        if (salesInvoice.Any())
                        {
                            listSalesInvoiceIds.Add(salesInvoice.FirstOrDefault().Id);
                            salesInvoiceExists = true;
                        }

                        Boolean itemExists = false;
                        var items = from d in db.MstArticles where d.ManualArticleCode.Equals(objSalesInvoiceItem.ItemCode) && d.ArticleTypeId == 1 && d.IsLocked == true select d;
                        if (items.Any())
                        {
                            itemExists = true;
                        }

                        Boolean unitExist = false;
                        var units = from d in db.MstUnits where d.Unit == objSalesInvoiceItem.Unit select d;
                        if (units.Any())
                        {
                            unitExist = true;
                        }

                        Boolean taxTypesExist = false;
                        var taxTypes = from d in db.MstTaxTypes where d.TaxType == objSalesInvoiceItem.VAT && d.IsLocked == true select d;
                        if (taxTypes.Any())
                        {
                            taxTypesExist = true;
                        }

                        Boolean discountsExist = false;
                        var discounts = from d in db.MstDiscounts where d.Discount == objSalesInvoiceItem.Discount && d.IsLocked == true select d;
                        if (discounts.Any())
                        {
                            discountsExist = true;
                        }

                        Boolean isValid = false;
                        if (salesInvoiceExists)
                        {
                            if (itemExists)
                            {
                                if (unitExist)
                                {
                                    if (taxTypesExist)
                                    {
                                        if (discountsExist)
                                        {
                                            if (!salesInvoice.FirstOrDefault().IsLocked)
                                            {
                                                isValid = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (isValid)
                        {
                            Int32? itemInventoryId = null;
                            var articleInventory = from d in db.MstArticleInventories where d.MstBranch.BranchCode.Equals(objSalesInvoiceItem.BranchCode) && d.ArticleId == items.FirstOrDefault().Id select d;
                            if (articleInventory.Any())
                            {
                                itemInventoryId = articleInventory.FirstOrDefault().Id;
                            }

                            // ===============
                            // Package Kitting
                            // ===============
                            if (items.FirstOrDefault().Kitting == 2)
                            {
                                var packageConversionUnit = from d in db.MstArticleUnits where d.ArticleId == items.FirstOrDefault().Id && d.UnitId == items.FirstOrDefault().UnitId select d;
                                if (packageConversionUnit.Any())
                                {
                                    Decimal baseQuantity = objSalesInvoiceItem.Quantity * 1;
                                    Decimal basePrice = objSalesInvoiceItem.Amount;

                                    if (packageConversionUnit.FirstOrDefault().Multiplier > 0)
                                    {
                                        baseQuantity = objSalesInvoiceItem.Quantity * (1 / packageConversionUnit.FirstOrDefault().Multiplier);
                                    }

                                    if (baseQuantity > 0)
                                    {
                                        basePrice = objSalesInvoiceItem.Amount / baseQuantity;
                                    }

                                    Data.TrnSalesInvoiceItem addSaleInvoiceItemPackage = new Data.TrnSalesInvoiceItem
                                    {
                                        SIId = salesInvoice.FirstOrDefault().Id,
                                        ItemId = items.FirstOrDefault().Id,
                                        ItemInventoryId = itemInventoryId,
                                        Particulars = objSalesInvoiceItem.Particulars,
                                        UnitId = units.FirstOrDefault().Id,
                                        Quantity = objSalesInvoiceItem.Quantity,
                                        Price = objSalesInvoiceItem.Price,
                                        DiscountId = discounts.FirstOrDefault().Id,
                                        DiscountRate = discounts.FirstOrDefault().DiscountRate,
                                        DiscountAmount = objSalesInvoiceItem.DiscountAmount,
                                        NetPrice = objSalesInvoiceItem.NetPrice,
                                        Amount = objSalesInvoiceItem.Amount,
                                        VATId = taxTypes.FirstOrDefault().Id,
                                        VATPercentage = taxTypes.FirstOrDefault().TaxRate,
                                        VATAmount = (objSalesInvoiceItem.Amount / (1 + (taxTypes.FirstOrDefault().TaxRate / 100))) * (taxTypes.FirstOrDefault().TaxRate / 100),
                                        BaseUnitId = items.FirstOrDefault().UnitId,
                                        BaseQuantity = baseQuantity,
                                        BasePrice = basePrice,
                                        SalesItemTimeStamp = Convert.ToDateTime(objSalesInvoiceItem.SalesItemTimeStamp)
                                    };

                                    db.TrnSalesInvoiceItems.InsertOnSubmit(addSaleInvoiceItemPackage);

                                    var articleComponents = from d in db.MstArticleComponents where d.MstArticle.ManualArticleCode == objSalesInvoiceItem.ItemCode select d;
                                    if (articleComponents.Any())
                                    {
                                        foreach (var articleComponent in articleComponents)
                                        {
                                            Decimal salesInvoiceItemDiscountAmount = 0 * (discounts.FirstOrDefault().DiscountRate / 100);
                                            Decimal salesInvoiceItemNetPrice = 0 - (0 * (discounts.FirstOrDefault().DiscountRate / 100));

                                            var discount = from d in db.MstDiscounts
                                                           where d.Id == discounts.FirstOrDefault().Id
                                                           select d;

                                            if (discount.Any())
                                            {
                                                if (!discount.FirstOrDefault().IsInclusive)
                                                {
                                                    var price = 0 / (1 + (taxTypes.FirstOrDefault().TaxRate / 100));
                                                    salesInvoiceItemDiscountAmount = price * (discounts.FirstOrDefault().DiscountRate / 100);
                                                    salesInvoiceItemNetPrice = price - (price * (discounts.FirstOrDefault().DiscountRate / 100));
                                                }
                                            }

                                            Decimal quantity = articleComponent.Quantity * objSalesInvoiceItem.Quantity;
                                            Decimal amount = quantity * salesInvoiceItemNetPrice;
                                            Decimal VATAmount = amount * (taxTypes.FirstOrDefault().TaxRate / 100);

                                            var taxTypeTAXIsInclusive = from d in db.MstTaxTypes where d.Id == taxTypes.FirstOrDefault().Id select d;
                                            if (taxTypeTAXIsInclusive.Any())
                                            {
                                                if (taxTypeTAXIsInclusive.FirstOrDefault().IsInclusive)
                                                {
                                                    VATAmount = amount / (1 + (taxTypes.FirstOrDefault().TaxRate / 100)) * (taxTypes.FirstOrDefault().TaxRate / 100);
                                                }
                                            }

                                            Int32? componentItemInventoryId = null;
                                            Boolean isValidComponentItem = false;

                                            if (articleComponent.MstArticle1.IsInventory)
                                            {
                                                var componentArticleInventory = from d in db.MstArticleInventories where d.MstBranch.BranchCode.Equals(objSalesInvoiceItem.BranchCode) && d.ArticleId == articleComponent.ComponentArticleId select d;
                                                if (componentArticleInventory.Any())
                                                {
                                                    componentItemInventoryId = componentArticleInventory.FirstOrDefault().Id;
                                                    isValidComponentItem = true;
                                                }
                                            }
                                            else
                                            {
                                                isValidComponentItem = true;
                                            }

                                            if (isValidComponentItem)
                                            {
                                                var componentItem = from d in db.MstArticles where d.Id == articleComponent.ComponentArticleId select d;
                                                if (componentItem.Any())
                                                {
                                                    var componentItemConversionUnit = from d in db.MstArticleUnits where d.ArticleId == articleComponent.ComponentArticleId && d.UnitId == articleComponent.MstArticle1.UnitId select d;
                                                    if (componentItemConversionUnit.Any())
                                                    {
                                                        Decimal componentBaseQuantity = (articleComponent.Quantity * objSalesInvoiceItem.Quantity) * 1;
                                                        if (componentItemConversionUnit.FirstOrDefault().Multiplier > 0)
                                                        {
                                                            componentBaseQuantity = (articleComponent.Quantity * objSalesInvoiceItem.Quantity) * (1 / componentItemConversionUnit.FirstOrDefault().Multiplier);
                                                        }

                                                        Decimal componentBasePrice = amount;
                                                        if (baseQuantity > 0)
                                                        {
                                                            componentBasePrice = amount / baseQuantity;
                                                        }

                                                        Data.TrnSalesInvoiceItem addSaleInvoiceItem = new Data.TrnSalesInvoiceItem
                                                        {
                                                            SIId = salesInvoice.FirstOrDefault().Id,
                                                            ItemId = articleComponent.ComponentArticleId,
                                                            ItemInventoryId = componentItemInventoryId,
                                                            Particulars = articleComponent.Particulars,
                                                            UnitId = articleComponent.MstArticle1.UnitId,
                                                            Quantity = articleComponent.Quantity * objSalesInvoiceItem.Quantity,
                                                            Price = 0,
                                                            DiscountId = discounts.FirstOrDefault().Id,
                                                            DiscountRate = discounts.FirstOrDefault().DiscountRate,
                                                            DiscountAmount = salesInvoiceItemDiscountAmount,
                                                            NetPrice = salesInvoiceItemNetPrice,
                                                            Amount = amount,
                                                            VATId = taxTypes.FirstOrDefault().Id,
                                                            VATPercentage = taxTypes.FirstOrDefault().TaxRate,
                                                            VATAmount = VATAmount,
                                                            BaseUnitId = componentItem.FirstOrDefault().UnitId,
                                                            BaseQuantity = componentBaseQuantity,
                                                            BasePrice = componentBasePrice,
                                                            SalesItemTimeStamp = Convert.ToDateTime(objSalesInvoiceItem.SalesItemTimeStamp)
                                                        };

                                                        db.TrnSalesInvoiceItems.InsertOnSubmit(addSaleInvoiceItem);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    Decimal salesInvoiceItemTotalAmount = 0;

                                    if (salesInvoice.FirstOrDefault().TrnSalesInvoiceItems.Any())
                                    {
                                        salesInvoiceItemTotalAmount = salesInvoice.FirstOrDefault().TrnSalesInvoiceItems.Sum(d => d.Amount);
                                    }

                                    var updateSalesInvoiceAmount = salesInvoice.FirstOrDefault();
                                    updateSalesInvoiceAmount.Amount = salesInvoiceItemTotalAmount;
                                }
                            }
                            else
                            {
                                // ==================
                                // Main Selected Item
                                // ==================
                                var conversionUnit = from d in db.MstArticleUnits where d.ArticleId == items.FirstOrDefault().Id && d.UnitId == items.FirstOrDefault().UnitId select d;
                                if (conversionUnit.Any())
                                {
                                    Decimal baseQuantity = objSalesInvoiceItem.Quantity * 1;
                                    if (conversionUnit.FirstOrDefault().Multiplier > 0)
                                    {
                                        baseQuantity = objSalesInvoiceItem.Quantity * (1 / conversionUnit.FirstOrDefault().Multiplier);
                                    }

                                    Decimal basePrice = objSalesInvoiceItem.Amount;
                                    if (baseQuantity > 0)
                                    {
                                        basePrice = objSalesInvoiceItem.Amount / baseQuantity;
                                    }

                                    Data.TrnSalesInvoiceItem addSaleInvoiceItem = new Data.TrnSalesInvoiceItem
                                    {
                                        SIId = salesInvoice.FirstOrDefault().Id,
                                        ItemId = items.FirstOrDefault().Id,
                                        ItemInventoryId = itemInventoryId,
                                        Particulars = objSalesInvoiceItem.Particulars,
                                        UnitId = units.FirstOrDefault().Id,
                                        Quantity = objSalesInvoiceItem.Quantity,
                                        Price = objSalesInvoiceItem.Price,
                                        DiscountId = discounts.FirstOrDefault().Id,
                                        DiscountRate = discounts.FirstOrDefault().DiscountRate,
                                        DiscountAmount = objSalesInvoiceItem.DiscountAmount,
                                        NetPrice = objSalesInvoiceItem.NetPrice,
                                        Amount = objSalesInvoiceItem.Amount,
                                        VATId = taxTypes.FirstOrDefault().Id,
                                        VATPercentage = taxTypes.FirstOrDefault().TaxRate,
                                        VATAmount = (objSalesInvoiceItem.Amount / (1 + (taxTypes.FirstOrDefault().TaxRate / 100))) * (taxTypes.FirstOrDefault().TaxRate / 100),
                                        BaseUnitId = items.FirstOrDefault().UnitId,
                                        BaseQuantity = baseQuantity,
                                        BasePrice = basePrice,
                                        SalesItemTimeStamp = Convert.ToDateTime(objSalesInvoiceItem.SalesItemTimeStamp)
                                    };

                                    db.TrnSalesInvoiceItems.InsertOnSubmit(addSaleInvoiceItem);

                                    Decimal salesInvoiceItemTotalAmount = 0;

                                    if (salesInvoice.FirstOrDefault().TrnSalesInvoiceItems.Any())
                                    {
                                        salesInvoiceItemTotalAmount = salesInvoice.FirstOrDefault().TrnSalesInvoiceItems.Sum(d => d.Amount);
                                    }

                                    var updateSalesInvoiceAmount = salesInvoice.FirstOrDefault();
                                    updateSalesInvoiceAmount.Amount = salesInvoiceItemTotalAmount;
                                }
                            }
                        }
                    }

                    db.SubmitChanges();

                    if (listSalesInvoiceIds.Any())
                    {
                        var salesInvoiceIds = from d in listSalesInvoiceIds
                                              group d by d into g
                                              select g.Key;

                        if (salesInvoiceIds.Any())
                        {
                            foreach (var salesInvoiceId in salesInvoiceIds)
                            {
                                LockCSVIntegratorSalesInvoice(salesInvoiceId);
                            }
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, "Sent Successful!");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Empty!");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // ========================
        // Get Sales Invoice Amount
        // ========================
        public Decimal GetSalesInvoiceAmount(Int32 SIId)
        {
            var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                    where d.SIId == SIId
                                    select d;

            if (salesInvoiceItems.Any())
            {
                return salesInvoiceItems.Sum(d => d.Amount);
            }
            else
            {
                return 0;
            }
        }

        // ===================================
        // Lock Sales Invoice (CSV Integrator)
        // ===================================
        public void LockCSVIntegratorSalesInvoice(Int32 id)
        {
            try
            {
                var salesInvoice = from d in db.TrnSalesInvoices where d.Id == id select d;
                if (salesInvoice.Any())
                {
                    Decimal paidAmount = 0;
                    Decimal adjustmentAmount = 0;

                    var collectionLines = from d in db.TrnCollectionLines where d.SIId == id && d.TrnCollection.IsLocked == true select d;
                    if (collectionLines.Any())
                    {
                        paidAmount = collectionLines.Sum(d => d.Amount);
                    }

                    var journalVoucherLines = from d in db.TrnJournalVoucherLines where d.ARSIId == id && d.TrnJournalVoucher.IsLocked == true select d;
                    if (journalVoucherLines.Any())
                    {
                        Decimal debitAmount = journalVoucherLines.Sum(d => d.DebitAmount);
                        Decimal creditAmount = journalVoucherLines.Sum(d => d.CreditAmount);

                        adjustmentAmount = debitAmount - creditAmount;
                    }

                    var lockSalesInvoice = salesInvoice.FirstOrDefault();
                    lockSalesInvoice.Amount = GetSalesInvoiceAmount(id);
                    lockSalesInvoice.PaidAmount = paidAmount;
                    lockSalesInvoice.AdjustmentAmount = adjustmentAmount;
                    lockSalesInvoice.BalanceAmount = (GetSalesInvoiceAmount(id) - paidAmount) + adjustmentAmount;
                    lockSalesInvoice.IsLocked = true;
                    db.SubmitChanges();

                    Business.Inventory inventory = new Business.Inventory();
                    inventory.InsertSalesInvoiceInventory(id);

                    Business.Journal journal = new Business.Journal();
                    journal.InsertSalesInvoiceJournal(id);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}