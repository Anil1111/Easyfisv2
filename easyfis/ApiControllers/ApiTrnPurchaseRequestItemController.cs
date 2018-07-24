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
    public class ApiTrnPurchaseRequestItemController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==========================
        // List Purchase Request Item
        // ==========================
        [Authorize, HttpGet, Route("api/purchaseRequestItem/list/{PRId}")]
        public List<Entities.TrnPurchaseRequestItem> ListPurchaseRequestItem(String PRId)
        {
            var purchaseRequestItems = from d in db.TrnPurchaseRequestItems
                                       where d.PRId == Convert.ToInt32(PRId)
                                       select new Entities.TrnPurchaseRequestItem
                                       {
                                           Id = d.Id,
                                           PRId = d.PRId,
                                           ItemId = d.ItemId,
                                           ItemCode = d.MstArticle.ManualArticleCode,
                                           ItemManualOldCode = d.MstArticle.ManualArticleOldCode,
                                           ItemDescription = d.MstArticle.Article,
                                           Particulars = d.Particulars,
                                           Quantity = d.Quantity,
                                           UnitId = d.UnitId,
                                           Unit = d.MstUnit.Unit,
                                           Cost = d.Cost,
                                           Amount = d.Amount,
                                           BaseUnitId = d.BaseUnitId,
                                           BaseUnit = d.MstUnit1.Unit,
                                           BaseQuantity = d.BaseQuantity,
                                           BaseCost = d.BaseCost
                                       };

            return purchaseRequestItems.ToList();
        }

        // ============================
        // Dropdown List - Item (Field)
        // ============================
        [Authorize, HttpGet, Route("api/purchaseRequestItem/dropdown/list/item")]
        public List<Entities.MstArticle> DropdownListPurchaseRequestItemListItem()
        {
            var items = from d in db.MstArticles.OrderBy(d => d.Article)
                        where d.ArticleTypeId == 1
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
        [Authorize, HttpGet, Route("api/purchaseRequestItem/dropdown/list/itemUnit/{itemId}")]
        public List<Entities.MstArticleUnit> DropdownListPurchaseRequestItemUnit(String itemId)
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
        [Authorize, HttpGet, Route("api/purchaseRequestItem/popUp/list/itemQuery")]
        public List<Entities.MstArticle> PopUpListPurchaseRequestItemListItemQuery()
        {
            var items = from d in db.MstArticles
                        where d.ArticleTypeId == 1
                        && d.IsLocked == true
                        select new Entities.MstArticle
                        {
                            Id = d.Id,
                            ManualArticleCode = d.ManualArticleCode,
                            ManualArticleOldCode = d.ManualArticleOldCode,
                            Article = d.Article,
                            Particulars = d.Particulars,
                            Cost = d.Cost
                        };

            return items.ToList();
        }

        // =========================
        // Add Purchase Request Item
        // =========================
        [Authorize, HttpPost, Route("api/purchaseRequestItem/add/{PRId}")]
        public HttpResponseMessage AddPurchaseRequestItem(Entities.TrnPurchaseRequestItem objPurchaseRequestItem, String PRId)
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
                                    && d.SysForm.FormName.Equals("PurchaseRequestDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var purchaseRequest = from d in db.TrnPurchaseRequests
                                                  where d.Id == Convert.ToInt32(PRId)
                                                  select d;

                            if (purchaseRequest.Any())
                            {
                                if (!purchaseRequest.FirstOrDefault().IsLocked)
                                {
                                    var item = from d in db.MstArticles
                                               where d.Id == objPurchaseRequestItem.ItemId
                                               && d.ArticleTypeId == 1
                                               && d.IsLocked == true
                                               select d;

                                    if (item.Any())
                                    {
                                        var conversionUnit = from d in db.MstArticleUnits
                                                             where d.ArticleId == objPurchaseRequestItem.ItemId
                                                             && d.UnitId == objPurchaseRequestItem.UnitId
                                                             && d.MstArticle.IsLocked == true
                                                             select d;

                                        if (conversionUnit.Any())
                                        {
                                            Decimal baseQuantity = objPurchaseRequestItem.Quantity * 1;
                                            if (conversionUnit.FirstOrDefault().Multiplier > 0)
                                            {
                                                baseQuantity = objPurchaseRequestItem.Quantity * (1 / conversionUnit.FirstOrDefault().Multiplier);
                                            }

                                            Decimal baseCost = objPurchaseRequestItem.Amount;
                                            if (baseQuantity > 0)
                                            {
                                                baseCost = objPurchaseRequestItem.Amount / baseQuantity;
                                            }

                                            Data.TrnPurchaseRequestItem newPurchaseRequestItem = new Data.TrnPurchaseRequestItem
                                            {
                                                PRId = objPurchaseRequestItem.PRId,
                                                ItemId = objPurchaseRequestItem.ItemId,
                                                Particulars = objPurchaseRequestItem.Particulars,
                                                UnitId = objPurchaseRequestItem.UnitId,
                                                Quantity = objPurchaseRequestItem.Quantity,
                                                Cost = objPurchaseRequestItem.Cost,
                                                Amount = objPurchaseRequestItem.Amount,
                                                BaseUnitId = item.FirstOrDefault().UnitId,
                                                BaseQuantity = baseQuantity,
                                                BaseCost = baseCost
                                            };

                                            db.TrnPurchaseRequestItems.InsertOnSubmit(newPurchaseRequestItem);
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
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new purchase request item if the current purchase request detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current purchase request details are not found in the server. Please add new purchase request first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new purchase request item in this purchase request detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this purchase request detail page.");
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
        // Update Purchase Request Item
        // ============================
        [Authorize, HttpPut, Route("api/purchaseRequestItem/update/{id}/{PRId}")]
        public HttpResponseMessage UpdatePurchaseRequestItem(Entities.TrnPurchaseRequestItem objPurchaseRequestItem, String id, String PRId)
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
                                    && d.SysForm.FormName.Equals("PurchaseRequestDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var purchaseRequest = from d in db.TrnPurchaseRequests
                                                  where d.Id == Convert.ToInt32(PRId)
                                                  select d;

                            if (purchaseRequest.Any())
                            {
                                if (!purchaseRequest.FirstOrDefault().IsLocked)
                                {
                                    var purchaseRequestItem = from d in db.TrnPurchaseRequestItems
                                                              where d.Id == Convert.ToInt32(id)
                                                              select d;

                                    if (purchaseRequestItem.Any())
                                    {
                                        var item = from d in db.MstArticles
                                                   where d.Id == objPurchaseRequestItem.ItemId
                                                   && d.ArticleTypeId == 1
                                                   && d.IsLocked == true
                                                   select d;

                                        if (item.Any())
                                        {
                                            var conversionUnit = from d in db.MstArticleUnits
                                                                 where d.ArticleId == objPurchaseRequestItem.ItemId
                                                                 && d.UnitId == objPurchaseRequestItem.UnitId
                                                                 && d.MstArticle.IsLocked == true
                                                                 select d;

                                            if (conversionUnit.Any())
                                            {
                                                Decimal baseQuantity = objPurchaseRequestItem.Quantity * 1;
                                                if (conversionUnit.FirstOrDefault().Multiplier > 0)
                                                {
                                                    baseQuantity = objPurchaseRequestItem.Quantity * (1 / conversionUnit.FirstOrDefault().Multiplier);
                                                }

                                                Decimal baseCost = objPurchaseRequestItem.Amount;
                                                if (baseQuantity > 0)
                                                {
                                                    baseCost = objPurchaseRequestItem.Amount / baseQuantity;
                                                }

                                                var updatePurchaseOrdeItem = purchaseRequestItem.FirstOrDefault();
                                                updatePurchaseOrdeItem.ItemId = objPurchaseRequestItem.ItemId;
                                                updatePurchaseOrdeItem.Particulars = objPurchaseRequestItem.Particulars;
                                                updatePurchaseOrdeItem.UnitId = objPurchaseRequestItem.UnitId;
                                                updatePurchaseOrdeItem.Quantity = objPurchaseRequestItem.Quantity;
                                                updatePurchaseOrdeItem.Cost = objPurchaseRequestItem.Cost;
                                                updatePurchaseOrdeItem.Amount = objPurchaseRequestItem.Amount;
                                                updatePurchaseOrdeItem.BaseUnitId = item.FirstOrDefault().UnitId;
                                                updatePurchaseOrdeItem.BaseQuantity = baseQuantity;
                                                updatePurchaseOrdeItem.BaseUnitId = item.FirstOrDefault().UnitId;
                                                updatePurchaseOrdeItem.BaseCost = baseCost;

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
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This purchase request item detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot update purchase request item if the current purchase request detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current purchase request details are not found in the server. Please add new purchase request first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update purchase request item in this purchase request detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this purchase request detail page.");
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
        // Delete Purchase Request Item
        // ============================
        [Authorize, HttpDelete, Route("api/purchaseRequestItem/delete/{id}/{PRId}")]
        public HttpResponseMessage DeletePurchaseRequestItem(String id, String PRId)
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
                                    && d.SysForm.FormName.Equals("PurchaseRequestDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var purchaseRequest = from d in db.TrnPurchaseRequests
                                                  where d.Id == Convert.ToInt32(PRId)
                                                  select d;

                            if (purchaseRequest.Any())
                            {
                                if (!purchaseRequest.FirstOrDefault().IsLocked)
                                {
                                    var purchaseRequestItem = from d in db.TrnPurchaseRequestItems
                                                              where d.Id == Convert.ToInt32(id)
                                                              select d;

                                    if (purchaseRequestItem.Any())
                                    {
                                        db.TrnPurchaseRequestItems.DeleteOnSubmit(purchaseRequestItem.First());
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This purchase request item detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot delete purchase request item if the current purchase request detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current purchase request details are not found in the server. Please add new purchase request first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete purchase request item in this purchase request item detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this purchase request detail page.");
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
