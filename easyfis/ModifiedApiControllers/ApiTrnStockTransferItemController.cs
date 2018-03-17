using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;

namespace easyfis.ModifiedApiControllers
{
    public class ApiTrnStockTransferItemController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========================
        // List Stock Transfer Item
        // ========================
        [Authorize, HttpGet, Route("api/stockTransferItem/list/{STId}")]
        public List<Entities.TrnStockTransferItem> ListStockTransferItem(String STId)
        {
            var stockTransferItems = from d in db.TrnStockTransferItems
                                     where d.STId == Convert.ToInt32(STId)
                                     select new Entities.TrnStockTransferItem
                                     {
                                         Id = d.Id,
                                         STId = d.STId,
                                         ItemId = d.ItemId,
                                         ItemCode = d.MstArticle.ManualArticleCode,
                                         ItemManualArticleOldCode = d.MstArticle.ManualArticleOldCode,
                                         ItemDescription = d.MstArticle.Article,
                                         ItemInventoryId = d.ItemInventoryId,
                                         ItemInventoryCode = d.MstArticleInventory.InventoryCode,
                                         Particulars = d.Particulars,
                                         UnitId = d.UnitId,
                                         Unit = d.MstUnit1.Unit,
                                         Quantity = d.Quantity,
                                         Cost = d.Cost,
                                         Amount = d.Amount,
                                         BaseUnitId = d.BaseUnitId,
                                         BaseUnit = d.MstUnit.Unit,
                                         BaseQuantity = d.BaseQuantity,
                                         BaseCost = d.BaseCost
                                     };

            return stockTransferItems.ToList();
        }

        // ============================
        // Dropdown List - Item (Field)
        // ============================
        [Authorize, HttpGet, Route("api/stockTransferItem/dropdown/list/itemInventory/item")]
        public List<Entities.MstArticleInventory> DropdownListStockTransferItemListItem()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var itemInventories = from d in db.MstArticleInventories
                                  where d.BranchId == branchId
                                  && d.Quantity > 0
                                  && d.MstArticle.IsInventory == true
                                  && d.MstArticle.IsLocked == true
                                  select new Entities.MstArticleInventory
                                  {
                                      Id = d.Id,
                                      ArticleId = d.ArticleId,
                                      ManualArticleCode = d.MstArticle.ManualArticleCode,
                                      Article = d.MstArticle.Article
                                  };

            return itemInventories.ToList();
        }

        // ===========================================
        // Dropdown List - Item Inventory Code (Field)
        // ===========================================
        [Authorize, HttpGet, Route("api/stockTransferItem/dropdown/list/itemInventoryCode/{itemId}")]
        public List<Entities.MstArticleInventory> DropdownListStockTransferItemListItemInventoryCode(String itemId)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var itemInventories = from d in db.MstArticleInventories
                                  where d.BranchId == branchId
                                  && d.ArticleId == Convert.ToInt32(itemId)
                                  && d.Quantity > 0
                                  && d.MstArticle.IsInventory == true
                                  && d.MstArticle.IsLocked == true
                                  select new Entities.MstArticleInventory
                                  {
                                      Id = d.Id,
                                      InventoryCode = d.InventoryCode,
                                      Cost = d.Cost
                                  };

            return itemInventories.ToList();
        }

        // ============================
        // Dropdown List - Unit (Field)
        // ============================
        [Authorize, HttpGet, Route("api/stockTransferItem/dropdown/list/itemUnit/{itemId}")]
        public List<Entities.MstArticleUnit> DropdownListStockTransferItemUnit(String itemId)
        {
            var itemUnit = from d in db.MstArticleUnits.OrderBy(d => d.MstUnit.Unit)
                           where d.ArticleId == Convert.ToInt32(itemId)
                           && d.MstArticle.IsLocked == true
                           select new Entities.MstArticleUnit
                           {
                               Id = d.Id,
                               UnitId = d.UnitId,
                               Unit = d.MstUnit.Unit
                           };

            return itemUnit.ToList();
        }

        // ========================
        // Pop-Up List - Item Query
        // ========================
        [Authorize, HttpGet, Route("api/stockTransferItem/popUp/list/itemQuery")]
        public List<Entities.MstArticleInventory> PopUpListStockTransferItemListItemQuery()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var itemInventories = from d in db.MstArticleInventories
                                  where d.BranchId == branchId
                                  && d.Quantity > 0
                                  && d.MstArticle.IsInventory == true
                                  && d.MstArticle.IsLocked == true
                                  select new Entities.MstArticleInventory
                                  {
                                      Id = d.Id,
                                      ArticleId = d.ArticleId,
                                      ManualArticleCode = d.MstArticle.ManualArticleCode,
                                      ManualArticleOldCode = d.MstArticle.ManualArticleOldCode,
                                      Article = d.MstArticle.Article,
                                      UnitId = d.MstArticle.UnitId,
                                      Unit = d.MstArticle.MstUnit.Unit,
                                      InventoryCode = d.InventoryCode,
                                      Price = d.MstArticle.Price,
                                      Quantity = d.Quantity,
                                      Cost = d.Cost,
                                      Amount = d.Amount
                                  };

            return itemInventories.ToList();
        }

        // =======================
        // Add Stock Transfer Item
        // =======================
        [Authorize, HttpPost, Route("api/stockTransferItem/add/{STId}")]
        public HttpResponseMessage AddStockTransferItem(Entities.TrnStockTransferItem objStockTransferItem, String STId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;
                    var currentBranchId = currentUser.FirstOrDefault().BranchId;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("StockTransferDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var stockTransfer = from d in db.TrnStockTransfers
                                                where d.Id == Convert.ToInt32(STId)
                                                select d;

                            if (stockTransfer.Any())
                            {
                                if (!stockTransfer.FirstOrDefault().IsLocked)
                                {
                                    var item = from d in db.MstArticles
                                               where d.Id == objStockTransferItem.ItemId
                                               && d.IsInventory == true
                                               && d.Kitting != 2
                                               && d.IsLocked == true
                                               select d;

                                    if (item.Any())
                                    {
                                        var conversionUnit = from d in db.MstArticleUnits
                                                             where d.ArticleId == objStockTransferItem.ItemId
                                                             && d.UnitId == objStockTransferItem.UnitId
                                                             && d.MstArticle.IsLocked == true
                                                             select d;

                                        if (conversionUnit.Any())
                                        {
                                            var itemInventories = from d in db.MstArticleInventories
                                                                  where d.ArticleId == objStockTransferItem.ItemId
                                                                  && d.BranchId == currentBranchId
                                                                  && d.Quantity > 0
                                                                  && d.MstArticle.IsInventory == true
                                                                  && d.MstArticle.IsLocked == true
                                                                  select d;

                                            if (itemInventories.Any())
                                            {
                                                Decimal baseQuantity = objStockTransferItem.Quantity * 1;
                                                if (conversionUnit.FirstOrDefault().Multiplier > 0)
                                                {
                                                    baseQuantity = objStockTransferItem.Quantity * (1 / conversionUnit.FirstOrDefault().Multiplier);
                                                }

                                                Decimal baseCost = objStockTransferItem.Amount;
                                                if (baseQuantity > 0)
                                                {
                                                    baseCost = objStockTransferItem.Amount / baseQuantity;
                                                }

                                                Data.TrnStockTransferItem newStockTransferItem = new Data.TrnStockTransferItem
                                                {
                                                    STId = Convert.ToInt32(STId),
                                                    ItemId = objStockTransferItem.ItemId,
                                                    ItemInventoryId = objStockTransferItem.ItemInventoryId,
                                                    Particulars = objStockTransferItem.Particulars,
                                                    UnitId = objStockTransferItem.UnitId,
                                                    Quantity = objStockTransferItem.Quantity,
                                                    Cost = objStockTransferItem.Cost,
                                                    Amount = objStockTransferItem.Amount,
                                                    BaseUnitId = item.FirstOrDefault().UnitId,
                                                    BaseQuantity = baseQuantity,
                                                    BaseCost = baseCost,
                                                };

                                                db.TrnStockTransferItems.InsertOnSubmit(newStockTransferItem);
                                                db.SubmitChanges();

                                                return Request.CreateResponse(HttpStatusCode.OK);
                                            }
                                            else
                                            {
                                                return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item has no inventory code.");
                                            }
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item has no unit conversion.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item was not found in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new stock transfer item if the current stock transfer detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current stock transfer details are not found in the server. Please add new stock transfer first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new stock transfer item in this stock transfer detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this stock transfer detail page.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // ==========================
        // Update Stock Transfer Item
        // ==========================
        [Authorize, HttpPut, Route("api/stockTransferItem/update/{id}/{STId}")]
        public HttpResponseMessage UpdateStockTransferItem(Entities.TrnStockTransferItem objStockTransferItem, String id, String STId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;
                    var currentBranchId = currentUser.FirstOrDefault().BranchId;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("StockTransferDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var stockTransfer = from d in db.TrnStockTransfers
                                                where d.Id == Convert.ToInt32(STId)
                                                select d;

                            if (stockTransfer.Any())
                            {
                                if (!stockTransfer.FirstOrDefault().IsLocked)
                                {
                                    var stockTransferItem = from d in db.TrnStockTransferItems
                                                            where d.Id == Convert.ToInt32(id)
                                                            select d;

                                    if (stockTransferItem.Any())
                                    {
                                        var item = from d in db.MstArticles
                                                   where d.Id == objStockTransferItem.ItemId
                                                   && d.ArticleTypeId == 1
                                                   && d.IsLocked == true
                                                   select d;

                                        if (item.Any())
                                        {
                                            var conversionUnit = from d in db.MstArticleUnits
                                                                 where d.ArticleId == objStockTransferItem.ItemId
                                                                 && d.UnitId == objStockTransferItem.UnitId
                                                                 && d.MstArticle.IsLocked == true
                                                                 select d;

                                            if (conversionUnit.Any())
                                            {
                                                var itemInventories = from d in db.MstArticleInventories
                                                                      where d.ArticleId == objStockTransferItem.ItemId
                                                                      && d.BranchId == currentBranchId
                                                                      && d.Quantity > 0
                                                                      && d.MstArticle.IsInventory == true
                                                                      && d.MstArticle.IsLocked == true
                                                                      select d;

                                                if (itemInventories.Any())
                                                {
                                                    Decimal baseQuantity = objStockTransferItem.Quantity * 1;
                                                    if (conversionUnit.FirstOrDefault().Multiplier > 0)
                                                    {
                                                        baseQuantity = objStockTransferItem.Quantity * (1 / conversionUnit.FirstOrDefault().Multiplier);
                                                    }

                                                    Decimal baseCost = objStockTransferItem.Amount;
                                                    if (baseQuantity > 0)
                                                    {
                                                        baseCost = objStockTransferItem.Amount / baseQuantity;
                                                    }

                                                    var updateStockTransferItem = stockTransferItem.FirstOrDefault();
                                                    updateStockTransferItem.STId = Convert.ToInt32(STId);
                                                    updateStockTransferItem.ItemId = objStockTransferItem.ItemId;
                                                    updateStockTransferItem.ItemInventoryId = objStockTransferItem.ItemInventoryId;
                                                    updateStockTransferItem.Particulars = objStockTransferItem.Particulars;
                                                    updateStockTransferItem.Quantity = objStockTransferItem.Quantity;
                                                    updateStockTransferItem.UnitId = objStockTransferItem.UnitId;
                                                    updateStockTransferItem.Cost = objStockTransferItem.Cost;
                                                    updateStockTransferItem.Amount = objStockTransferItem.Amount;
                                                    updateStockTransferItem.BaseUnitId = item.FirstOrDefault().UnitId;
                                                    updateStockTransferItem.BaseQuantity = baseQuantity;
                                                    updateStockTransferItem.BaseCost = baseCost;

                                                    db.SubmitChanges();

                                                    return Request.CreateResponse(HttpStatusCode.OK);
                                                }
                                                else
                                                {
                                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item has no inventory code.");
                                                }
                                            }
                                            else
                                            {
                                                return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item has no unit conversion.");
                                            }
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item was not found in the server.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This stock transfer item detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot update stock transfer item if the current stock transfer detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current stock transfer details are not found in the server. Please add new stock transfer first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update stock transfer item in this stock transfer detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this stock transfer detail page.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // ==========================
        // Delete Stock Transfer Item
        // ==========================
        [Authorize, HttpDelete, Route("api/stockTransferItem/delete/{id}/{STId}")]
        public HttpResponseMessage DeleteStockTransferItem(String id, String STId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("StockTransferDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var stockTransfer = from d in db.TrnStockTransfers
                                                where d.Id == Convert.ToInt32(STId)
                                                select d;

                            if (stockTransfer.Any())
                            {
                                if (!stockTransfer.FirstOrDefault().IsLocked)
                                {
                                    var stockTransferItem = from d in db.TrnStockTransferItems
                                                            where d.Id == Convert.ToInt32(id)
                                                            select d;

                                    if (stockTransferItem.Any())
                                    {
                                        db.TrnStockTransferItems.DeleteOnSubmit(stockTransferItem.First());
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This stock transfer item detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot delete stock transfer item if the current stock transfer detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current stock transfer details are not found in the server. Please add new stock transfer first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete stock transfer item in this stock transfer item detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this stock transfer detail page.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
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
