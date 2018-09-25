using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Microsoft.AspNet.Identity;

namespace easyfis.ModifiedApiControllers
{
    public class ApiMstConsignmentController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

        // ================
        // List Consignment
        // ================
        [Authorize, HttpGet, Route("api/consignment/item/list/{supplierId}")]
        public List<Entities.MstArticle> ListConsignmentItem(String supplierId)
        {
            var consignmentItems = from d in db.MstArticles
                                   where d.DefaultSupplierId == Convert.ToInt32(supplierId)
                                   && d.ArticleTypeId == 1
                                   && d.IsLocked == true
                                   && d.IsInventory == true
                                   select new Entities.MstArticle
                                   {
                                       Id = d.Id,
                                       ArticleCode = d.ArticleCode,
                                       ManualArticleCode = d.ManualArticleCode,
                                       Article = d.Article,
                                       ManualArticleOldCode = d.ManualArticleOldCode,
                                       ConsignmentCostPercentage = d.ConsignmentCostPercentage,
                                       ConsignmentCostValue = d.ConsignmentCostValue
                                   };

            return consignmentItems.ToList();
        }

        // ============================
        // Dropdown List - Item (Field)
        // ============================
        [Authorize, HttpGet, Route("api/consignment/item/list/item")]
        public List<Entities.MstArticle> DropdownListConsignmentItemListItem()
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
        // Add & Update Consignment
        // ========================
        [Authorize, HttpPut, Route("api/consignment/item/add/update/{supplierId}")]
        public HttpResponseMessage AddUpdateConsignmentItem(Entities.MstArticle objItem, String supplierId)
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
                                    && d.SysForm.FormName.Equals("SupplierDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var article = from d in db.MstArticles
                                          where d.Id == Convert.ToInt32(supplierId)
                                          select d;

                            if (article.Any())
                            {
                                if (!article.FirstOrDefault().IsLocked)
                                {
                                    var item = from d in db.MstArticles
                                               where d.Id == objItem.Id
                                               && d.ArticleTypeId == 1
                                               && d.IsLocked == true
                                               select d;

                                    if (item.Any())
                                    {
                                        var updateItem = item.FirstOrDefault();
                                        updateItem.DefaultSupplierId = Convert.ToInt32(supplierId);
                                        updateItem.ConsignmentCostPercentage = objItem.ConsignmentCostPercentage;
                                        updateItem.ConsignmentCostValue = objItem.ConsignmentCostValue;
                                        updateItem.IsConsignment = true;
                                        updateItem.IsInventory = true;
                                        db.SubmitChanges();

                                        String newObject = at.GetObjectString(item.FirstOrDefault());
                                        at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "The item you're looking was not found in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new consignment item if the current supplier detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current supplier details are not found in the server. Please add new supplier first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new consignment item in this supplier detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this supplier detail page.");
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
        // Delete Consignment Item
        // =======================
        [Authorize, HttpDelete, Route("api/consignment/item/{itemId}")]
        public HttpResponseMessage DeleteConsignmentItem(String itemId)
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
                                    && d.SysForm.FormName.Equals("SupplierDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var item = from d in db.MstArticles
                                       where d.Id == Convert.ToInt32(itemId)
                                       && d.ArticleTypeId == 1
                                       && d.IsLocked == true
                                       select d;

                            if (item.Any())
                            {
                                var updateItem = item.FirstOrDefault();
                                updateItem.DefaultSupplierId = null;
                                updateItem.ConsignmentCostPercentage = 0;
                                updateItem.ConsignmentCostValue = 0;
                                updateItem.IsConsignment = false;

                                String oldObject = at.GetObjectString(item.FirstOrDefault());
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected consignment item is not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete consignment item.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this supplier detail page.");
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
