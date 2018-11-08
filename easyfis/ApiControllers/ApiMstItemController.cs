using System;
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
    public class ApiMstItemController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========
        // Business
        // ========
        private Business.AuditTrail auditTrail = new Business.AuditTrail();

        // =========
        // List Item
        // =========
        [Authorize, HttpGet, Route("api/item/list")]
        public List<Entities.MstArticle> ListItem()
        {
            var items = from d in db.MstArticles.OrderByDescending(d => d.ArticleCode)
                        where d.ArticleTypeId == 1
                        select new Entities.MstArticle
                        {
                            Id = d.Id,
                            ArticleCode = d.ArticleCode,
                            ManualArticleCode = d.ManualArticleCode,
                            Article = d.Article,
                            Category = d.Category,
                            Unit = d.MstUnit.Unit,
                            Price = d.Price,
                            IsInventory = d.IsInventory,
                            ManualArticleOldCode = d.ManualArticleOldCode,
                            IsLocked = d.IsLocked,
                            CreatedById = d.CreatedById,
                            CreatedBy = d.MstUser.FullName,
                            CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                            UpdatedById = d.UpdatedById,
                            UpdatedBy = d.MstUser1.FullName,
                            UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                        };

            return items.ToList();
        }

        // ==================================
        // Dropdown List - Item Group (Field)
        // ==================================
        [Authorize, HttpGet, Route("api/item/dropdown/list/itemGroup")]
        public List<Entities.MstArticleGroup> DropdownListItemGroup()
        {
            var itemGroups = from d in db.MstArticleGroups.OrderBy(d => d.ArticleGroup)
                             where d.ArticleTypeId == 1
                             && d.IsLocked == true
                             select new Entities.MstArticleGroup
                             {
                                 Id = d.Id,
                                 ArticleGroup = d.ArticleGroup,
                                 AccountId = d.AccountId,
                                 SalesAccountId = d.SalesAccountId,
                                 CostAccountId = d.CostAccountId,
                                 AssetAccountId = d.AssetAccountId,
                                 ExpenseAccountId = d.ExpenseAccountId
                             };

            return itemGroups.ToList();
        }

        // ==========================================
        // Dropdown List - Item Group Account (Field)
        // ==========================================
        [Authorize, HttpGet, Route("api/item/dropdown/list/itemGroup/accounts")]
        public List<Entities.MstAccount> DropdownListItemGroupAccount()
        {
            var itemGroupAccounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                                    where d.IsLocked == true
                                    select new Entities.MstAccount
                                    {
                                        Id = d.Id,
                                        AccountCode = d.AccountCode,
                                        Account = d.Account
                                    };

            return itemGroupAccounts.ToList();
        }

        // =================================
        // Dropdown List - Item Unit (Field)
        // =================================
        [Authorize, HttpGet, Route("api/item/dropdown/list/unit")]
        public List<Entities.MstUnit> DropdownListItemUnit()
        {
            var itemUnits = from d in db.MstUnits.OrderBy(d => d.Unit)
                            where d.IsLocked == true
                            select new Entities.MstUnit
                            {
                                Id = d.Id,
                                Unit = d.Unit
                            };

            return itemUnits.ToList();
        }

        // =====================================
        // Dropdown List - Item Tax Type (Field)
        // =====================================
        [Authorize, HttpGet, Route("api/item/dropdown/list/taxType")]
        public List<Entities.MstTaxType> DropdownListItemTaxType()
        {
            var itemTaxTypes = from d in db.MstTaxTypes.OrderBy(d => d.TaxType)
                               where d.IsLocked == true
                               select new Entities.MstTaxType
                               {
                                   Id = d.Id,
                                   TaxType = d.TaxType
                               };

            return itemTaxTypes.ToList();
        }

        // =====================================
        // Dropdown List - Item Supplier (Field)
        // =====================================
        [Authorize, HttpGet, Route("api/item/dropdown/list/supplier")]
        public List<Entities.MstArticle> DropdownListItemSupplier()
        {
            var itemSuppliers = from d in db.MstArticles.OrderBy(d => d.Article)
                                where d.IsLocked == true
                                && d.ArticleTypeId == 3
                                select new Entities.MstArticle
                                {
                                    Id = d.Id,
                                    Article = d.Article
                                };

            return itemSuppliers.ToList();
        }

        // ===========
        // Detail Item
        // ===========
        [Authorize, HttpGet, Route("api/item/detail/{id}")]
        public Entities.MstArticle DetailItem(String id)
        {
            var item = from d in db.MstArticles
                       where d.Id == Convert.ToInt32(id)
                       && d.ArticleTypeId == 1
                       select new Entities.MstArticle
                       {
                           Id = d.Id,
                           ArticleCode = d.ArticleCode,
                           ManualArticleCode = d.ManualArticleCode,
                           Article = d.Article,
                           ArticleGroupId = d.ArticleGroupId,
                           AccountId = d.AccountId,
                           SalesAccountId = d.SalesAccountId,
                           CostAccountId = d.CostAccountId,
                           AssetAccountId = d.AssetAccountId,
                           ExpenseAccountId = d.ExpenseAccountId,
                           Category = d.Category,
                           UnitId = d.UnitId,
                           Price = d.Price,
                           Particulars = d.Particulars,
                           InputTaxId = d.InputTaxId,
                           OutputTaxId = d.OutputTaxId,
                           WTaxTypeId = d.WTaxTypeId,
                           IsInventory = d.IsInventory,
                           IsConsignment = d.IsConsignment,
                           ConsignmentCostPercentage = d.ConsignmentCostPercentage,
                           ConsignmentCostValue = d.ConsignmentCostValue,
                           ManualArticleOldCode = d.ManualArticleOldCode,
                           Cost = d.Cost,
                           Kitting = d.Kitting,
                           DateAcquired = d.DateAcquired.ToShortDateString(),
                           UsefulLife = d.UsefulLife,
                           SalvageValue = d.SalvageValue,
                           DefaultSupplierId = d.DefaultSupplierId,
                           IsLocked = d.IsLocked,
                           CreatedById = d.CreatedById,
                           CreatedBy = d.MstUser.FullName,
                           CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                           UpdatedById = d.UpdatedById,
                           UpdatedBy = d.MstUser1.FullName,
                           UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                       };

            return item.FirstOrDefault();
        }

        // ===================
        // Fill Leading Zeroes
        // ===================
        public String FillLeadingZeroes(Int32 number, Int32 length)
        {
            var result = number.ToString();
            var pad = length - result.Length;
            while (pad > 0)
            {
                result = '0' + result;
                pad--;
            }

            return result;
        }

        // ========
        // Add Item
        // ========
        [Authorize, HttpPost, Route("api/item/add")]
        public HttpResponseMessage AddItem()
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
                                    && d.SysForm.FormName.Equals("ItemList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultItemCode = "0000000001";
                            var lastItem = from d in db.MstArticles.OrderByDescending(d => d.Id)
                                           where d.ArticleTypeId == 1
                                           select d;

                            if (lastItem.Any())
                            {
                                var itemCode = Convert.ToInt32(lastItem.FirstOrDefault().ArticleCode) + 0000000001;
                                defaultItemCode = FillLeadingZeroes(itemCode, 10);
                            }

                            var articleGroups = from d in db.MstArticleGroups
                                                where d.ArticleTypeId == 1
                                                select d;

                            if (articleGroups.Any())
                            {
                                var units = from d in db.MstUnits
                                            select d;

                                if (units.Any())
                                {
                                    var taxTypes = from d in db.MstTaxTypes
                                                   select d;

                                    if (taxTypes.Any())
                                    {
                                        var terms = from d in db.MstTerms
                                                    select d;

                                        if (terms.Any())
                                        {
                                            Data.MstArticle newItem = new Data.MstArticle
                                            {
                                                ArticleCode = defaultItemCode,
                                                ManualArticleCode = defaultItemCode,
                                                Article = "NA",
                                                Category = "NA",
                                                ArticleTypeId = 1,
                                                ArticleGroupId = articleGroups.FirstOrDefault().Id,
                                                AccountId = articleGroups.FirstOrDefault().AccountId,
                                                SalesAccountId = articleGroups.FirstOrDefault().SalesAccountId,
                                                CostAccountId = articleGroups.FirstOrDefault().CostAccountId,
                                                AssetAccountId = articleGroups.FirstOrDefault().AssetAccountId,
                                                ExpenseAccountId = articleGroups.FirstOrDefault().ExpenseAccountId,
                                                UnitId = units.FirstOrDefault().Id,
                                                OutputTaxId = db.MstTaxTypes.FirstOrDefault().Id,
                                                InputTaxId = db.MstTaxTypes.FirstOrDefault().Id,
                                                WTaxTypeId = db.MstTaxTypes.FirstOrDefault().Id,
                                                Price = 0,
                                                Cost = 0,
                                                IsInventory = false,
                                                IsConsignment = false,
                                                ConsignmentCostPercentage = 0,
                                                ConsignmentCostValue = 0,
                                                Particulars = "NA",
                                                Address = "NA",
                                                TermId = terms.FirstOrDefault().Id,
                                                ContactNumber = "NA",
                                                ContactPerson = "NA",
                                                EmailAddress = "NA",
                                                TaxNumber = "NA",
                                                CreditLimit = 0,
                                                DateAcquired = DateTime.Now,
                                                UsefulLife = 0,
                                                SalvageValue = 0,
                                                ManualArticleOldCode = "NA",
                                                Kitting = 0,
                                                DefaultSupplierId = null,
                                                IsLocked = false,
                                                CreatedById = currentUserId,
                                                CreatedDateTime = DateTime.Now,
                                                UpdatedById = currentUserId,
                                                UpdatedDateTime = DateTime.Now
                                            };

                                            db.MstArticles.InsertOnSubmit(newItem);
                                            db.SubmitChanges();

                                            String newObject = auditTrail.GetObjectString(newItem);
                                            auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                            return Request.CreateResponse(HttpStatusCode.OK, newItem.Id);
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.NotFound, "No term found. Please setup more terms for all master tables.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "No tax type found. Please setup more tax types for all master tables.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.NotFound, "No unit found. Please setup more units for all master tables.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No article group found. Please setup at least one article group for items.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add item.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this item page.");
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

        // =========
        // Save Item
        // =========
        [Authorize, HttpPut, Route("api/item/save/{id}")]
        public HttpResponseMessage SaveItem(Entities.MstArticle objItem, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var item = from d in db.MstArticles
                               where d.Id == Convert.ToInt32(id)
                               && d.ArticleTypeId == 1
                               select d;

                    if (item.Any())
                    {
                        if (!item.FirstOrDefault().IsLocked)
                        {
                            String oldObject = auditTrail.GetObjectString(item.FirstOrDefault());

                            if (item.FirstOrDefault().MstArticleUnits.Any())
                            {
                                if (item.FirstOrDefault().MstArticlePrices.Any())
                                {
                                    var saveItem = item.FirstOrDefault();
                                    saveItem.ManualArticleCode = objItem.ManualArticleCode;
                                    saveItem.Article = objItem.Article;
                                    saveItem.ArticleGroupId = objItem.ArticleGroupId;
                                    saveItem.AccountId = objItem.AccountId;
                                    saveItem.SalesAccountId = objItem.SalesAccountId;
                                    saveItem.CostAccountId = objItem.CostAccountId;
                                    saveItem.AssetAccountId = objItem.AssetAccountId;
                                    saveItem.ExpenseAccountId = objItem.ExpenseAccountId;
                                    saveItem.Category = objItem.Category;
                                    saveItem.UnitId = objItem.UnitId;
                                    saveItem.Price = objItem.Price;
                                    saveItem.Particulars = objItem.Particulars;
                                    saveItem.InputTaxId = objItem.InputTaxId;
                                    saveItem.OutputTaxId = objItem.OutputTaxId;
                                    saveItem.WTaxTypeId = objItem.WTaxTypeId;
                                    saveItem.IsInventory = objItem.IsInventory;
                                    saveItem.IsConsignment = objItem.IsConsignment;
                                    saveItem.ConsignmentCostPercentage = objItem.ConsignmentCostPercentage;
                                    saveItem.ConsignmentCostValue = objItem.ConsignmentCostValue;
                                    saveItem.ManualArticleOldCode = objItem.ManualArticleOldCode;
                                    saveItem.Cost = objItem.Cost;
                                    saveItem.Kitting = objItem.Kitting;
                                    saveItem.DefaultSupplierId = objItem.DefaultSupplierId;
                                    saveItem.DateAcquired = Convert.ToDateTime(objItem.DateAcquired);
                                    saveItem.UsefulLife = objItem.UsefulLife;
                                    saveItem.SalvageValue = objItem.SalvageValue;
                                    saveItem.UpdatedById = currentUserId;
                                    saveItem.UpdatedDateTime = DateTime.Now;
                                    db.SubmitChanges();

                                    String newObject = auditTrail.GetObjectString(item.FirstOrDefault());
                                    auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.NotFound, "No Price Found. Please provide at least one price.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No Unit Conversion Found. Please provide at least one unit conversion.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Saving Error. These item details are already locked.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These item details are not found in the server.");
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

        // =========
        // Lock Item
        // =========
        [Authorize, HttpPut, Route("api/item/lock/{id}")]
        public HttpResponseMessage LockItem(Entities.MstArticle objItem, String id)
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
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var item = from d in db.MstArticles
                                       where d.Id == Convert.ToInt32(id)
                                       && d.ArticleTypeId == 1
                                       select d;

                            if (item.Any())
                            {
                                if (!item.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = auditTrail.GetObjectString(item.FirstOrDefault());

                                    if (item.FirstOrDefault().MstArticleUnits.Any())
                                    {
                                        if (item.FirstOrDefault().MstArticlePrices.Any())
                                        {
                                            var itemByManualCode = from d in db.MstArticles
                                                                   where d.ArticleTypeId == 1
                                                                   && d.ManualArticleCode.Equals(objItem.ManualArticleCode)
                                                                   && d.IsLocked == true
                                                                   select d;

                                            if (!itemByManualCode.Any())
                                            {
                                                var lockItem = item.FirstOrDefault();
                                                lockItem.ManualArticleCode = objItem.ManualArticleCode;
                                                lockItem.Article = objItem.Article;
                                                lockItem.ArticleGroupId = objItem.ArticleGroupId;
                                                lockItem.AccountId = objItem.AccountId;
                                                lockItem.SalesAccountId = objItem.SalesAccountId;
                                                lockItem.CostAccountId = objItem.CostAccountId;
                                                lockItem.AssetAccountId = objItem.AssetAccountId;
                                                lockItem.ExpenseAccountId = objItem.ExpenseAccountId;
                                                lockItem.Category = objItem.Category;
                                                lockItem.UnitId = objItem.UnitId;
                                                lockItem.Price = objItem.Price;
                                                lockItem.Particulars = objItem.Particulars;
                                                lockItem.InputTaxId = objItem.InputTaxId;
                                                lockItem.OutputTaxId = objItem.OutputTaxId;
                                                lockItem.WTaxTypeId = objItem.WTaxTypeId;
                                                lockItem.IsInventory = objItem.IsInventory;
                                                lockItem.IsConsignment = objItem.IsConsignment;
                                                lockItem.ConsignmentCostPercentage = objItem.ConsignmentCostPercentage;
                                                lockItem.ConsignmentCostValue = objItem.ConsignmentCostValue;
                                                lockItem.ManualArticleOldCode = objItem.ManualArticleOldCode;
                                                lockItem.Cost = objItem.Cost;
                                                lockItem.Kitting = objItem.Kitting;
                                                lockItem.DefaultSupplierId = objItem.DefaultSupplierId;
                                                lockItem.DateAcquired = Convert.ToDateTime(objItem.DateAcquired);
                                                lockItem.UsefulLife = objItem.UsefulLife;
                                                lockItem.SalvageValue = objItem.SalvageValue;
                                                lockItem.IsLocked = true;
                                                lockItem.UpdatedById = currentUserId;
                                                lockItem.UpdatedDateTime = DateTime.Now;

                                                db.SubmitChanges();

                                                String newObject = auditTrail.GetObjectString(item.FirstOrDefault());
                                                auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                                return Request.CreateResponse(HttpStatusCode.OK);
                                            }
                                            else
                                            {
                                                return Request.CreateResponse(HttpStatusCode.BadRequest, "Manual Code is already taken.");
                                            }
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.NotFound, "No Price Found. Please provide at least one price.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "No Unit Conversion Found. Please provide at least one unit conversion.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These item details are already locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These item details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock item.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this item detail page.");
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

        // ===========
        // Unlock Item
        // ===========
        [Authorize, HttpPut, Route("api/item/unlock/{id}")]
        public HttpResponseMessage UnlockItem(String id)
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
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var item = from d in db.MstArticles
                                       where d.Id == Convert.ToInt32(id)
                                       && d.ArticleTypeId == 1
                                       select d;

                            if (item.Any())
                            {
                                if (item.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = auditTrail.GetObjectString(item.FirstOrDefault());

                                    var unlockItem = item.FirstOrDefault();
                                    unlockItem.IsLocked = false;
                                    unlockItem.UpdatedById = currentUserId;
                                    unlockItem.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    String newObject = auditTrail.GetObjectString(item.FirstOrDefault());
                                    auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These item details are already unlocked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These item details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock item.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this item detail page.");
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

        // ===========
        // Delete Item
        // ===========
        [Authorize, HttpDelete, Route("api/item/delete/{id}")]
        public HttpResponseMessage DeleteItem(String id)
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
                                    && d.SysForm.FormName.Equals("ItemList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var item = from d in db.MstArticles
                                       where d.Id == Convert.ToInt32(id)
                                       && d.ArticleTypeId == 1
                                       select d;

                            if (item.Any())
                            {
                                db.MstArticles.DeleteOnSubmit(item.First());

                                String oldObject = auditTrail.GetObjectString(item.FirstOrDefault());
                                auditTrail.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected item is not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete item.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this item page.");
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
