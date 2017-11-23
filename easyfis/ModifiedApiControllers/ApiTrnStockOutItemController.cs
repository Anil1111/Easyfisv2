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
    public class ApiTrnStockOutItemController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===================
        // List Stock Out Item
        // ===================
        [Authorize, HttpGet, Route("api/stockOutItem/list/{OTId}")]
        public List<Entities.TrnStockOutItem> ListStockOutItem(String OTId)
        {
            var stockOutItems = from d in db.TrnStockOutItems
                                where d.OTId == Convert.ToInt32(OTId)
                                select new Entities.TrnStockOutItem
                                {
                                    Id = d.Id,
                                    OTId = d.OTId,
                                    ItemId = d.ItemId,
                                    ItemCode = d.MstArticle.ManualArticleCode,
                                    ItemDescription = d.MstArticle.Article,
                                    Particulars = d.Particulars,
                                    Quantity = d.Quantity,
                                    UnitId = d.UnitId,
                                    Unit = d.MstUnit1.Unit,
                                    Cost = d.Cost,
                                    Amount = d.Amount,
                                    ItemInventoryId = d.ItemInventoryId,
                                    ItemInventoryCode = d.MstArticleInventory.InventoryCode,
                                    BaseUnitId = d.BaseUnitId,
                                    BaseUnit = d.MstUnit.Unit,
                                    BaseQuantity = d.BaseQuantity,
                                    BaseCost = d.BaseCost,
                                    ExpenseAccountId = d.ExpenseAccountId,
                                    ExpenseAccount = d.MstAccount.Account,
                                };

            return stockOutItems.ToList();
        }

        // ============================
        // Dropdown List - Item (Field)
        // ============================
        [Authorize, HttpGet, Route("api/stockOutItem/dropdown/list/itemInventory/item")]
        public List<Entities.MstArticleInventory> DropdownListStockOutItemListItem()
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
                                      Article = d.MstArticle.Article,
                                      ExpenseAccountId = d.MstArticle.ExpenseAccountId
                                  };

            return itemInventories.ToList();
        }

        // ===========================================
        // Dropdown List - Item Inventory Code (Field)
        // ===========================================
        [Authorize, HttpGet, Route("api/stockOutItem/dropdown/list/itemInventoryCode/{itemId}")]
        public List<Entities.MstArticleInventory> DropdownListStockOutItemListItemInventoryCode(String itemId)
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
        [Authorize, HttpGet, Route("api/stockOutItem/dropdown/list/itemUnit/{itemId}")]
        public List<Entities.MstArticleUnit> DropdownListStockOutItemUnit(String itemId)
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

        // =======================================
        // Dropdown List - Expense Account (Field)
        // =======================================
        [Authorize, HttpGet, Route("api/stockOutItem/dropdown/list/expenseAccount")]
        public List<Entities.MstAccount> DropdownListStockOutItemExpenseAccount()
        {
            var expenseAccounts = from d in db.MstAccounts
                                  where d.IsLocked == true
                                  select new Entities.MstAccount
                                  {
                                      Id = d.Id,
                                      AccountCode = d.AccountCode,
                                      Account = d.Account
                                  };

            return expenseAccounts.ToList();
        }

        // ========================
        // Pop-Up List - Item Query
        // ========================
        [Authorize, HttpGet, Route("api/stockOutItem/popUp/list/itemQuery")]
        public List<Entities.MstArticleInventory> PopUpListStockOutItemListItemQuery()
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

        // ==================
        // Add Stock Out Item
        // ==================
        [Authorize, HttpPost, Route("api/stockOutItem/add/{OTId}")]
        public HttpResponseMessage AddStockOutItem(Entities.TrnStockOutItem objStockOutItem, String OTId)
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
                                    && d.SysForm.FormName.Equals("StockOutDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var stockOut = from d in db.TrnStockOuts
                                           where d.Id == Convert.ToInt32(OTId)
                                           select d;

                            if (stockOut.Any())
                            {
                                if (!stockOut.FirstOrDefault().IsLocked)
                                {
                                    var item = from d in db.MstArticles
                                               where d.Id == objStockOutItem.ItemId
                                               && d.IsInventory == true
                                               && d.Kitting != 2
                                               && d.IsLocked == true
                                               select d;

                                    if (item.Any())
                                    {
                                        var conversionUnit = from d in db.MstArticleUnits
                                                             where d.ArticleId == objStockOutItem.ItemId
                                                             && d.UnitId == objStockOutItem.UnitId
                                                             && d.MstArticle.IsLocked == true
                                                             select d;

                                        if (conversionUnit.Any())
                                        {
                                            var itemInventories = from d in db.MstArticleInventories
                                                                  where d.ArticleId == objStockOutItem.ItemId
                                                                  && d.BranchId == currentBranchId
                                                                  && d.Quantity > 0
                                                                  && d.MstArticle.IsInventory == true
                                                                  && d.MstArticle.IsLocked == true
                                                                  select d;

                                            if (itemInventories.Any())
                                            {
                                                var expenseAccounts = from d in db.MstAccounts
                                                                      where d.Id == objStockOutItem.ExpenseAccountId
                                                                      && d.IsLocked == true
                                                                      select d;

                                                if (expenseAccounts.Any())
                                                {
                                                    Decimal baseQuantity = objStockOutItem.Quantity * 1;
                                                    if (conversionUnit.FirstOrDefault().Multiplier > 0)
                                                    {
                                                        baseQuantity = objStockOutItem.Quantity * (1 / conversionUnit.FirstOrDefault().Multiplier);
                                                    }

                                                    Decimal baseCost = objStockOutItem.Amount;
                                                    if (baseQuantity > 0)
                                                    {
                                                        baseCost = objStockOutItem.Amount / baseQuantity;
                                                    }

                                                    Data.TrnStockOutItem newStockOutItem = new Data.TrnStockOutItem
                                                    {
                                                        OTId = Convert.ToInt32(OTId),
                                                        ItemId = objStockOutItem.ItemId,
                                                        ItemInventoryId = objStockOutItem.ItemInventoryId,
                                                        Particulars = objStockOutItem.Particulars,
                                                        UnitId = objStockOutItem.UnitId,
                                                        Quantity = objStockOutItem.Quantity,
                                                        Cost = objStockOutItem.Cost,
                                                        Amount = objStockOutItem.Amount,
                                                        BaseUnitId = item.FirstOrDefault().UnitId,
                                                        BaseQuantity = baseQuantity,
                                                        BaseCost = baseCost,
                                                        ExpenseAccountId = objStockOutItem.ExpenseAccountId
                                                    };

                                                    db.TrnStockOutItems.InsertOnSubmit(newStockOutItem);
                                                    db.SubmitChanges();

                                                    return Request.CreateResponse(HttpStatusCode.OK);
                                                }
                                                else
                                                {
                                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item has no expense account.");
                                                }
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
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new stock out item if the current stock out detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current stock out details are not found in the server. Please add new stock out first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new stock out item in this stock out detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this stock out detail page.");
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

        // =====================
        // Update Stock Out Item
        // =====================
        [Authorize, HttpPut, Route("api/stockOutItem/update/{id}/{OTId}")]
        public HttpResponseMessage UpdateStockOutItem(Entities.TrnStockOutItem objStockOutItem, String id, String OTId)
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
                                    && d.SysForm.FormName.Equals("StockOutDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var stockOut = from d in db.TrnStockOuts
                                           where d.Id == Convert.ToInt32(OTId)
                                           select d;

                            if (stockOut.Any())
                            {
                                if (!stockOut.FirstOrDefault().IsLocked)
                                {
                                    var stockOutItem = from d in db.TrnStockOutItems
                                                       where d.Id == Convert.ToInt32(id)
                                                       select d;

                                    if (stockOutItem.Any())
                                    {
                                        var item = from d in db.MstArticles
                                                   where d.Id == objStockOutItem.ItemId
                                                   && d.ArticleTypeId == 1
                                                   && d.IsLocked == true
                                                   select d;

                                        if (item.Any())
                                        {
                                            var conversionUnit = from d in db.MstArticleUnits
                                                                 where d.ArticleId == objStockOutItem.ItemId
                                                                 && d.UnitId == objStockOutItem.UnitId
                                                                 && d.MstArticle.IsLocked == true
                                                                 select d;

                                            if (conversionUnit.Any())
                                            {
                                                var itemInventories = from d in db.MstArticleInventories
                                                                      where d.ArticleId == objStockOutItem.ItemId
                                                                      && d.BranchId == currentBranchId
                                                                      && d.Quantity > 0
                                                                      && d.MstArticle.IsInventory == true
                                                                      && d.MstArticle.IsLocked == true
                                                                      select d;

                                                if (itemInventories.Any())
                                                {
                                                    var expenseAccounts = from d in db.MstAccounts
                                                                          where d.Id == objStockOutItem.ExpenseAccountId
                                                                          && d.IsLocked == true
                                                                          select d;

                                                    if (expenseAccounts.Any())
                                                    {
                                                        Decimal baseQuantity = objStockOutItem.Quantity * 1;
                                                        if (conversionUnit.FirstOrDefault().Multiplier > 0)
                                                        {
                                                            baseQuantity = objStockOutItem.Quantity * (1 / conversionUnit.FirstOrDefault().Multiplier);
                                                        }

                                                        Decimal baseCost = objStockOutItem.Amount;
                                                        if (baseQuantity > 0)
                                                        {
                                                            baseCost = objStockOutItem.Amount / baseQuantity;
                                                        }

                                                        var updateStockOutItem = stockOutItem.FirstOrDefault();
                                                        updateStockOutItem.OTId = Convert.ToInt32(OTId);
                                                        updateStockOutItem.ItemId = objStockOutItem.ItemId;
                                                        updateStockOutItem.ItemInventoryId = objStockOutItem.ItemInventoryId;
                                                        updateStockOutItem.Particulars = objStockOutItem.Particulars;
                                                        updateStockOutItem.Quantity = objStockOutItem.Quantity;
                                                        updateStockOutItem.UnitId = objStockOutItem.UnitId;
                                                        updateStockOutItem.Cost = objStockOutItem.Cost;
                                                        updateStockOutItem.Amount = objStockOutItem.Amount;
                                                        updateStockOutItem.BaseUnitId = item.FirstOrDefault().UnitId;
                                                        updateStockOutItem.BaseQuantity = baseQuantity;
                                                        updateStockOutItem.BaseCost = baseCost;
                                                        updateStockOutItem.ExpenseAccountId = objStockOutItem.ExpenseAccountId;

                                                        db.SubmitChanges();

                                                        return Request.CreateResponse(HttpStatusCode.OK);
                                                    }
                                                    else
                                                    {
                                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item has no expense account.");
                                                    }
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
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This stock out item detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot update stock out item if the current stock out detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current stock out details are not found in the server. Please add new stock out first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update stock out item in this stock out detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this stock out detail page.");
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

        // =====================
        // Delete Stock Out Item
        // =====================
        [Authorize, HttpDelete, Route("api/stockOutItem/delete/{id}/{OTId}")]
        public HttpResponseMessage DeleteStockOutItem(String id, String OTId)
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
                                    && d.SysForm.FormName.Equals("StockOutDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var stockOut = from d in db.TrnStockOuts
                                           where d.Id == Convert.ToInt32(OTId)
                                           select d;

                            if (stockOut.Any())
                            {
                                if (!stockOut.FirstOrDefault().IsLocked)
                                {
                                    var stockOutItem = from d in db.TrnStockOutItems
                                                       where d.Id == Convert.ToInt32(id)
                                                       select d;

                                    if (stockOutItem.Any())
                                    {
                                        db.TrnStockOutItems.DeleteOnSubmit(stockOutItem.First());
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This stock out item detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot delete stock out item if the current stock out detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current stock out details are not found in the server. Please add new stock out first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete stock out item in this stock out item detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this stock out detail page.");
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
