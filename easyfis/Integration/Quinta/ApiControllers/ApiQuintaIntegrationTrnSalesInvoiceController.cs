using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.Quinta.ApiControllers
{
    public class ApiQuintaIntegrationTrnSalesInvoiceController : ApiController
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

        // ========================================
        // Quinta Integration: Dropdown List (Term)
        // ========================================
        [HttpGet, Route("api/quinta/integration/salesInvoice/dropdown/term/list")]
        public List<Entities.QuintaIntegrationMstTerm> QuintaIntegrationListTerm()
        {
            var terms = from d in db.MstTerms
                        select new Entities.QuintaIntegrationMstTerm
                        {
                            Id = d.Id,
                            Term = d.Term
                        };

            return terms.OrderBy(t => t.Term).ToList();
        }

        // ============================================
        // Quinta Integration: Dropdown List (Tax Type)
        // ============================================
        [HttpGet, Route("api/quinta/integration/salesInvoice/dropdown/taxType/list")]
        public List<Entities.QuintaIntegrationMstTaxType> QuintaIntegrationListTaxType()
        {
            var taxTypes = from d in db.MstTaxTypes
                           select new Entities.QuintaIntegrationMstTaxType
                           {
                               Id = d.Id,
                               TaxType = d.TaxType
                           };

            return taxTypes.OrderBy(t => t.TaxType).ToList();
        }

        // ============================================
        // Quinta Integration: Dropdown List (Discount)
        // ============================================
        [HttpGet, Route("api/quinta/integration/salesInvoice/dropdown/discount/list")]
        public List<Entities.QuintaIntegrationMstDiscount> QuintaIntegrationListDiscount()
        {
            var discounts = from d in db.MstDiscounts
                            select new Entities.QuintaIntegrationMstDiscount
                            {
                                Id = d.Id,
                                Discount = d.Discount
                            };

            return discounts.OrderBy(t => t.Discount).ToList();
        }

        // =====================================
        // Quinta Integration: Add Sales Invoice 
        // =====================================
        [HttpPost, Route("api/quinta/integration/salesInvoice/add")]
        public List<Entities.ReturnedDocument> QuintaIntegrationAddSalesInvoice(Entities.RootObject objRoot)
        {
            try
            {
                List<Entities.ReturnedDocument> listSINumber = new List<Entities.ReturnedDocument>();

                var branch = from d in db.MstBranches
                             select d;

                if (branch.Any())
                {
                    Int32 currentBranchId = branch.FirstOrDefault().Id;

                    if (objRoot.TRN.Any())
                    {
                        foreach (var objSales in objRoot.TRN)
                        {
                            if (objSales != null)
                            {
                                Boolean unitExist = false;
                                var units = from d in db.MstUnits
                                            select d;

                                if (units.Any())
                                {
                                    unitExist = true;
                                }

                                Boolean termExist = false;
                                var terms = from d in db.MstTerms
                                            where d.Term.Equals(objRoot.DefaultTerm)
                                            select d;

                                if (terms.Any())
                                {
                                    termExist = true;
                                }

                                Boolean userExist = false;
                                var users = from d in db.MstUsers
                                            where d.UserName.Equals("admin")
                                            select d;

                                if (users.Any())
                                {
                                    userExist = true;
                                }

                                Int32 customerId = 0;
                                var customers = from d in db.MstArticles
                                                where d.ManualArticleCode.Equals(objSales.ACI)
                                                && d.ArticleTypeId == 2
                                                select d;

                                if (customers.Any())
                                {
                                    customerId = customers.FirstOrDefault().Id;
                                }
                                else
                                {
                                    var defaultCustomerCode = "0000000001";
                                    var lastCustomer = from d in db.MstArticles.OrderByDescending(d => d.Id)
                                                       where d.ArticleTypeId == 2
                                                       select d;

                                    if (lastCustomer.Any())
                                    {
                                        var customerCode = Convert.ToInt32(lastCustomer.FirstOrDefault().ArticleCode) + 0000000001;
                                        defaultCustomerCode = ZeroFill(customerCode, 10);
                                    }

                                    var articleGroups = from d in db.MstArticleGroups
                                                        where d.ArticleTypeId == 2
                                                        select d;

                                    if (articleGroups.Any())
                                    {
                                        Data.MstArticle newCustomer = new Data.MstArticle
                                        {
                                            ArticleCode = defaultCustomerCode,
                                            ManualArticleCode = objSales.ACI,
                                            Article = objSales.ACC,
                                            Category = "NA",
                                            ArticleTypeId = 2,
                                            ArticleGroupId = articleGroups.FirstOrDefault().Id,
                                            AccountId = articleGroups.FirstOrDefault().AccountId,
                                            SalesAccountId = articleGroups.FirstOrDefault().SalesAccountId,
                                            CostAccountId = articleGroups.FirstOrDefault().CostAccountId,
                                            AssetAccountId = articleGroups.FirstOrDefault().AssetAccountId,
                                            ExpenseAccountId = articleGroups.FirstOrDefault().ExpenseAccountId,
                                            UnitId = units.FirstOrDefault().Id,
                                            OutputTaxId = db.MstTaxTypes.FirstOrDefault().Id,
                                            InputTaxId = db.MstTaxTypes.FirstOrDefault().Id,
                                            WTaxTypeId = db.MstTaxTypes.FirstOrDefault().Id,
                                            Price = 0,
                                            Cost = 0,
                                            IsInventory = false,
                                            Particulars = "NA",
                                            Address = "NA",
                                            TermId = terms.FirstOrDefault().Id,
                                            ContactNumber = "NA",
                                            ContactPerson = "NA",
                                            EmailAddress = "NA",
                                            TaxNumber = "NA",
                                            CreditLimit = 0,
                                            DateAcquired = DateTime.Now,
                                            UsefulLife = 0,
                                            SalvageValue = 0,
                                            ManualArticleOldCode = "NA",
                                            Kitting = 0,
                                            IsLocked = true,
                                            CreatedById = users.FirstOrDefault().Id,
                                            CreatedDateTime = DateTime.Now,
                                            UpdatedById = users.FirstOrDefault().Id,
                                            UpdatedDateTime = DateTime.Now
                                        };

                                        db.MstArticles.InsertOnSubmit(newCustomer);
                                        db.SubmitChanges();

                                        customerId = newCustomer.Id;
                                    }
                                }

                                Boolean taxTypeExist = false;
                                var taxTypes = from d in db.MstTaxTypes
                                               select d;

                                if (taxTypes.Any())
                                {
                                    taxTypeExist = true;
                                }

                                Boolean discountExist = false;
                                var discounts = from d in db.MstDiscounts
                                                where d.Discount.Equals(objRoot.DefaultDiscount)
                                                select d;

                                if (discounts.Any())
                                {
                                    discountExist = true;
                                }

                                if (userExist)
                                {
                                    if (termExist)
                                    {
                                        if (customerId != 0)
                                        {
                                            Int32 salesInvoiceId = 0;
                                            var salesInvoice = from d in db.TrnSalesInvoices
                                                               where d.ManualSINumber.Equals(objSales.FTN)
                                                               select d;

                                            if (salesInvoice.Any())
                                            {
                                                salesInvoiceId = salesInvoice.FirstOrDefault().Id;

                                                Business.Inventory inventory = new Business.Inventory();
                                                inventory.DeleteSalesInvoiceInventory(Convert.ToInt32(salesInvoiceId));

                                                Business.Journal journal = new Business.Journal();
                                                journal.DeleteSalesInvoiceJournal(Convert.ToInt32(salesInvoiceId));

                                                listSINumber.Add(new Entities.ReturnedDocument()
                                                {
                                                    SINumber = salesInvoice.FirstOrDefault().SINumber,
                                                    ManualSINumber = salesInvoice.FirstOrDefault().ManualSINumber
                                                });
                                            }
                                            else
                                            {
                                                var SINumberResult = "0000000001";
                                                var lastSalesInvoice = from d in db.TrnSalesInvoices.OrderByDescending(d => d.Id)
                                                                       where d.BranchId == currentBranchId
                                                                       select d;

                                                if (lastSalesInvoice.Any())
                                                {
                                                    var SINumber = Convert.ToInt32(lastSalesInvoice.FirstOrDefault().SINumber) + 0000000001;
                                                    SINumberResult = ZeroFill(SINumber, 10);
                                                }

                                                // ====================
                                                // Insert Sales Invoice
                                                // ====================
                                                Data.TrnSalesInvoice addSalesInvoice = new Data.TrnSalesInvoice
                                                {
                                                    BranchId = currentBranchId,
                                                    SINumber = SINumberResult,
                                                    SIDate = DateTime.Today,
                                                    CustomerId = customerId,
                                                    TermId = terms.FirstOrDefault().Id,
                                                    DocumentReference = objSales.FTN,
                                                    ManualSINumber = objSales.FTN,
                                                    Remarks = "NA",
                                                    Amount = objSales.NAM,
                                                    PaidAmount = 0,
                                                    AdjustmentAmount = 0,
                                                    BalanceAmount = objSales.NAM,
                                                    SoldById = users.FirstOrDefault().Id,
                                                    PreparedById = users.FirstOrDefault().Id,
                                                    CheckedById = users.FirstOrDefault().Id,
                                                    ApprovedById = users.FirstOrDefault().Id,
                                                    IsLocked = true,
                                                    CreatedById = users.FirstOrDefault().Id,
                                                    CreatedDateTime = DateTime.Now,
                                                    UpdatedById = users.FirstOrDefault().Id,
                                                    UpdatedDateTime = DateTime.Now
                                                };

                                                db.TrnSalesInvoices.InsertOnSubmit(addSalesInvoice);
                                                db.SubmitChanges();

                                                salesInvoiceId = addSalesInvoice.Id;

                                                listSINumber.Add(new Entities.ReturnedDocument()
                                                {
                                                    SINumber = addSalesInvoice.SINumber,
                                                    ManualSINumber = addSalesInvoice.ManualSINumber
                                                });
                                            }

                                            if (salesInvoiceId != 0)
                                            {
                                                Int32 itemId = 0;
                                                var items = from d in db.MstArticles
                                                            where d.ManualArticleCode.Equals(objSales.SAI)
                                                            && d.ArticleTypeId == 1
                                                            select d;

                                                if (items.Any())
                                                {
                                                    itemId = items.FirstOrDefault().Id;
                                                }
                                                else
                                                {
                                                    var defaultItemCode = "0000000001";
                                                    var lastItem = from d in db.MstArticles.OrderByDescending(d => d.Id)
                                                                   where d.ArticleTypeId == 1
                                                                   select d;

                                                    if (lastItem.Any())
                                                    {
                                                        var itemCode = Convert.ToInt32(lastItem.FirstOrDefault().ArticleCode) + 0000000001;
                                                        defaultItemCode = ZeroFill(itemCode, 10);
                                                    }

                                                    var articleGroups = from d in db.MstArticleGroups
                                                                        where d.ArticleTypeId == 1
                                                                        select d;

                                                    if (articleGroups.Any())
                                                    {
                                                        Data.MstArticle newItem = new Data.MstArticle
                                                        {
                                                            ArticleCode = defaultItemCode,
                                                            ManualArticleCode = objSales.SAI,
                                                            Article = objSales.SAM,
                                                            Category = "NA",
                                                            ArticleTypeId = 1,
                                                            ArticleGroupId = articleGroups.FirstOrDefault().Id,
                                                            AccountId = articleGroups.FirstOrDefault().AccountId,
                                                            SalesAccountId = articleGroups.FirstOrDefault().SalesAccountId,
                                                            CostAccountId = articleGroups.FirstOrDefault().CostAccountId,
                                                            AssetAccountId = articleGroups.FirstOrDefault().AssetAccountId,
                                                            ExpenseAccountId = articleGroups.FirstOrDefault().ExpenseAccountId,
                                                            UnitId = units.FirstOrDefault().Id,
                                                            OutputTaxId = taxTypes.Where(d => d.TaxType.Equals(objRoot.DefaultVatOutput)).FirstOrDefault().Id,
                                                            InputTaxId = taxTypes.Where(d => d.TaxType.Equals(objRoot.DefaultVatInput)).FirstOrDefault().Id,
                                                            WTaxTypeId = taxTypes.Where(d => d.TaxType.Equals(objRoot.DefaultWTax)).FirstOrDefault().Id,
                                                            Price = objSales.NAM,
                                                            Cost = 0,
                                                            IsInventory = false,
                                                            Particulars = "NA",
                                                            Address = "NA",
                                                            TermId = terms.FirstOrDefault().Id,
                                                            ContactNumber = "NA",
                                                            ContactPerson = "NA",
                                                            EmailAddress = "NA",
                                                            TaxNumber = "NA",
                                                            CreditLimit = 0,
                                                            DateAcquired = DateTime.Now,
                                                            UsefulLife = 0,
                                                            SalvageValue = 0,
                                                            ManualArticleOldCode = "NA",
                                                            Kitting = 0,
                                                            DefaultSupplierId = null,
                                                            IsLocked = true,
                                                            CreatedById = users.FirstOrDefault().Id,
                                                            CreatedDateTime = DateTime.Now,
                                                            UpdatedById = users.FirstOrDefault().Id,
                                                            UpdatedDateTime = DateTime.Now
                                                        };

                                                        db.MstArticles.InsertOnSubmit(newItem);
                                                        db.SubmitChanges();

                                                        itemId = newItem.Id;

                                                        Data.MstArticleUnit newItemUnitConversion = new Data.MstArticleUnit
                                                        {
                                                            ArticleId = Convert.ToInt32(itemId),
                                                            UnitId = units.FirstOrDefault().Id,
                                                            Multiplier = 1,
                                                            IsCountUnit = false
                                                        };

                                                        db.MstArticleUnits.InsertOnSubmit(newItemUnitConversion);
                                                        db.SubmitChanges();

                                                        Data.MstArticlePrice newItemPrice = new Data.MstArticlePrice
                                                        {
                                                            ArticleId = Convert.ToInt32(itemId),
                                                            PriceDescription = "SRP",
                                                            Price = objSales.NAM,
                                                            Remarks = "NA"
                                                        };

                                                        db.MstArticlePrices.InsertOnSubmit(newItemPrice);
                                                        db.SubmitChanges();
                                                    }
                                                }

                                                if (itemId != 0)
                                                {
                                                    if (unitExist)
                                                    {
                                                        if (taxTypeExist)
                                                        {
                                                            if (discountExist)
                                                            {
                                                                Int32? itemInventoryId = null;
                                                                var articleInventory = from d in db.MstArticleInventories
                                                                                       where d.BranchId == currentBranchId
                                                                                       && d.ArticleId == itemId
                                                                                       select d;

                                                                if (articleInventory.Any())
                                                                {
                                                                    itemInventoryId = articleInventory.FirstOrDefault().Id;
                                                                }

                                                                var conversionUnit = from d in db.MstArticleUnits
                                                                                     where d.ArticleId == itemId
                                                                                     && d.UnitId == items.FirstOrDefault().UnitId
                                                                                     select d;

                                                                if (conversionUnit.Any())
                                                                {
                                                                    Decimal baseQuantity = 1;
                                                                    Decimal basePrice = objSales.NAM;

                                                                    if (conversionUnit.FirstOrDefault().Multiplier > 0)
                                                                    {
                                                                        baseQuantity = 1 * (1 / conversionUnit.FirstOrDefault().Multiplier);
                                                                    }

                                                                    if (baseQuantity > 0)
                                                                    {
                                                                        basePrice = objSales.NAM / baseQuantity;
                                                                    }

                                                                    Data.TrnSalesInvoiceItem newSalesInvoiceItem = new Data.TrnSalesInvoiceItem
                                                                    {
                                                                        SIId = salesInvoiceId,
                                                                        ItemId = itemId,
                                                                        ItemInventoryId = itemInventoryId,
                                                                        Particulars = "NA",
                                                                        UnitId = units.FirstOrDefault().Id,
                                                                        Quantity = 1,
                                                                        Price = objSales.NAM,
                                                                        DiscountId = discounts.FirstOrDefault().Id,
                                                                        DiscountRate = discounts.FirstOrDefault().DiscountRate,
                                                                        DiscountAmount = 0,
                                                                        NetPrice = objSales.NAM,
                                                                        Amount = objSales.NAM,
                                                                        VATId = items.FirstOrDefault().OutputTaxId,
                                                                        VATPercentage = items.FirstOrDefault().MstTaxType.TaxRate,
                                                                        VATAmount = (objSales.NAM / (1 + (items.FirstOrDefault().MstTaxType.TaxRate / 100))) * (items.FirstOrDefault().MstTaxType.TaxRate / 100),
                                                                        BaseUnitId = items.FirstOrDefault().UnitId,
                                                                        BaseQuantity = baseQuantity,
                                                                        BasePrice = basePrice,
                                                                        SalesItemTimeStamp = DateTime.Now
                                                                    };

                                                                    db.TrnSalesInvoiceItems.InsertOnSubmit(newSalesInvoiceItem);
                                                                    db.SubmitChanges();
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                var currentSalesInvoice = from d in db.TrnSalesInvoices
                                                                          where d.Id == salesInvoiceId
                                                                          select d;

                                                if (currentSalesInvoice.Any())
                                                {
                                                    var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                                                            where d.SIId == salesInvoiceId
                                                                            select d;

                                                    Decimal salesAmount = 0;
                                                    Decimal paidAmount = currentSalesInvoice.FirstOrDefault().PaidAmount;
                                                    Decimal adjustmentAmount = currentSalesInvoice.FirstOrDefault().AdjustmentAmount;

                                                    if (salesInvoiceItems.Any())
                                                    {
                                                        salesAmount = salesInvoiceItems.Sum(d => d.Amount);
                                                    }

                                                    var updateSalesInvoiceAmount = currentSalesInvoice.FirstOrDefault();
                                                    updateSalesInvoiceAmount.Amount = salesAmount;
                                                    updateSalesInvoiceAmount.BalanceAmount = (salesAmount - paidAmount) + adjustmentAmount;

                                                    db.SubmitChanges();
                                                }

                                                inventory.InsertSalesInvoiceInventory(Convert.ToInt32(salesInvoiceId));
                                                journal.InsertSalesInvoiceJournal(Convert.ToInt32(salesInvoiceId));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return listSINumber.ToList();
                }
                else
                {
                    return new List<Entities.ReturnedDocument>();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return new List<Entities.ReturnedDocument>();
            }
        }
    }
}
