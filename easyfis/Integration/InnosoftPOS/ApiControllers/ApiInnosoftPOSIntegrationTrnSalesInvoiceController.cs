using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.InnosoftPOS.ApiControllers
{
    public class ApiInnosoftPOSIntegrationTrnSalesInvoiceController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

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

        // ============================================
        // Add Sales Invoice (Innosoft POS Integration)
        // ============================================
        [HttpGet, Route("api/innosoftPOSIntegration/salesInvoice/add")]
        public HttpResponseMessage AddInnosoftPOSIntegrationSalesInvoice(Entities.InnosoftPOSIntegrationTrnSalesInvoice objSalesInvoice)
        {
            try
            {
                Boolean isHeaderValid = false;
                String returnErrorMessage = "";

                var branch = from d in db.MstBranches where d.BranchCode.Equals(objSalesInvoice.BranchCode) select d;
                var customer = from d in db.MstArticles where d.ManualArticleCode.Equals(objSalesInvoice.ManualCustomerCode) && d.ArticleTypeId == 2 select d;
                var term = from d in db.MstTerms where d.Term.Equals(objSalesInvoice.Term) select d;
                var users = from d in db.MstUsers where d.UserName.Equals(objSalesInvoice.CreatedBy) select d;

                if (!branch.Any())
                {
                    returnErrorMessage = "Branch not exist!";
                }
                else if (!customer.Any())
                {
                    returnErrorMessage = "Customer not exist!";
                }
                else if (!term.Any())
                {
                    returnErrorMessage = "Term not exist!";
                }
                else if (!users.Any())
                {
                    returnErrorMessage = "User not exist!";
                }
                else
                {
                    isHeaderValid = true;
                }

                if (isHeaderValid)
                {
                    var defaultSINumber = "0000000001";
                    var lastSalesInvoice = from d in db.TrnSalesInvoices.OrderByDescending(d => d.Id)
                                           where d.BranchId == branch.FirstOrDefault().Id
                                           select d;

                    if (lastSalesInvoice.Any())
                    {
                        var newSINumber = Convert.ToInt32(lastSalesInvoice.FirstOrDefault().SINumber) + 0000000001;
                        defaultSINumber = FillLeadingZeroes(newSINumber, 10);
                    }

                    Data.TrnSalesInvoice addSalesInvoice = new Data.TrnSalesInvoice
                    {
                        BranchId = branch.FirstOrDefault().Id,
                        SINumber = defaultSINumber,
                        SIDate = Convert.ToDateTime(objSalesInvoice.SIDate),
                        CustomerId = customer.FirstOrDefault().Id,
                        TermId = term.FirstOrDefault().Id,
                        DocumentReference = objSalesInvoice.DocumentReference,
                        ManualSINumber = objSalesInvoice.ManualSINumber,
                        Remarks = objSalesInvoice.Remarks,
                        Amount = objSalesInvoice.Amount,
                        PaidAmount = 0,
                        AdjustmentAmount = 0,
                        BalanceAmount = objSalesInvoice.Amount,
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

                    if (objSalesInvoice.ListSalesInvoiceItem.Any())
                    {
                        foreach (var objSalesInvoiceItem in objSalesInvoice.ListSalesInvoiceItem.ToList())
                        {
                            Boolean isLineValid = true;
                            Int32? itemInventoryId = null;

                            var item = from d in db.MstArticles where d.ManualArticleCode.Equals(objSalesInvoiceItem.ManualItemCode) && d.ArticleTypeId == 1 select d;
                            var unit = from d in db.MstUnits where d.Unit.Equals(objSalesInvoiceItem.Unit) select d;
                            var discount = from d in db.MstDiscounts where d.Discount.Equals(objSalesInvoiceItem.Discount) select d;
                            var tax = from d in db.MstTaxTypes where d.TaxType.Equals(objSalesInvoiceItem.VAT) select d;

                            if (!item.Any())
                            {
                                isLineValid = false;
                            }
                            else if (!unit.Any())
                            {
                                isLineValid = false;
                            }
                            else if (!discount.Any())
                            {
                                isLineValid = false;
                            }
                            else if (!tax.Any())
                            {
                                isLineValid = false;
                            }
                            else
                            {
                                var articleInventory = from d in db.MstArticleInventories where d.BranchId == branch.FirstOrDefault().Id && d.ArticleId == item.FirstOrDefault().Id select d;
                                if (articleInventory.Any())
                                {
                                    itemInventoryId = articleInventory.FirstOrDefault().Id;
                                }
                            }

                            if (isLineValid)
                            {
                                var conversionUnit = from d in db.MstArticleUnits where d.ArticleId == item.FirstOrDefault().Id && d.UnitId == item.FirstOrDefault().UnitId select d;
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
                                        SIId = addSalesInvoice.Id,
                                        ItemId = item.FirstOrDefault().Id,
                                        ItemInventoryId = itemInventoryId,
                                        Particulars = objSalesInvoiceItem.Particulars,
                                        UnitId = item.FirstOrDefault().Id,
                                        Quantity = objSalesInvoiceItem.Quantity,
                                        Price = objSalesInvoiceItem.Price,
                                        DiscountId = discount.FirstOrDefault().Id,
                                        DiscountRate = discount.FirstOrDefault().DiscountRate,
                                        DiscountAmount = objSalesInvoiceItem.DiscountAmount,
                                        NetPrice = objSalesInvoiceItem.NetPrice,
                                        Amount = objSalesInvoiceItem.Amount,
                                        VATId = tax.FirstOrDefault().Id,
                                        VATPercentage = tax.FirstOrDefault().TaxRate,
                                        VATAmount = (objSalesInvoiceItem.Amount / (1 + (tax.FirstOrDefault().TaxRate / 100))) * (tax.FirstOrDefault().TaxRate / 100),
                                        BaseUnitId = item.FirstOrDefault().UnitId,
                                        BaseQuantity = baseQuantity,
                                        BasePrice = basePrice,
                                        SalesItemTimeStamp = Convert.ToDateTime(objSalesInvoiceItem.SalesItemTimeStamp)
                                    };

                                    db.TrnSalesInvoiceItems.InsertOnSubmit(addSaleInvoiceItem);
                                }
                            }
                        }

                        db.SubmitChanges();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, returnErrorMessage);
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Oops! Something's went wrong from the server! Error: " + e.Message);
            }
        }
    }
}