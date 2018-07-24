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
    public class ApiMstOtherArticleController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==================
        // List Other Article
        // ==================
        [Authorize, HttpGet, Route("api/otherArticle/list")]
        public List<Entities.MstArticle> ListOtherArticle()
        {
            var otherArticles = from d in db.MstArticles.OrderByDescending(d => d.ArticleCode)
                                where d.ArticleTypeId == 6
                                select new Entities.MstArticle
                                {
                                    Id = d.Id,
                                    ArticleCode = d.ArticleCode,
                                    Article = d.Article,
                                    Address = d.Address,
                                    ContactNumber = d.ContactNumber,
                                    ContactPerson = d.ContactPerson,
                                    ArticleGroupId = d.ArticleGroupId,
                                    ArticleGroup = d.MstArticleGroup.ArticleGroup,
                                    AccountId = d.AccountId,
                                    IsLocked = d.IsLocked,
                                    CreatedBy = d.MstUser.FullName,
                                    CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                    UpdatedBy = d.MstUser1.FullName,
                                    UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                };

            return otherArticles.ToList();
        }

        // ===========================================
        // Dropdown List - Other Article Group (Field)
        // ===========================================
        [Authorize, HttpGet, Route("api/otherArticle/dropdown/list/otherArticleGroup")]
        public List<Entities.MstArticleGroup> DropdownListOtherArticleGroup()
        {
            var otherArticleGroups = from d in db.MstArticleGroups.OrderBy(d => d.ArticleGroup)
                                     where d.ArticleTypeId == 6
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

            return otherArticleGroups.ToList();
        }

        // ===================================================
        // Dropdown List - Other Article Group Account (Field)
        // ===================================================
        [Authorize, HttpGet, Route("api/otherArticle/dropdown/list/otherArticleGroup/account")]
        public List<Entities.MstAccount> DropdownListOtherArticleGroupAccount()
        {
            var otherArticleGroupAccounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                                            where d.IsLocked == true
                                            select new Entities.MstAccount
                                            {
                                                Id = d.Id,
                                                AccountCode = d.AccountCode,
                                                Account = d.Account
                                            };

            return otherArticleGroupAccounts.ToList();
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

        // =================
        // Add Other Article
        // =================
        [Authorize, HttpPost, Route("api/otherArticle/add")]
        public HttpResponseMessage AddOtherArticle(Entities.MstArticle objOtherArticle)
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
                                    && d.SysForm.FormName.Equals("SystemTables")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultOtherArticleCode = "0000000001";
                            var lastOtherArticle = from d in db.MstArticles.OrderByDescending(d => d.Id)
                                                   where d.ArticleTypeId == 6
                                                   select d;

                            if (lastOtherArticle.Any())
                            {
                                var otherArticleCode = Convert.ToInt32(lastOtherArticle.FirstOrDefault().ArticleCode) + 0000000001;
                                defaultOtherArticleCode = FillLeadingZeroes(otherArticleCode, 10);
                            }

                            var articleGroups = from d in db.MstArticleGroups
                                                where d.Id == objOtherArticle.ArticleGroupId
                                                && d.ArticleTypeId == 6
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
                                            Data.MstArticle newOtherArticle = new Data.MstArticle
                                            {
                                                ArticleCode = defaultOtherArticleCode,
                                                ManualArticleCode = "NA",
                                                Article = objOtherArticle.Article,
                                                Category = "NA",
                                                ArticleTypeId = 6,
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
                                                Particulars = "NA",
                                                Address = objOtherArticle.Address,
                                                TermId = terms.FirstOrDefault().Id,
                                                ContactNumber = objOtherArticle.ContactNumber,
                                                ContactPerson = objOtherArticle.ContactPerson,
                                                EmailAddress = "NA",
                                                TaxNumber = "NA",
                                                CreditLimit = 0,
                                                DateAcquired = DateTime.Now,
                                                UsefulLife = 0,
                                                SalvageValue = 0,
                                                ManualArticleOldCode = "NA",
                                                Kitting = 0,
                                                IsLocked = true,
                                                CreatedById = currentUserId,
                                                CreatedDateTime = DateTime.Now,
                                                UpdatedById = currentUserId,
                                                UpdatedDateTime = DateTime.Now
                                            };

                                            db.MstArticles.InsertOnSubmit(newOtherArticle);
                                            db.SubmitChanges();

                                            return Request.CreateResponse(HttpStatusCode.OK, newOtherArticle.Id);
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
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No article group found. Please setup at least one article group for other articles.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add other article.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this system table page.");
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
        // Update Other Article
        // ====================
        [Authorize, HttpPut, Route("api/otherArticle/update/{id}")]
        public HttpResponseMessage UpdateOtherArticle(Entities.MstArticle objOtherArticle, String id)
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
                                    && d.SysForm.FormName.Equals("SystemTables")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var otherArticle = from d in db.MstArticles
                                               where d.Id == Convert.ToInt32(id)
                                               && d.ArticleTypeId == 6
                                               select d;

                            if (otherArticle.Any())
                            {
                                var lockOtherArticle = otherArticle.FirstOrDefault();
                                lockOtherArticle.Article = objOtherArticle.Article;
                                lockOtherArticle.ArticleGroupId = objOtherArticle.ArticleGroupId;
                                lockOtherArticle.AccountId = objOtherArticle.AccountId;
                                lockOtherArticle.SalesAccountId = objOtherArticle.SalesAccountId;
                                lockOtherArticle.CostAccountId = objOtherArticle.CostAccountId;
                                lockOtherArticle.AssetAccountId = objOtherArticle.AssetAccountId;
                                lockOtherArticle.ExpenseAccountId = objOtherArticle.ExpenseAccountId;
                                lockOtherArticle.Address = objOtherArticle.Address;
                                lockOtherArticle.ContactNumber = objOtherArticle.ContactNumber;
                                lockOtherArticle.ContactPerson = objOtherArticle.ContactPerson;
                                lockOtherArticle.IsLocked = true;
                                lockOtherArticle.UpdatedById = currentUserId;
                                lockOtherArticle.UpdatedDateTime = DateTime.Now;

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These other article details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update other article.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this system tables page.");
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
        // Delete Other Article
        // ====================
        [Authorize, HttpDelete, Route("api/otherArticle/delete/{id}")]
        public HttpResponseMessage DeleteOtherArticle(String id)
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
                                    && d.SysForm.FormName.Equals("SystemTables")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var otherArticle = from d in db.MstArticles
                                               where d.Id == Convert.ToInt32(id)
                                               && d.ArticleTypeId == 6
                                               select d;

                            if (otherArticle.Any())
                            {
                                db.MstArticles.DeleteOnSubmit(otherArticle.First());
                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected other article is not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete other article.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this system table page.");
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
