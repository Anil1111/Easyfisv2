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
    public class ApiMstBankController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

        // =========
        // List Bank
        // =========
        [Authorize, HttpGet, Route("api/bank/list")]
        public List<Entities.MstArticle> ListBank()
        {
            var banks = from d in db.MstArticles.OrderByDescending(d => d.ArticleCode)
                        where d.ArticleTypeId == 5
                        select new Entities.MstArticle
                        {
                            Id = d.Id,
                            ArticleCode = d.ArticleCode,
                            ManualArticleCode = d.ManualArticleCode,
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

            return banks.ToList();
        }

        // ==================================
        // Dropdown List - Bank Group (Field)
        // ==================================
        [Authorize, HttpGet, Route("api/bank/dropdown/list/bankGroup")]
        public List<Entities.MstArticleGroup> DropdownListBankGroup()
        {
            var bankGroups = from d in db.MstArticleGroups.OrderBy(d => d.ArticleGroup)
                             where d.ArticleTypeId == 5
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

            return bankGroups.ToList();
        }

        // ==========================================
        // Dropdown List - Bank Group Account (Field)
        // ==========================================
        [Authorize, HttpGet, Route("api/bank/dropdown/list/bankGroup/account/{accountId}")]
        public List<Entities.MstAccount> DropdownListBankGroupAccount(String accountId)
        {
            var bankGroupAccounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                                    where d.Id == Convert.ToInt32(accountId)
                                    && d.IsLocked == true
                                    select new Entities.MstAccount
                                    {
                                        Id = d.Id,
                                        AccountCode = d.AccountCode,
                                        Account = d.Account
                                    };

            return bankGroupAccounts.ToList();
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
        // Add Bank
        // ========
        [Authorize, HttpPost, Route("api/bank/add")]
        public HttpResponseMessage AddBank(Entities.MstArticle objBank)
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
                                    && d.SysForm.FormName.Equals("BankList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultBankCode = "0000000001";
                            var lastBank = from d in db.MstArticles.OrderByDescending(d => d.Id)
                                           where d.ArticleTypeId == 5
                                           select d;

                            if (lastBank.Any())
                            {
                                var bankCode = Convert.ToInt32(lastBank.FirstOrDefault().ArticleCode) + 0000000001;
                                defaultBankCode = FillLeadingZeroes(bankCode, 10);
                            }

                            var articleGroups = from d in db.MstArticleGroups
                                                where d.Id == objBank.ArticleGroupId
                                                && d.ArticleTypeId == 5
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
                                            var bankByManualCode = from d in db.MstArticles
                                                                   where d.ArticleTypeId == 5
                                                                   && d.ManualArticleCode.Equals(objBank.ManualArticleCode)
                                                                   select d;

                                            if (!bankByManualCode.Any())
                                            {
                                                Data.MstArticle newBank = new Data.MstArticle
                                                {
                                                    ArticleCode = defaultBankCode,
                                                    ManualArticleCode = objBank.ManualArticleCode,
                                                    Article = objBank.Article,
                                                    Category = "NA",
                                                    ArticleTypeId = 5,
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
                                                    Address = objBank.Address,
                                                    TermId = terms.FirstOrDefault().Id,
                                                    ContactNumber = objBank.ContactNumber,
                                                    ContactPerson = objBank.ContactPerson,
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

                                                db.MstArticles.InsertOnSubmit(newBank);
                                                db.SubmitChanges();

                                                String newObject = at.GetObjectString(newBank);
                                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                                return Request.CreateResponse(HttpStatusCode.OK, newBank.Id);
                                            }
                                            else
                                            {
                                                return Request.CreateResponse(HttpStatusCode.BadRequest, "Manual Code is already taken.");
                                            }
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
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No article group found. Please setup at least one article group for banks.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add bank.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this bank page.");
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
        // Update Bank
        // ===========
        [Authorize, HttpPut, Route("api/bank/update/{id}")]
        public HttpResponseMessage UpdateBank(Entities.MstArticle objBank, String id)
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
                                    && d.SysForm.FormName.Equals("BankList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var bank = from d in db.MstArticles
                                       where d.Id == Convert.ToInt32(id)
                                       && d.ArticleTypeId == 5
                                       select d;

                            if (bank.Any())
                            {
                                var bankByManualCode = from d in db.MstArticles
                                                       where d.Id != Convert.ToInt32(id)
                                                       && d.ArticleTypeId == 5
                                                       && d.ManualArticleCode.Equals(objBank.ManualArticleCode)
                                                       select d;

                                if (!bankByManualCode.Any())
                                {
                                    String oldObject = at.GetObjectString(bank.FirstOrDefault());

                                    var lockBank = bank.FirstOrDefault();
                                    lockBank.ManualArticleCode = objBank.ManualArticleCode;
                                    lockBank.Article = objBank.Article;
                                    lockBank.ArticleGroupId = objBank.ArticleGroupId;
                                    lockBank.AccountId = objBank.AccountId;
                                    lockBank.SalesAccountId = objBank.SalesAccountId;
                                    lockBank.CostAccountId = objBank.CostAccountId;
                                    lockBank.AssetAccountId = objBank.AssetAccountId;
                                    lockBank.ExpenseAccountId = objBank.ExpenseAccountId;
                                    lockBank.Address = objBank.Address;
                                    lockBank.ContactNumber = objBank.ContactNumber;
                                    lockBank.ContactPerson = objBank.ContactPerson;
                                    lockBank.IsLocked = true;
                                    lockBank.UpdatedById = currentUserId;
                                    lockBank.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    String newObject = at.GetObjectString(bank.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Manual Code is already taken.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These bank details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update bank.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this bank detail page.");
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
        // Delete Bank
        // ===========
        [Authorize, HttpDelete, Route("api/bank/delete/{id}")]
        public HttpResponseMessage DeleteBank(String id)
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
                                    && d.SysForm.FormName.Equals("BankList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var bank = from d in db.MstArticles
                                       where d.Id == Convert.ToInt32(id)
                                       && d.ArticleTypeId == 5
                                       select d;

                            if (bank.Any())
                            {
                                db.MstArticles.DeleteOnSubmit(bank.First());

                                String oldObject = at.GetObjectString(bank.FirstOrDefault());
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected bank is not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete bank.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this bank page.");
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
