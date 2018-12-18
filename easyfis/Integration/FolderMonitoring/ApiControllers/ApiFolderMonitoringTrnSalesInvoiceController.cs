using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.FolderMonitoring.ApiControllers
{
    public class ApiFolderMonitoringTrnSalesInvoiceController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========
        // Business
        // ========
        private Business.AccountsReceivable accountsReceivable = new Business.AccountsReceivable();
        private Business.Inventory inventory = new Business.Inventory();
        private Business.Journal journal = new Business.Journal();

        // ===================
        // Fill Leading Zeroes
        // ===================
        public String FillLeadingZeroes(Int32 number, Int32 length)
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

        // ==================
        // Compute VAT Amount
        // ==================
        public Decimal ComputeVATAmount(Boolean IsInclusive, Decimal Amount, Decimal VATRate)
        {
            if (IsInclusive)
            {
                return (Amount / (1 + VATRate / 100)) * (VATRate / 100);
            }
            else
            {
                return Amount * (VATRate / 100); ;
            }
        }

        // ===================================
        // Add Folder Monitoring Sales Invoice
        // ===================================
        [HttpPost, Route("api/folderMonitoring/salesInvoice/add")]
        public HttpResponseMessage AddFolderMonitoringSalesInvoice(List<Entities.FolderMonitoringTrnSalesInvoice> folderMonitoringTrnSalesInvoiceObjects)
        {
            try
            {
                if (folderMonitoringTrnSalesInvoiceObjects.Any())
                {
                    foreach (var folderMonitoringTrnSalesInvoiceObject in folderMonitoringTrnSalesInvoiceObjects)
                    {
                        Boolean isBranchExist = false,
                                isCustomerExist = false,
                                isTermExist = false,
                                isUserExist = false,
                                isItemExist = false,
                                isDiscountExist = false;

                        var branch = from d in db.MstBranches where d.BranchCode.Equals(folderMonitoringTrnSalesInvoiceObject.BranchCode) select d;
                        if (branch.Any()) { isBranchExist = true; }

                        var customer = from d in db.MstArticles where d.ArticleTypeId == 2 && d.ManualArticleCode.Equals(folderMonitoringTrnSalesInvoiceObject.CustomerCode) && d.IsLocked == true select d;
                        if (customer.Any()) { isCustomerExist = true; }

                        var term = from d in db.MstTerms where d.Term.Equals(folderMonitoringTrnSalesInvoiceObject.Term) select d;
                        if (term.Any()) { isTermExist = true; }

                        var user = from d in db.MstUsers where d.UserName.Equals(folderMonitoringTrnSalesInvoiceObject.UserCode) select d;
                        if (user.Any()) { isUserExist = true; }

                        var item = from d in db.MstArticles where d.ArticleTypeId == 1 && d.ManualArticleCode.Equals(folderMonitoringTrnSalesInvoiceObject.ItemCode) && d.IsLocked == true select d;
                        if (item.Any()) { isItemExist = true; }

                        var discount = from d in db.MstDiscounts where d.Discount.Equals(folderMonitoringTrnSalesInvoiceObject.Discount) select d;
                        if (discount.Any()) { isDiscountExist = true; }

                        if (isBranchExist && isCustomerExist && isTermExist && isUserExist && isItemExist && isDiscountExist)
                        {
                            Int32 SIId = 0;

                            var currentSalesInvoice = from d in db.TrnSalesInvoices where d.BranchId == branch.FirstOrDefault().Id && d.ManualSINumber.Equals(folderMonitoringTrnSalesInvoiceObject.ManualSINumber) && d.IsLocked == true select d;
                            if (currentSalesInvoice.Any())
                            {
                                SIId = currentSalesInvoice.FirstOrDefault().Id;

                                var unlockSalesInvoice = currentSalesInvoice.FirstOrDefault();
                                unlockSalesInvoice.IsLocked = false;
                                unlockSalesInvoice.UpdatedById = user.FirstOrDefault().Id;
                                unlockSalesInvoice.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnSalesInvoiceObject.CreatedDateTime);
                                db.SubmitChanges();

                                inventory.DeleteSalesInvoiceInventory(SIId);
                                journal.DeleteSalesInvoiceJournal(SIId);
                            }
                            else
                            {
                                var defaultSINumber = "0000000001";
                                var lastSalesInvoice = from d in db.TrnSalesInvoices.OrderByDescending(d => d.Id) where d.BranchId == branch.FirstOrDefault().Id select d;
                                if (lastSalesInvoice.Any())
                                {
                                    var SINumber = Convert.ToInt32(lastSalesInvoice.FirstOrDefault().SINumber) + 0000000001;
                                    defaultSINumber = FillLeadingZeroes(SINumber, 10);
                                }

                                Data.TrnSalesInvoice newSalesInvoice = new Data.TrnSalesInvoice
                                {
                                    BranchId = branch.FirstOrDefault().Id,
                                    SINumber = defaultSINumber,
                                    SIDate = Convert.ToDateTime(folderMonitoringTrnSalesInvoiceObject.SIDate),
                                    DocumentReference = folderMonitoringTrnSalesInvoiceObject.DocumentReference,
                                    CustomerId = customer.FirstOrDefault().Id,
                                    TermId = term.FirstOrDefault().Id,
                                    Remarks = folderMonitoringTrnSalesInvoiceObject.Remarks,
                                    ManualSINumber = folderMonitoringTrnSalesInvoiceObject.ManualSINumber,
                                    Amount = 0,
                                    PaidAmount = 0,
                                    AdjustmentAmount = 0,
                                    BalanceAmount = 0,
                                    SoldById = user.FirstOrDefault().Id,
                                    PreparedById = user.FirstOrDefault().Id,
                                    CheckedById = user.FirstOrDefault().Id,
                                    ApprovedById = user.FirstOrDefault().Id,
                                    Status = null,
                                    IsCancelled = false,
                                    IsPrinted = false,
                                    IsLocked = false,
                                    CreatedById = user.FirstOrDefault().Id,
                                    CreatedDateTime = Convert.ToDateTime(folderMonitoringTrnSalesInvoiceObject.CreatedDateTime),
                                    UpdatedById = user.FirstOrDefault().Id,
                                    UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnSalesInvoiceObject.CreatedDateTime)
                                };

                                db.TrnSalesInvoices.InsertOnSubmit(newSalesInvoice);
                                db.SubmitChanges();

                                SIId = newSalesInvoice.Id;
                            }

                            var unitConversion = from d in item.FirstOrDefault().MstArticleUnits where d.MstUnit.Unit.Equals(folderMonitoringTrnSalesInvoiceObject.Unit) select d;
                            if (unitConversion.Any())
                            {
                                Decimal baseQuantity = folderMonitoringTrnSalesInvoiceObject.Quantity * 1;
                                if (unitConversion.FirstOrDefault().Multiplier > 0)
                                {
                                    baseQuantity = folderMonitoringTrnSalesInvoiceObject.Quantity * (1 / unitConversion.FirstOrDefault().Multiplier);
                                }

                                Decimal basePrice = folderMonitoringTrnSalesInvoiceObject.Amount;
                                if (baseQuantity > 0)
                                {
                                    basePrice = folderMonitoringTrnSalesInvoiceObject.Amount / baseQuantity;
                                }

                                Int32? itemInventoryId = null;
                                Boolean isValid = false;

                                if (item.FirstOrDefault().IsInventory)
                                {
                                    var itemInventory = from d in db.MstArticleInventories where d.BranchId == branch.FirstOrDefault().Id && d.ArticleId == item.FirstOrDefault().Id select d;
                                    if (itemInventory.Any()) { itemInventoryId = itemInventory.FirstOrDefault().Id; isValid = true; }
                                }
                                else
                                {
                                    isValid = true;
                                }

                                if (isValid)
                                {
                                    Data.TrnSalesInvoiceItem newSalesInvoiceItem = new Data.TrnSalesInvoiceItem
                                    {
                                        SIId = SIId,
                                        ItemId = item.FirstOrDefault().Id,
                                        ItemInventoryId = itemInventoryId,
                                        Particulars = folderMonitoringTrnSalesInvoiceObject.Particulars,
                                        UnitId = unitConversion.FirstOrDefault().UnitId,
                                        Quantity = folderMonitoringTrnSalesInvoiceObject.Quantity,
                                        Price = folderMonitoringTrnSalesInvoiceObject.Price,
                                        DiscountId = discount.FirstOrDefault().Id,
                                        DiscountRate = folderMonitoringTrnSalesInvoiceObject.DiscountRate,
                                        DiscountAmount = folderMonitoringTrnSalesInvoiceObject.DiscountAmount,
                                        NetPrice = folderMonitoringTrnSalesInvoiceObject.NetPrice,
                                        Amount = folderMonitoringTrnSalesInvoiceObject.Amount,
                                        VATId = item.FirstOrDefault().OutputTaxId,
                                        VATPercentage = item.FirstOrDefault().MstTaxType.TaxRate,
                                        VATAmount = ComputeVATAmount(item.FirstOrDefault().MstTaxType.IsInclusive, folderMonitoringTrnSalesInvoiceObject.Amount, item.FirstOrDefault().MstTaxType.TaxRate),
                                        BaseUnitId = item.FirstOrDefault().UnitId,
                                        BaseQuantity = baseQuantity,
                                        BasePrice = basePrice,
                                        SalesItemTimeStamp = DateTime.Now
                                    };

                                    db.TrnSalesInvoiceItems.InsertOnSubmit(newSalesInvoiceItem);
                                    db.SubmitChanges();

                                    var itemComponents = from d in item.FirstOrDefault().MstArticleComponents select d;
                                    if (itemComponents.Any())
                                    {
                                        foreach (var itemComponent in itemComponents)
                                        {
                                            var itemComponentUnitConversion = from d in itemComponent.MstArticle1.MstArticleUnits where d.UnitId == itemComponent.MstArticle1.UnitId select d;
                                            if (itemComponentUnitConversion.Any())
                                            {
                                                Decimal itemComponentDiscountAmount = 0 * (folderMonitoringTrnSalesInvoiceObject.DiscountRate / 100);
                                                Decimal itemComponentNetPrice = 0 - itemComponentDiscountAmount;

                                                if (discount.FirstOrDefault().IsInclusive)
                                                {
                                                    Decimal price = 0 / (1 + (itemComponent.MstArticle1.MstTaxType.TaxRate / 100));

                                                    itemComponentDiscountAmount = price * (folderMonitoringTrnSalesInvoiceObject.DiscountRate / 100);
                                                    itemComponentNetPrice = price - itemComponentDiscountAmount;
                                                }

                                                Decimal itemComponentQuantity = itemComponent.Quantity * folderMonitoringTrnSalesInvoiceObject.Quantity;
                                                Decimal itemComponentAmount = itemComponentNetPrice * itemComponentQuantity;

                                                Decimal componentItemBaseQuantity = itemComponentQuantity * 1;
                                                if (itemComponentUnitConversion.FirstOrDefault().Multiplier > 0)
                                                {
                                                    componentItemBaseQuantity = itemComponentQuantity * (1 / itemComponentUnitConversion.FirstOrDefault().Multiplier);
                                                }

                                                Decimal componentItemBasePrice = itemComponentAmount;
                                                if (baseQuantity > 0)
                                                {
                                                    componentItemBasePrice = itemComponentAmount / componentItemBaseQuantity;
                                                }

                                                Int32? itemComponentInventoryId = null;

                                                var itemComponentInventory = from d in itemComponent.MstArticle1.MstArticleInventories where d.BranchId == branch.FirstOrDefault().Id && d.ArticleId == itemComponent.ArticleId select d;
                                                if (itemComponentInventory.Any()) { itemComponentInventoryId = itemComponentInventory.FirstOrDefault().Id; }

                                                Data.TrnSalesInvoiceItem newComponentSalesInvoiceItem = new Data.TrnSalesInvoiceItem
                                                {
                                                    SIId = SIId,
                                                    ItemId = itemComponent.ComponentArticleId,
                                                    ItemInventoryId = itemComponentInventoryId,
                                                    Particulars = itemComponent.Particulars,
                                                    UnitId = itemComponentUnitConversion.FirstOrDefault().UnitId,
                                                    Quantity = itemComponentQuantity,
                                                    Price = 0,
                                                    DiscountId = discount.FirstOrDefault().Id,
                                                    DiscountRate = folderMonitoringTrnSalesInvoiceObject.DiscountRate,
                                                    DiscountAmount = itemComponentDiscountAmount,
                                                    NetPrice = itemComponentNetPrice,
                                                    Amount = itemComponentAmount,
                                                    VATId = itemComponent.MstArticle1.MstTaxType.Id,
                                                    VATPercentage = itemComponent.MstArticle1.MstTaxType.TaxRate,
                                                    VATAmount = ComputeVATAmount(itemComponent.MstArticle1.MstTaxType.IsInclusive, itemComponentAmount, itemComponent.MstArticle1.MstTaxType.TaxRate),
                                                    BaseUnitId = itemComponent.MstArticle1.UnitId,
                                                    BaseQuantity = componentItemBaseQuantity,
                                                    BasePrice = componentItemBasePrice,
                                                    SalesItemTimeStamp = DateTime.Now
                                                };

                                                db.TrnSalesInvoiceItems.InsertOnSubmit(newComponentSalesInvoiceItem);
                                                db.SubmitChanges();
                                            }
                                        }
                                    }
                                }

                                var salesInvoice = from d in db.TrnSalesInvoices where d.Id == SIId && d.IsLocked == false select d;
                                if (salesInvoice.Any())
                                {
                                    Decimal amount = 0;
                                    var salesInvoiceItems = from d in db.TrnSalesInvoiceItems where d.SIId == SIId select d;
                                    if (salesInvoiceItems.Any()) { amount = salesInvoiceItems.Sum(d => d.Amount); }

                                    var lockSalesInvoice = salesInvoice.FirstOrDefault();
                                    lockSalesInvoice.Amount = amount;
                                    lockSalesInvoice.IsLocked = true;
                                    lockSalesInvoice.UpdatedById = user.FirstOrDefault().Id;
                                    lockSalesInvoice.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnSalesInvoiceObject.CreatedDateTime);
                                    db.SubmitChanges();

                                    accountsReceivable.UpdateAccountsReceivable(SIId);
                                    inventory.InsertSalesInvoiceInventory(SIId);
                                    journal.InsertSalesInvoiceJournal(SIId);
                                }
                            }
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No data found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }
    }
}