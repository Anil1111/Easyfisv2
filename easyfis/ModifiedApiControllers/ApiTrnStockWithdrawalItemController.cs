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
    public class ApiTrnStockWithdrawalItemController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==========================
        // List Stock Withdrawal Item
        // ==========================
        [Authorize, HttpGet, Route("api/stockWithdrawalItem/list/{INId}")]
        public List<Entities.TrnStockWithdrawalItem> ListStockWithdrawalItem(String SWId)
        {
            var stockWithdrawalItems = from d in db.TrnStockWithdrawalItems
                                       where d.SWId == Convert.ToInt32(SWId)
                                       select new Entities.TrnStockWithdrawalItem
                                       {
                                           Id = d.Id,
                                           SWId = d.SWId,
                                           ItemId = d.ItemId,
                                           ItemManualCode = d.MstArticle.ManualArticleCode,
                                           ItemSKUCode = d.MstArticle.ManualArticleOldCode,
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

            return stockWithdrawalItems.ToList();
        }

        // ============================
        // Dropdown List - Item (Field)
        // ============================
        [Authorize, HttpGet, Route("api/stockWithdrawalItem/dropdown/list/itemInventory/item")]
        public List<Entities.MstArticleInventory> DropdownListStockWithdrawalItemListItem()
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
        [Authorize, HttpGet, Route("api/stockWithdrawalItem/dropdown/list/itemInventoryCode/{itemId}")]
        public List<Entities.MstArticleInventory> DropdownListStockWithdrawalItemListItemInventoryCode(String itemId)
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
        [Authorize, HttpGet, Route("api/stockWithdrawalItem/dropdown/list/itemUnit/{itemId}")]
        public List<Entities.MstArticleUnit> DropdownListStockWithdrawalItemUnit(String itemId)
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
        [Authorize, HttpGet, Route("api/stockWithdrawalItem/popUp/list/itemQuery")]
        public List<Entities.MstArticleInventory> PopUpListStockWithdrawalItemListItemQuery()
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

        // =========================
        // Add Stock Withdrawal Item
        // =========================
        [Authorize, HttpPost, Route("api/stockWithdrawalItem/add/{SWId}")]
        public HttpResponseMessage AddStockWithdrawalItem(Entities.TrnStockWithdrawalItem objStockWithdrawalItem, String SWId)
        {
            try
            {
                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                if (currentUser.Any())
                {
                    Int32 currentUserId = currentUser.FirstOrDefault().Id;
                    Int32 currentBranchId = currentUser.FirstOrDefault().BranchId;

                    IQueryable<Data.MstUserForm> userForms = from d in db.MstUserForms where d.UserId == currentUserId && d.SysForm.FormName.Equals("StockWithdrawalDetail") select d;
                    IQueryable<Data.TrnStockWithdrawal> stockWithdrawal = from d in db.TrnStockWithdrawals where d.Id == Convert.ToInt32(SWId) select d;
                    IQueryable<Data.MstArticle> item = from d in db.MstArticles where d.Id == objStockWithdrawalItem.ItemId && d.IsInventory == true && d.Kitting != 2 && d.IsLocked == true select d;
                    IQueryable<Data.MstArticleInventory> itemInventories = from d in db.MstArticleInventories
                                                                           where d.ArticleId == objStockWithdrawalItem.ItemId && d.BranchId == currentBranchId
                                                                           && d.Quantity > 0 && d.MstArticle.IsInventory == true && d.MstArticle.IsLocked == true
                                                                           select d;

                    Boolean isValid = false;
                    String returnMessage = "";

                    if (!userForms.Any())
                    {
                        returnMessage = "Sorry. You have no access for this stock withdrawal page.";
                    }
                    else if (!userForms.FirstOrDefault().CanAdd)
                    {
                        returnMessage = "Sorry. You have no rights to add stock withdrawal item.";
                    }
                    else if (!stockWithdrawal.Any())
                    {
                        returnMessage = "These current stock withdrawal details are not found in the server. Please add new stock withdrawal first before proceeding.";
                    }
                    else if (stockWithdrawal.FirstOrDefault().IsLocked)
                    {
                        returnMessage = "You cannot add new stock withdrawal item if the current stock withdrawal detail is locked.";
                    }
                    else if (!item.Any())
                    {
                        returnMessage = "The selected item was not found in the server.";
                    }
                    else if (!itemInventories.Any())
                    {
                        returnMessage = "The selected item has no inventory code.";
                    }
                    else
                    {
                        isValid = true;
                    }

                    if (isValid)
                    {
                        var conversionUnit = from d in db.MstArticleUnits
                                             where d.ArticleId == objStockWithdrawalItem.ItemId
                                             && d.UnitId == objStockWithdrawalItem.UnitId
                                             && d.MstArticle.IsLocked == true
                                             select d;

                        if (conversionUnit.Any())
                        {
                            Decimal baseQuantity = objStockWithdrawalItem.Quantity * 1;
                            if (conversionUnit.FirstOrDefault().Multiplier > 0)
                            {
                                baseQuantity = objStockWithdrawalItem.Quantity * (1 / conversionUnit.FirstOrDefault().Multiplier);
                            }

                            Decimal baseCost = objStockWithdrawalItem.Amount;
                            if (baseQuantity > 0)
                            {
                                baseCost = objStockWithdrawalItem.Amount / baseQuantity;
                            }

                            Data.TrnStockWithdrawalItem newStockWithdrawalItem = new Data.TrnStockWithdrawalItem
                            {
                                SWId = Convert.ToInt32(SWId),
                                ItemId = objStockWithdrawalItem.ItemId,
                                ItemInventoryId = objStockWithdrawalItem.ItemInventoryId,
                                Particulars = objStockWithdrawalItem.Particulars,
                                UnitId = objStockWithdrawalItem.UnitId,
                                Quantity = objStockWithdrawalItem.Quantity,
                                Cost = objStockWithdrawalItem.Cost,
                                Amount = objStockWithdrawalItem.Amount,
                                BaseUnitId = item.FirstOrDefault().UnitId,
                                BaseQuantity = baseQuantity,
                                BaseCost = baseCost,
                            };

                            db.TrnStockWithdrawalItems.InsertOnSubmit(newStockWithdrawalItem);
                            db.SubmitChanges();

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item has no unit conversion.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, returnMessage);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // ============================
        // Update Stock Withdrawal Item
        // ============================
        [Authorize, HttpPut, Route("api/stockWithdrawalItem/update/{id}/{SWId}")]
        public HttpResponseMessage UpdateStockWithdrawalItem(Entities.TrnStockWithdrawalItem objStockWithdrawalItem, String id, String SWId)
        {
            try
            {
                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                if (currentUser.Any())
                {
                    Int32 currentUserId = currentUser.FirstOrDefault().Id;
                    Int32 currentBranchId = currentUser.FirstOrDefault().BranchId;

                    IQueryable<Data.MstUserForm> userForms = from d in db.MstUserForms where d.UserId == currentUserId && d.SysForm.FormName.Equals("StockWithdrawalDetail") select d;
                    IQueryable<Data.TrnStockWithdrawal> stockWithdrawal = from d in db.TrnStockWithdrawals where d.Id == Convert.ToInt32(SWId) select d;
                    IQueryable<Data.TrnStockWithdrawalItem> stockWithdrawalItem = from d in db.TrnStockWithdrawalItems where d.Id == Convert.ToInt32(id) select d;
                    IQueryable<Data.MstArticle> item = from d in db.MstArticles where d.Id == objStockWithdrawalItem.ItemId && d.IsInventory == true && d.Kitting != 2 && d.IsLocked == true select d;
                    IQueryable<Data.MstArticleInventory> itemInventories = from d in db.MstArticleInventories
                                                                           where d.ArticleId == objStockWithdrawalItem.ItemId && d.BranchId == currentBranchId
                                                                           && d.Quantity > 0 && d.MstArticle.IsInventory == true && d.MstArticle.IsLocked == true
                                                                           select d;

                    Boolean isValid = false;
                    String returnMessage = "";

                    if (!userForms.Any())
                    {
                        returnMessage = "Sorry. You have no access for this stock withdrawal page.";
                    }
                    else if (!userForms.FirstOrDefault().CanAdd)
                    {
                        returnMessage = "Sorry. You have no rights to add stock withdrawal item.";
                    }
                    else if (!stockWithdrawal.Any())
                    {
                        returnMessage = "These current stock withdrawal details are not found in the server. Please add new stock withdrawal first before proceeding.";
                    }
                    else if (stockWithdrawal.FirstOrDefault().IsLocked)
                    {
                        returnMessage = "You cannot add new stock withdrawal item if the current stock withdrawal detail is locked.";
                    }
                    else if (!stockWithdrawalItem.Any())
                    {
                        returnMessage = "This stock withdrawal item detail is no longer exist in the server.";
                    }
                    else if (!item.Any())
                    {
                        returnMessage = "The selected item was not found in the server.";
                    }
                    else if (!itemInventories.Any())
                    {
                        returnMessage = "The selected item has no inventory code.";
                    }
                    else
                    {
                        isValid = true;
                    }

                    if (isValid)
                    {
                        var conversionUnit = from d in db.MstArticleUnits
                                             where d.ArticleId == objStockWithdrawalItem.ItemId
                                             && d.UnitId == objStockWithdrawalItem.UnitId
                                             && d.MstArticle.IsLocked == true
                                             select d;

                        if (conversionUnit.Any())
                        {
                            Decimal baseQuantity = objStockWithdrawalItem.Quantity * 1;
                            if (conversionUnit.FirstOrDefault().Multiplier > 0)
                            {
                                baseQuantity = objStockWithdrawalItem.Quantity * (1 / conversionUnit.FirstOrDefault().Multiplier);
                            }

                            Decimal baseCost = objStockWithdrawalItem.Amount;
                            if (baseQuantity > 0)
                            {
                                baseCost = objStockWithdrawalItem.Amount / baseQuantity;
                            }

                            var updateStockWithdrawalItem = stockWithdrawalItem.FirstOrDefault();
                            updateStockWithdrawalItem.SWId = Convert.ToInt32(SWId);
                            updateStockWithdrawalItem.ItemId = objStockWithdrawalItem.ItemId;
                            updateStockWithdrawalItem.ItemInventoryId = objStockWithdrawalItem.ItemInventoryId;
                            updateStockWithdrawalItem.Particulars = objStockWithdrawalItem.Particulars;
                            updateStockWithdrawalItem.Quantity = objStockWithdrawalItem.Quantity;
                            updateStockWithdrawalItem.UnitId = objStockWithdrawalItem.UnitId;
                            updateStockWithdrawalItem.Cost = objStockWithdrawalItem.Cost;
                            updateStockWithdrawalItem.Amount = objStockWithdrawalItem.Amount;
                            updateStockWithdrawalItem.BaseUnitId = item.FirstOrDefault().UnitId;
                            updateStockWithdrawalItem.BaseQuantity = baseQuantity;
                            updateStockWithdrawalItem.BaseCost = baseCost;
                            db.SubmitChanges();

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item has no unit conversion.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, returnMessage);
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

        // ============================
        // Delete Stock Withdrawal Item
        // ============================
        [Authorize, HttpDelete, Route("api/stockWithdrawalItem/delete/{id}/{SWId}")]
        public HttpResponseMessage DeleteStockWithdrawalItem(String id, String SWId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    Int32 currentUserId = currentUser.FirstOrDefault().Id;

                    IQueryable<Data.MstUserForm> userForms = from d in db.MstUserForms where d.UserId == currentUserId && d.SysForm.FormName.Equals("StockWithdrawalDetail") select d;
                    IQueryable<Data.TrnStockWithdrawal> stockWithdrawal = from d in db.TrnStockWithdrawals where d.Id == Convert.ToInt32(SWId) select d;
                    IQueryable<Data.TrnStockWithdrawalItem> stockWithdrawalItem = from d in db.TrnStockWithdrawalItems where d.Id == Convert.ToInt32(id) select d;

                    Boolean isValid = false;
                    String returnMessage = "";

                    if (!userForms.Any())
                    {
                        returnMessage = "Sorry. You have no access in this stock withdrawal detail page.";
                    }
                    else if (!userForms.FirstOrDefault().CanDelete)
                    {
                        returnMessage = "Sorry. You have no rights to delete stock withdrawal item in this stock withdrawal item detail page.";
                    }
                    else if (!stockWithdrawal.Any())
                    {
                        returnMessage = "These current stock withdrawal details are not found in the server. Please add new stock withdrawal first before proceeding.";
                    }
                    else if (!stockWithdrawal.FirstOrDefault().IsLocked)
                    {
                        returnMessage = "You cannot delete stock withdrawal item if the current stock withdrawal detail is locked.";
                    }
                    else if (!stockWithdrawalItem.Any())
                    {
                        returnMessage = "This stock withdrawal item detail is no longer exist in the server.";
                    }
                    else
                    {
                        isValid = true;
                    }

                    if (isValid)
                    {
                        db.TrnStockWithdrawalItems.DeleteOnSubmit(stockWithdrawalItem.First());
                        db.SubmitChanges();

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, returnMessage);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
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