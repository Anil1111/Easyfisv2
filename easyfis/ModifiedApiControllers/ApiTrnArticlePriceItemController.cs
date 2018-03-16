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
    public class ApiTrnArticlePriceItemController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =========================
        // List Article Price Item
        // =========================
        [Authorize, HttpGet, Route("api/articlePriceItem/list/{articlePriceId}")]
        public List<Entities.TrnArticlePriceItem> ListArticlePriceItem(String articlePriceId)
        {
            var articlePriceItems = from d in db.TrnArticlePriceItems
                                    where d.ArticlePriceId == Convert.ToInt32(articlePriceId)
                                    select new Entities.TrnArticlePriceItem
                                    {
                                        Id = d.Id,
                                        ArticlePriceId = d.ArticlePriceId,
                                        ItemId = d.ItemId,
                                        ItemCode = d.MstArticle.ManualArticleCode,
                                        ItemDescription = d.MstArticle.Article,
                                        Price = d.Price,
                                        TriggerQuantity = d.TriggerQuantity
                                    };

            return articlePriceItems.ToList();
        }

        // ============================
        // Dropdown List - Item (Field)
        // ============================
        [Authorize, HttpGet, Route("api/articlePriceItem/dropdown/list/item")]
        public List<Entities.MstArticle> DropdownListArticlePriceItemListItem()
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

        // ========================
        // Pop-Up List - Item Query
        // ========================
        [Authorize, HttpGet, Route("api/articlePriceItem/popUp/list/itemQuery")]
        public List<Entities.MstArticle> PopUpListArticlePriceItemListItemQuery()
        {
            var items = from d in db.MstArticles
                        where d.ArticleTypeId == 1
                        && d.IsLocked == true
                        select new Entities.MstArticle
                        {
                            Id = d.Id,
                            ManualArticleCode = d.ManualArticleCode,
                            Article = d.Article,
                            Particulars = d.Particulars,
                            Unit = d.MstUnit.Unit
                        };

            return items.ToList();
        }

        // ======================
        // Add Article Price Item
        // ======================
        [Authorize, HttpPost, Route("api/articlePriceItem/add/{articlePriceId}")]
        public HttpResponseMessage AddArticlePriceItem(Entities.TrnArticlePriceItem objArticlePriceItem, String articlePriceId)
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
                                    && d.SysForm.FormName.Equals("ItemPriceDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var articlePrice = from d in db.TrnArticlePrices
                                               where d.Id == Convert.ToInt32(articlePriceId)
                                               select d;

                            if (articlePrice.Any())
                            {
                                if (!articlePrice.FirstOrDefault().IsLocked)
                                {
                                    var articles = from d in db.MstArticles
                                                   where d.Id == objArticlePriceItem.ItemId
                                                   && d.IsLocked == true
                                                   select d;

                                    if (articles.Any())
                                    {
                                        Data.TrnArticlePriceItem newArticlePriceItem = new Data.TrnArticlePriceItem
                                        {
                                            ArticlePriceId = Convert.ToInt32(articlePriceId),
                                            ItemId = articles.FirstOrDefault().Id,
                                            Price = objArticlePriceItem.Price,
                                            TriggerQuantity = objArticlePriceItem.TriggerQuantity
                                        };

                                        db.TrnArticlePriceItems.InsertOnSubmit(newArticlePriceItem);
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No Article.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new article price item if the current item price detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current item price details are not found in the server. Please add new item price first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new article price item in this item price detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this item price detail page.");
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

        // =========================
        // Update Article Price Item
        // =========================
        [Authorize, HttpPut, Route("api/articlePriceItem/update/{id}/{articlePriceId}")]
        public HttpResponseMessage UpdateArticlePriceItem(Entities.TrnArticlePriceItem objArticlePriceItem, String id, String articlePriceId)
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
                                    && d.SysForm.FormName.Equals("ItemPriceDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var articlePrice = from d in db.TrnArticlePrices
                                               where d.Id == Convert.ToInt32(articlePriceId)
                                               select d;

                            if (articlePrice.Any())
                            {
                                if (!articlePrice.FirstOrDefault().IsLocked)
                                {
                                    var articlePriceItem = from d in db.TrnArticlePriceItems
                                                           where d.Id == Convert.ToInt32(id)
                                                           select d;

                                    if (articlePriceItem.Any())
                                    {
                                        var articles = from d in db.MstArticles
                                                       where d.Id == objArticlePriceItem.ItemId
                                                       && d.IsLocked == true
                                                       select d;

                                        if (articles.Any())
                                        {
                                            var updateArticlePriceItem = articlePriceItem.FirstOrDefault();
                                            updateArticlePriceItem.ItemId = articles.FirstOrDefault().Id;
                                            updateArticlePriceItem.Price = objArticlePriceItem.Price;
                                            updateArticlePriceItem.TriggerQuantity = objArticlePriceItem.TriggerQuantity;

                                            db.SubmitChanges();

                                            return Request.CreateResponse(HttpStatusCode.OK);
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.BadRequest, "No Article.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This article price item detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new article price item if the current item price detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current item price details are not found in the server. Please add new item price first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update article price item in this item price detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this item price detail page.");
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

        // =========================
        // Delete Article Price Item
        // =========================
        [Authorize, HttpDelete, Route("api/articlePriceItem/delete/{id}/{articlePriceId}")]
        public HttpResponseMessage DeleteArticlePriceItem(String id, String articlePriceId)
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
                                    && d.SysForm.FormName.Equals("ItemPriceDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var articlePrice = from d in db.TrnArticlePrices
                                               where d.Id == Convert.ToInt32(articlePriceId)
                                               select d;

                            if (articlePrice.Any())
                            {
                                if (!articlePrice.FirstOrDefault().IsLocked)
                                {
                                    var articlePriceItem = from d in db.TrnArticlePriceItems
                                                           where d.Id == Convert.ToInt32(id)
                                                           select d;

                                    if (articlePriceItem.Any())
                                    {
                                        db.TrnArticlePriceItems.DeleteOnSubmit(articlePriceItem.First());
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This article price item detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot apply purchase order items to article price item if the current item price detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current item price details are not found in the server. Please add new item price first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete article price item in this item price detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this item price detail page.");
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
