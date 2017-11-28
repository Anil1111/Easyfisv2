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
    public class ApiTrnStockCountItemController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =====================
        // List Stock Count Item
        // =====================
        [Authorize, HttpGet, Route("api/stockCountItem/list/{SCId}")]
        public List<Entities.TrnStockCountItem> ListStockCountItem(String SCId)
        {
            var stockCountItems = from d in db.TrnStockCountItems
                                  where d.SCId == Convert.ToInt32(SCId)
                                  select new Entities.TrnStockCountItem
                                  {
                                      Id = d.Id,
                                      SCId = d.SCId,
                                      ItemId = d.ItemId,
                                      ItemCode = d.MstArticle.ManualArticleCode,
                                      ItemDescription = d.MstArticle.Article,
                                      Particulars = d.Particulars,
                                      Quantity = d.Quantity,
                                      UnitId = d.MstArticle.UnitId,
                                      Unit = d.MstArticle.MstUnit.Unit
                                  };

            return stockCountItems.ToList();
        }

        // ============================
        // Dropdown List - Item (Field)
        // ============================
        [Authorize, HttpGet, Route("api/stockCountItem/dropdown/list/item")]
        public List<Entities.MstArticle> DropdownListStockCountItemListItem()
        {
            var items = from d in db.MstArticles.OrderBy(d => d.Article)
                        where d.ArticleTypeId == 1
                        && d.IsInventory == true
                        && d.Kitting != 2
                        && d.IsLocked == true
                        select new Entities.MstArticle
                        {
                            Id = d.Id,
                            ManualArticleCode = d.ManualArticleCode,
                            Article = d.Article,
                            UnitId = d.UnitId,
                            Unit = d.MstUnit.Unit
                        };

            return items.ToList();
        }

        // ============================
        // Dropdown List - Unit (Field)
        // ============================
        [Authorize, HttpGet, Route("api/stockCountItem/dropdown/list/itemUnit/{itemId}")]
        public List<Entities.MstArticleUnit> DropdownListStockCountItemUnit(String itemId)
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
        [Authorize, HttpGet, Route("api/stockCountItem/popUp/list/itemQuery")]
        public List<Entities.MstArticle> PopUpListStockCountItemListItemQuery()
        {
            var items = from d in db.MstArticles
                        where d.ArticleTypeId == 1
                        && d.IsInventory == true
                        && d.Kitting != 2
                        && d.IsLocked == true
                        select new Entities.MstArticle
                        {
                            Id = d.Id,
                            ManualArticleCode = d.ManualArticleCode,
                            Article = d.Article,
                            UnitId = d.UnitId,
                            Unit = d.MstUnit.Unit,
                            Price = d.Price
                        };

            return items.ToList();
        }

        // ====================
        // Add Stock Count Item
        // ====================
        [Authorize, HttpPost, Route("api/stockCountItem/add/{SCId}")]
        public HttpResponseMessage AddStockCountItem(Entities.TrnStockCountItem objStockCountItem, String SCId)
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
                                    && d.SysForm.FormName.Equals("StockCountDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var stockIn = from d in db.TrnStockCounts
                                          where d.Id == Convert.ToInt32(SCId)
                                          select d;

                            if (stockIn.Any())
                            {
                                if (!stockIn.FirstOrDefault().IsLocked)
                                {
                                    var item = from d in db.MstArticles
                                               where d.Id == objStockCountItem.ItemId
                                               && d.ArticleTypeId == 1
                                               && d.IsLocked == true
                                               select d;

                                    if (item.Any())
                                    {
                                        var conversionUnit = from d in db.MstArticleUnits
                                                             where d.ArticleId == objStockCountItem.ItemId
                                                             && d.UnitId == objStockCountItem.UnitId
                                                             && d.MstArticle.IsLocked == true
                                                             select d;

                                        if (conversionUnit.Any())
                                        {
                                            Data.TrnStockCountItem newStockCountItem = new Data.TrnStockCountItem
                                            {
                                                SCId = Convert.ToInt32(SCId),
                                                ItemId = objStockCountItem.ItemId,
                                                Particulars = objStockCountItem.Particulars,
                                                Quantity = objStockCountItem.Quantity
                                            };

                                            db.TrnStockCountItems.InsertOnSubmit(newStockCountItem);
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
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new stock count item if the current stock count detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current stock count details are not found in the server. Please add new stock count first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new stock count item in this stock count detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this stock count detail page.");
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

        // =======================
        // Update Stock Count Item
        // =======================
        [Authorize, HttpPut, Route("api/stockCountItem/update/{id}/{SCId}")]
        public HttpResponseMessage UpdateStockCountItem(Entities.TrnStockCountItem objStockCountItem, String id, String SCId)
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
                                    && d.SysForm.FormName.Equals("StockCountDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var stockIn = from d in db.TrnStockCounts
                                          where d.Id == Convert.ToInt32(SCId)
                                          select d;

                            if (stockIn.Any())
                            {
                                if (!stockIn.FirstOrDefault().IsLocked)
                                {
                                    var stockCountItem = from d in db.TrnStockCountItems
                                                         where d.Id == Convert.ToInt32(id)
                                                         select d;

                                    if (stockCountItem.Any())
                                    {
                                        var item = from d in db.MstArticles
                                                   where d.Id == objStockCountItem.ItemId
                                                   && d.ArticleTypeId == 1
                                                   && d.IsLocked == true
                                                   select d;

                                        if (item.Any())
                                        {
                                            var conversionUnit = from d in db.MstArticleUnits
                                                                 where d.ArticleId == objStockCountItem.ItemId
                                                                 && d.UnitId == objStockCountItem.UnitId
                                                                 && d.MstArticle.IsLocked == true
                                                                 select d;

                                            if (conversionUnit.Any())
                                            {
                                                var updateStockCountItem = stockCountItem.FirstOrDefault();
                                                updateStockCountItem.SCId = Convert.ToInt32(SCId);
                                                updateStockCountItem.ItemId = objStockCountItem.ItemId;
                                                updateStockCountItem.Particulars = objStockCountItem.Particulars;
                                                updateStockCountItem.Quantity = objStockCountItem.Quantity;
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
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This stock count item detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot update stock count item if the current stock count detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current stock count details are not found in the server. Please add new stock count first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update stock count item in this stock count detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this stock count detail page.");
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

        // =======================
        // Delete Stock Count Item
        // =======================
        [Authorize, HttpDelete, Route("api/stockCountItem/delete/{id}/{SCId}")]
        public HttpResponseMessage DeleteStockCountItem(String id, String SCId)
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
                                    && d.SysForm.FormName.Equals("StockCountDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var stockIn = from d in db.TrnStockCounts
                                          where d.Id == Convert.ToInt32(SCId)
                                          select d;

                            if (stockIn.Any())
                            {
                                if (!stockIn.FirstOrDefault().IsLocked)
                                {
                                    var stockCountItem = from d in db.TrnStockCountItems
                                                         where d.Id == Convert.ToInt32(id)
                                                         select d;

                                    if (stockCountItem.Any())
                                    {
                                        db.TrnStockCountItems.DeleteOnSubmit(stockCountItem.First());
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This stock count item detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot delete stock count item if the current stock count detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current stock count details are not found in the server. Please add new stock count first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete stock count item in this stock count item detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this stock count detail page.");
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
