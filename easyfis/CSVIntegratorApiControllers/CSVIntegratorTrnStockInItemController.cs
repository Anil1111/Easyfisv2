using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Diagnostics;

namespace easyfis.CSVIntegratorApiControllers
{
    public class CSVIntegratorTrnStockInItemController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==================================
        // Add Stock In Item (CSV Integrator)
        // ==================================
        [HttpPost, Route("api/add/CSVIntegrator/stockInItem")]
        public HttpResponseMessage AddStockInItem(List<CSVIntegratorEntities.CSVIntegratorTrnStockInItem> objStockInItems)
        {
            try
            {
                if (objStockInItems.Any())
                {
                    foreach (var objStockInItem in objStockInItems)
                    {
                        Boolean stockInExists = false;
                        var stockIn = from d in db.TrnStockIns where d.MstBranch.BranchCode.Equals(objStockInItem.BranchCode) && d.INNumber.Equals(objStockInItem.INNumber) select d;
                        if (stockIn.Any())
                        {
                            stockInExists = true;
                        }

                        Boolean itemExists = false;
                        var item = from d in db.MstArticles where d.ManualArticleCode.Equals(objStockInItem.ItemCode) && d.ArticleTypeId == 1 && d.IsLocked == true select d;
                        if (item.Any())
                        {
                            itemExists = true;
                        }

                        Boolean isValid = false;
                        if (stockInExists)
                        {
                            if (itemExists)
                            {
                                if (!stockIn.FirstOrDefault().IsLocked)
                                {
                                    isValid = true;
                                }
                            }
                        }

                        if (isValid)
                        {
                            var conversionUnit = from d in db.MstArticleUnits
                                                 where d.ArticleId == item.FirstOrDefault().Id
                                                 && d.UnitId == item.FirstOrDefault().UnitId
                                                 && d.MstArticle.IsLocked == true
                                                 select d;

                            if (conversionUnit.Any())
                            {
                                Decimal baseQuantity = objStockInItem.Quantity * 1;
                                if (conversionUnit.FirstOrDefault().Multiplier > 0)
                                {
                                    baseQuantity = objStockInItem.Quantity * (1 / conversionUnit.FirstOrDefault().Multiplier);
                                }

                                Decimal baseCost = objStockInItem.Amount;
                                if (baseQuantity > 0)
                                {
                                    baseCost = objStockInItem.Amount / baseQuantity;
                                }

                                Data.TrnStockInItem newStockInItem = new Data.TrnStockInItem
                                {
                                    INId = stockIn.FirstOrDefault().Id,
                                    ItemId = item.FirstOrDefault().Id,
                                    Particulars = objStockInItem.Particulars,
                                    UnitId = item.FirstOrDefault().UnitId,
                                    Quantity = objStockInItem.Quantity,
                                    Cost = objStockInItem.Cost,
                                    Amount = objStockInItem.Amount,
                                    BaseUnitId = item.FirstOrDefault().UnitId,
                                    BaseQuantity = baseQuantity,
                                    BaseCost = baseCost
                                };

                                db.TrnStockInItems.InsertOnSubmit(newStockInItem);
                            }
                        }
                    }

                    db.SubmitChanges();

                    return Request.CreateResponse(HttpStatusCode.OK);
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
    }
}