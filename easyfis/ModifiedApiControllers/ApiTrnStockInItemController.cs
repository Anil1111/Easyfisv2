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
    public class ApiTrnStockInItemController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==================
        // List Stock In Item
        // ==================
        [Authorize, HttpGet, Route("api/stockInItem/list/{INId}")]
        public List<Entities.TrnStockInItem> ListStockInItem(String INId)
        {
            var stockInItems = from d in db.TrnStockInItems
                               where d.INId == Convert.ToInt32(INId)
                               select new Entities.TrnStockInItem
                               {
                                   Id = d.Id,
                                   INId = d.INId,
                                   ItemId = d.ItemId,
                                   ItemCode = d.MstArticle.ManualArticleCode,
                                   ItemDescription = d.MstArticle.Article,
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

            return stockInItems.ToList();
        }

        // ======================================
        // Get Item Inventory Cost - Cost (Field)
        // ======================================
        [Authorize, HttpGet, Route("api/stockInItem/getItemInventoryCost/{itemId}")]
        public Decimal GetItemInventoryCost(String itemId)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var itemComponents = from d in db.MstArticleComponents
                                 where d.ArticleId == Convert.ToInt32(itemId)
                                 select d;

            if (itemComponents.Any())
            {
                Decimal cost = 0;

                foreach (var itemComponent in itemComponents)
                {
                    var itemInventories = from d in db.MstArticleInventories.OrderByDescending(c => c.Cost)
                                          where d.ArticleId == itemComponent.ComponentArticleId
                                          && d.BranchId == branchId
                                          select d;

                    if (itemInventories.Any())
                    {
                        cost += itemInventories.FirstOrDefault().Cost * itemComponent.Quantity;
                    }
                }

                return cost;
            }
            else
            {
                return 0;
            }
        }

        // ============================
        // Dropdown List - Item (Field)
        // ============================
        [Authorize, HttpGet, Route("api/stockInItem/dropdown/list/item")]
        public List<Entities.MstArticle> DropdownListStockInItemListItem()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var items = from d in db.MstArticles.OrderBy(d => d.Article)
                        where d.ArticleTypeId == 1
                        && d.IsInventory == true
                        && d.Kitting != 2
                        && d.IsLocked == true
                        select new Entities.MstArticle
                        {
                            Id = d.Id,
                            ManualArticleCode = d.ManualArticleCode,
                            Article = d.Article
                        };

            return items.ToList();
        }

        // ============================
        // Dropdown List - Unit (Field)
        // ============================
        [Authorize, HttpGet, Route("api/stockInItem/dropdown/list/itemUnit/{itemId}")]
        public List<Entities.MstArticleUnit> DropdownListStockInItemUnit(String itemId)
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
        [Authorize, HttpGet, Route("api/stockInItem/popUp/list/itemQuery")]
        public List<Entities.MstArticle> PopUpListStockInItemListItemQuery()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var items = from d in db.MstArticles
                        where d.ArticleTypeId == 1
                        && d.IsInventory == true
                        && d.IsLocked == true
                        select new Entities.MstArticle
                        {
                            Id = d.Id,
                            ManualArticleCode = d.ManualArticleCode,
                            Article = d.Article,
                            Particulars = d.Particulars,
                            Price = d.Price
                        };

            return items.ToList();
        }

        // =================
        // Add Stock In Item
        // =================
        [Authorize, HttpPost, Route("api/stockInItem/add/{INId}")]
        public HttpResponseMessage AddStockInItem(Entities.TrnStockInItem objStockInItem, String INId)
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
                                    && d.SysForm.FormName.Equals("StockInDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var stockIn = from d in db.TrnStockIns
                                          where d.Id == Convert.ToInt32(INId)
                                          select d;

                            if (stockIn.Any())
                            {
                                if (!stockIn.FirstOrDefault().IsLocked)
                                {
                                    var item = from d in db.MstArticles
                                               where d.Id == objStockInItem.ItemId
                                               && d.ArticleTypeId == 1
                                               && d.IsLocked == true
                                               select d;

                                    if (item.Any())
                                    {
                                        var conversionUnit = from d in db.MstArticleUnits
                                                             where d.ArticleId == objStockInItem.ItemId
                                                             && d.UnitId == objStockInItem.UnitId
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
                                                INId = Convert.ToInt32(INId),
                                                ItemId = objStockInItem.ItemId,
                                                Particulars = objStockInItem.Particulars,
                                                UnitId = objStockInItem.UnitId,
                                                Quantity = objStockInItem.Quantity,
                                                Cost = objStockInItem.Cost,
                                                Amount = objStockInItem.Amount,
                                                BaseUnitId = item.FirstOrDefault().UnitId,
                                                BaseQuantity = baseQuantity,
                                                BaseCost = baseCost
                                            };

                                            db.TrnStockInItems.InsertOnSubmit(newStockInItem);
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
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item was not found in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new stock in item if the current stock in detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current stock in details are not found in the server. Please add new stock in first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new stock in item in this stock in detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this stock in detail page.");
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

        // ====================
        // Update Stock In Item
        // ====================
        [Authorize, HttpPut, Route("api/stockInItem/update/{id}/{INId}")]
        public HttpResponseMessage UpdateStockInItem(Entities.TrnStockInItem objStockInItem, String id, String INId)
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
                                    && d.SysForm.FormName.Equals("StockInDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var stockIn = from d in db.TrnStockIns
                                          where d.Id == Convert.ToInt32(INId)
                                          select d;

                            if (stockIn.Any())
                            {
                                if (!stockIn.FirstOrDefault().IsLocked)
                                {
                                    var stockInItem = from d in db.TrnStockInItems
                                                      where d.Id == Convert.ToInt32(id)
                                                      select d;

                                    if (stockInItem.Any())
                                    {
                                        var item = from d in db.MstArticles
                                                   where d.Id == objStockInItem.ItemId
                                                   && d.ArticleTypeId == 1
                                                   && d.IsLocked == true
                                                   select d;

                                        if (item.Any())
                                        {
                                            var conversionUnit = from d in db.MstArticleUnits
                                                                 where d.ArticleId == objStockInItem.ItemId
                                                                 && d.UnitId == objStockInItem.UnitId
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

                                                var updateStockInItem = stockInItem.FirstOrDefault();
                                                updateStockInItem.INId = Convert.ToInt32(INId);
                                                updateStockInItem.ItemId = objStockInItem.ItemId;
                                                updateStockInItem.Particulars = objStockInItem.Particulars;
                                                updateStockInItem.Quantity = objStockInItem.Quantity;
                                                updateStockInItem.UnitId = objStockInItem.UnitId;
                                                updateStockInItem.Cost = objStockInItem.Cost;
                                                updateStockInItem.Amount = objStockInItem.Amount;
                                                updateStockInItem.BaseUnitId = item.FirstOrDefault().UnitId;
                                                updateStockInItem.BaseQuantity = baseQuantity;
                                                updateStockInItem.BaseUnitId = item.FirstOrDefault().UnitId;
                                                updateStockInItem.BaseCost = baseCost;

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
                                            return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected item was not found in the server.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This stock in item detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot update stock in item if the current stock in detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current stock in details are not found in the server. Please add new stock in first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update stock in item in this stock in detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this stock in detail page.");
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

        // ====================
        // Delete Stock In Item
        // ====================
        [Authorize, HttpDelete, Route("api/stockInItem/delete/{id}/{INId}")]
        public HttpResponseMessage DeleteStockInItem(String id, String INId)
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
                                    && d.SysForm.FormName.Equals("StockInDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var stockIn = from d in db.TrnStockIns
                                          where d.Id == Convert.ToInt32(INId)
                                          select d;

                            if (stockIn.Any())
                            {
                                if (!stockIn.FirstOrDefault().IsLocked)
                                {
                                    var stockInItem = from d in db.TrnStockInItems
                                                      where d.Id == Convert.ToInt32(id)
                                                      select d;

                                    if (stockInItem.Any())
                                    {
                                        db.TrnStockInItems.DeleteOnSubmit(stockInItem.First());
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This stock in item detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot delete stock in item if the current stock in detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current stock in details are not found in the server. Please add new stock in first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete stock in item in this stock in item detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this stock in detail page.");
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
