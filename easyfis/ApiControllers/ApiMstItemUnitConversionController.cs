﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;
using System.Reflection;

namespace easyfis.ModifiedApiControllers
{
    public class ApiMstItemUnitConversionController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

        // =========================
        // List Item Unit Conversion
        // =========================
        [Authorize, HttpGet, Route("api/unitConversion/list/{itemId}")]
        public List<Entities.MstArticleUnit> ListItemUnitConversion(String itemId)
        {
            var unitConversions = from d in db.MstArticleUnits
                                  where d.ArticleId == Convert.ToInt32(itemId)
                                  select new Entities.MstArticleUnit
                                  {
                                      Id = d.Id,
                                      Multiplier = d.Multiplier,
                                      UnitId = d.UnitId,
                                      Unit = d.MstUnit.Unit,
                                      IsCountUnit = d.IsCountUnit
                                  };

            return unitConversions.ToList();
        }

        // ===================================================
        // Dropdown List - Item Unit Conversion - Unit (Field)
        // ===================================================
        [Authorize, HttpGet, Route("api/unitConversion/dropdown/list/unit")]
        public List<Entities.MstUnit> DropdownListItemUnitConversionUnit()
        {
            var unitConversionsUnit = from d in db.MstUnits.OrderBy(d => d.Unit)
                                      where d.IsLocked == true
                                      select new Entities.MstUnit
                                      {
                                          Id = d.Id,
                                          Unit = d.Unit
                                      };

            return unitConversionsUnit.ToList();
        }

        // ========================
        // Add Item Unit Conversion
        // ========================
        [Authorize, HttpPost, Route("api/unitConversion/add/{itemId}")]
        public HttpResponseMessage AddItemUnitConversion(Entities.MstArticleUnit objUnitConversion, String itemId)
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
                                    && d.SysForm.FormName.Equals("ItemDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var item = from d in db.MstArticles
                                       where d.Id == Convert.ToInt32(itemId)
                                       && d.ArticleTypeId == 1
                                       select d;

                            if (item.Any())
                            {
                                if (!item.FirstOrDefault().IsLocked)
                                {
                                    Data.MstArticleUnit newItemUnitConversion = new Data.MstArticleUnit
                                    {
                                        ArticleId = Convert.ToInt32(itemId),
                                        UnitId = objUnitConversion.UnitId,
                                        Multiplier = objUnitConversion.Multiplier,
                                        IsCountUnit = objUnitConversion.IsCountUnit
                                    };

                                    db.MstArticleUnits.InsertOnSubmit(newItemUnitConversion);
                                    db.SubmitChanges();

                                    String newObject = at.GetObjectString(newItemUnitConversion);
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new unit conversion if the current item detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current item details are not found in the server. Please add new item first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new unit conversion in this item detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this item detail page.");
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

        // ===========================
        // Update Item Unit Conversion
        // ===========================
        [Authorize, HttpPut, Route("api/unitConversion/update/{id}/{itemId}")]
        public HttpResponseMessage UpdateItemUnitConversion(Entities.MstArticleUnit objUnitConversion, String id, String itemId)
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
                                    && d.SysForm.FormName.Equals("ItemDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var item = from d in db.MstArticles
                                       where d.Id == Convert.ToInt32(itemId)
                                       && d.ArticleTypeId == 1
                                       select d;

                            if (item.Any())
                            {
                                if (!item.FirstOrDefault().IsLocked)
                                {
                                    var unitConversion = from d in db.MstArticleUnits
                                                         where d.Id == Convert.ToInt32(id)
                                                         select d;

                                    if (unitConversion.Any())
                                    {
                                        String oldObject = at.GetObjectString(unitConversion.FirstOrDefault());

                                        var updateUnitConversion = unitConversion.FirstOrDefault();
                                        updateUnitConversion.UnitId = objUnitConversion.UnitId;
                                        updateUnitConversion.Multiplier = objUnitConversion.Multiplier;
                                        updateUnitConversion.IsCountUnit = objUnitConversion.IsCountUnit;
                                        db.SubmitChanges();

                                        String newObject = at.GetObjectString(unitConversion.FirstOrDefault());
                                        at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This unit conversion detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot update unit conversion if the current item detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current item details are not found in the server. Please add new item first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to update unit conversion in this item detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this item detail page.");
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

        // ===========================
        // Delete Item Unit Conversion
        // ===========================
        [Authorize, HttpDelete, Route("api/unitConversion/delete/{id}/{itemId}")]
        public HttpResponseMessage DeleteItemUnitConversion(String id, String itemId)
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
                                    && d.SysForm.FormName.Equals("ItemDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var item = from d in db.MstArticles
                                       where d.Id == Convert.ToInt32(itemId)
                                       && d.ArticleTypeId == 1
                                       select d;

                            if (item.Any())
                            {
                                if (!item.FirstOrDefault().IsLocked)
                                {
                                    var unitConversion = from d in db.MstArticleUnits
                                                         where d.Id == Convert.ToInt32(id)
                                                         select d;

                                    if (unitConversion.Any())
                                    {
                                        db.MstArticleUnits.DeleteOnSubmit(unitConversion.First());

                                        String oldObject = at.GetObjectString(unitConversion.FirstOrDefault());
                                        at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This unit conversion detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot delete unit conversion if the current item detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current item details are not found in the server. Please add new item first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete unit conversion in this item detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this item detail page.");
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
