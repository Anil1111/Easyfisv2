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
    public class ApiTrnCollectionLineController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ====================
        // List Collection Line
        // ====================
        [Authorize, HttpGet, Route("api/collectionLine/list/{ORId}")]
        public List<Entities.TrnCollectionLine> ListCollectionLine(String ORId)
        {
            var collectionLines = from d in db.TrnCollectionLines
                                  where d.ORId == Convert.ToInt32(ORId)
                                  select new Entities.TrnCollectionLine
                                  {
                                      Id = d.Id,
                                      ORId = d.ORId,
                                      BranchId = d.BranchId,
                                      Branch = d.MstBranch.Branch,
                                      AccountId = d.AccountId,
                                      Account = d.MstAccount.Account,
                                      ArticleId = d.ArticleId,
                                      Article = d.MstArticle.Article,
                                      SIId = d.SIId,
                                      SINumber = d.SIId != null ? d.TrnSalesInvoice.SINumber : "",
                                      Particulars = d.Particulars,
                                      Amount = d.Amount,
                                      PayTypeId = d.PayTypeId,
                                      PayType = d.MstPayType.PayType,
                                      CheckNumber = d.CheckNumber,
                                      CheckDate = d.CheckDate.ToShortDateString(),
                                      CheckBank = d.CheckBank,
                                      DepositoryBankId = d.DepositoryBankId,
                                      DepositoryBank = d.MstArticle1.Article,
                                      IsClear = d.IsClear
                                  };

            return collectionLines.ToList();
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/collectionLine/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListCollectionLineBranch()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var companyId = currentUser.FirstOrDefault().CompanyId;

            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           where d.CompanyId == companyId
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // ===============================
        // Dropdown List - Account (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/collectionLine/dropdown/list/account")]
        public List<Entities.MstAccount> DropdownListCollectionLineAccount()
        {
            var accounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                           where d.IsLocked == true
                           select new Entities.MstAccount
                           {
                               Id = d.Id,
                               AccountCode = d.AccountCode,
                               Account = d.Account
                           };

            return accounts.ToList();
        }

        // ===============================
        // Dropdown List - Article (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/collectionLine/dropdown/list/article/{accountId}")]
        public List<Entities.MstArticle> DropdownListCollectionLineArticle(String accountId)
        {
            List<Entities.MstArticle> listArticles = new List<Entities.MstArticle>();

            var accountArticleTypes = from d in db.MstAccountArticleTypes
                                      where d.AccountId == Convert.ToInt32(accountId)
                                      && d.MstAccount.IsLocked == true
                                      select d;

            if (accountArticleTypes.Any())
            {
                foreach (var accountArticleType in accountArticleTypes)
                {
                    var articles = from d in db.MstArticles
                                   where d.ArticleTypeId == accountArticleType.ArticleTypeId
                                   && d.IsLocked == true
                                   select d;

                    if (articles.Any())
                    {
                        foreach (var article in articles)
                        {
                            listArticles.Add(new Entities.MstArticle()
                            {
                                Id = article.Id,
                                Article = article.Article
                            });
                        }
                    }
                }
            }

            return listArticles.OrderBy(d => d.Article).ToList();
        }

        // =====================================
        // Dropdown List - Sales Invoice (Field)
        // =====================================
        [Authorize, HttpGet, Route("api/collectionLine/dropdown/list/salesInvoice/{customerId}")]
        public List<Entities.TrnSalesInvoice> DropdownListCollectionLineSalesInvoice(String customerId)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var salesInvoices = from d in db.TrnSalesInvoices.OrderByDescending(d => d.Id)
                                where d.CustomerId == Convert.ToInt32(customerId)
                                && d.BranchId == branchId
                                && d.BalanceAmount > 0
                                && d.IsLocked == true
                                select new Entities.TrnSalesInvoice
                                {
                                    Id = d.Id,
                                    SINumber = d.SINumber,
                                    SIDate = d.SIDate.ToShortDateString(),
                                    BalanceAmount = d.BalanceAmount
                                };

            return salesInvoices.ToList();
        }

        // ================================
        // Dropdown List - Pay Type (Field)
        // ================================
        [Authorize, HttpGet, Route("api/collectionLine/dropdown/list/payType")]
        public List<Entities.MstPayType> DropdownListCollectionLinePayType()
        {
            var payTypes = from d in db.MstPayTypes.OrderBy(d => d.PayType)
                           where d.IsLocked == true
                           select new Entities.MstPayType
                           {
                               Id = d.Id,
                               PayType = d.PayType
                           };

            return payTypes.ToList();
        }

        // =======================================
        // Dropdown List - Depository Bank (Field)
        // =======================================
        [Authorize, HttpGet, Route("api/collectionLine/dropdown/list/depositoryBank")]
        public List<Entities.MstArticle> DropdownListCollectionLineDepositoryBank()
        {
            var depositoryBanks = from d in db.MstArticles.OrderBy(d => d.Article)
                                  where d.ArticleTypeId == 5
                                  && d.IsLocked == true
                                  select new Entities.MstArticle
                                  {
                                      Id = d.Id,
                                      Article = d.Article
                                  };

            return depositoryBanks.ToList();
        }

        // ==================================
        // Pop-Up List - Sales Invoice Status
        // ==================================
        [Authorize, HttpGet, Route("api/collectionLine/popUp/list/salesInvoiceStatus/{customerId}/{startDate}/{endDate}")]
        public List<Entities.TrnSalesInvoice> PopUpListCollectionLineListSalesInvoiceStatus(String customerId, String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var salesInvoices = from d in db.TrnSalesInvoices.OrderByDescending(d => d.Id)
                                where d.CustomerId == Convert.ToInt32(customerId)
                                && d.SIDate >= Convert.ToDateTime(startDate)
                                && d.SIDate <= Convert.ToDateTime(endDate)
                                && d.BranchId == branchId
                                && d.BalanceAmount > 0
                                && d.IsLocked == true
                                select new Entities.TrnSalesInvoice
                                {
                                    Id = d.Id,
                                    SINumber = d.SINumber,
                                    SIDate = d.SIDate.ToShortDateString(),
                                    ManualSINumber = d.ManualSINumber,
                                    Amount = d.Amount,
                                    PaidAmount = d.PaidAmount,
                                    AdjustmentAmount = d.AdjustmentAmount,
                                    BalanceAmount = d.BalanceAmount
                                };

            return salesInvoices.ToList();
        }

        // =======================================================
        // Apply (Download) Collection Line - Sales Invoice Status
        // =======================================================
        [Authorize, HttpPost, Route("api/collectionLine/popUp/apply/salesInvoiceStatus/{ORId}")]
        public HttpResponseMessage ApplySalesInvoiceStatusCollectionLine(List<Entities.TrnCollectionLine> objCollectionLines, String ORId)
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
                                    && d.SysForm.FormName.Equals("CollectionDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var collection = from d in db.TrnCollections
                                             where d.Id == Convert.ToInt32(ORId)
                                             select d;

                            if (collection.Any())
                            {
                                if (!collection.FirstOrDefault().IsLocked)
                                {
                                    foreach (var objCollectionLine in objCollectionLines)
                                    {
                                        var salesInvoice = from d in db.TrnSalesInvoices
                                                           where d.Id == objCollectionLine.SIId
                                                           && d.BranchId == currentBranchId
                                                           && d.IsLocked == true
                                                           select d;

                                        if (salesInvoice.Any())
                                        {
                                            var payTypes = from d in db.MstPayTypes
                                                           where d.IsLocked == true
                                                           && d.Id == objCollectionLine.PayTypeId
                                                           select d;

                                            if (payTypes.Any())
                                            {
                                                var depositoryBanks = from d in db.MstArticles
                                                                      where d.ArticleTypeId == 5
                                                                      && d.IsLocked == true
                                                                      && d.Id == objCollectionLine.DepositoryBankId
                                                                      select d;

                                                if (depositoryBanks.Any())
                                                {
                                                    Data.TrnCollectionLine newCollectionLine = new Data.TrnCollectionLine
                                                    {
                                                        ORId = Convert.ToInt32(ORId),
                                                        BranchId = salesInvoice.FirstOrDefault().BranchId,
                                                        AccountId = salesInvoice.FirstOrDefault().MstArticle.AccountId,
                                                        ArticleId = salesInvoice.FirstOrDefault().CustomerId,
                                                        SIId = salesInvoice.FirstOrDefault().Id,
                                                        Particulars = salesInvoice.FirstOrDefault().Remarks,
                                                        Amount = objCollectionLine.Amount,
                                                        PayTypeId = payTypes.FirstOrDefault().Id,
                                                        CheckNumber = objCollectionLine.CheckNumber,
                                                        CheckDate = Convert.ToDateTime(objCollectionLine.CheckDate),
                                                        CheckBank = objCollectionLine.CheckBank,
                                                        DepositoryBankId = depositoryBanks.FirstOrDefault().Id,
                                                        IsClear = objCollectionLine.IsClear
                                                    };

                                                    db.TrnCollectionLines.InsertOnSubmit(newCollectionLine);
                                                    db.SubmitChanges();
                                                }
                                            }
                                        }
                                    }

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new collection line if the current collection detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current collection details are not found in the server. Please add new collection first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new collection line in this collection detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this collection detail page.");
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

        // ===============================
        // Pop-Up List - Customer Advances
        // ===============================
        [Authorize, HttpGet, Route("api/collectionLine/popUp/list/customerAdvances/{customerId}")]
        public List<Entities.TrnJournal> PopUpListCollectionLineListCustomerAdvances(String customerId)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;
            var customerAdvancesAccountId = currentUser.FirstOrDefault().CustomerAdvancesAccountId;

            var journals = from d in db.TrnJournals
                           where d.ArticleId == Convert.ToInt32(customerId)
                           && d.AccountId == customerAdvancesAccountId
                           && d.BranchId == branchId
                           group d by new
                           {
                               BranchId = d.BranchId,
                               Branch = d.MstBranch.Branch,
                               AccountId = d.AccountId,
                               Account = d.MstAccount.Account,
                               AccountCode = d.MstAccount.AccountCode,
                               ArticleId = d.ArticleId,
                               Article = d.MstArticle.Article
                           } into g
                           select new Entities.TrnJournal
                           {
                               BranchId = g.Key.BranchId,
                               Branch = g.Key.Branch,
                               AccountId = g.Key.AccountId,
                               Account = g.Key.Account,
                               AccountCode = g.Key.AccountCode,
                               ArticleId = g.Key.ArticleId,
                               Article = g.Key.Article,
                               DebitAmount = g.Sum(d => d.DebitAmount),
                               CreditAmount = g.Sum(d => d.CreditAmount),
                               Balance = g.Sum(d => d.CreditAmount) - g.Sum(d => d.DebitAmount)
                           };

            return journals.Where(d => d.Balance != 0).ToList();
        }

        // ====================================================
        // Apply (Download) Collection Line - Customer Advances
        // ====================================================
        [Authorize, HttpPost, Route("api/collectionLine/popUp/apply/customerAdvances/{ORId}")]
        public HttpResponseMessage ApplyCustomerAdvancesCollectionLine(Entities.TrnCollectionLine objCollectionLine, String ORId)
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
                                    && d.SysForm.FormName.Equals("CollectionDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var collection = from d in db.TrnCollections
                                             where d.Id == Convert.ToInt32(ORId)
                                             select d;

                            if (collection.Any())
                            {
                                if (!collection.FirstOrDefault().IsLocked)
                                {
                                    var journals = from d in db.TrnJournals
                                                   where d.ArticleId == objCollectionLine.ArticleId
                                                   && d.AccountId == objCollectionLine.AccountId
                                                   && d.BranchId == currentBranchId
                                                   group d by new
                                                   {
                                                       BranchId = d.BranchId,
                                                       AccountId = d.AccountId,
                                                       ArticleId = d.ArticleId,
                                                   } into g
                                                   select new
                                                   {
                                                       BranchId = g.Key.BranchId,
                                                       AccountId = g.Key.AccountId,
                                                       ArticleId = g.Key.ArticleId,
                                                       DebitAmount = g.Sum(d => d.DebitAmount),
                                                       CreditAmount = g.Sum(d => d.CreditAmount),
                                                       BalanceAmount = g.Sum(d => d.CreditAmount) - g.Sum(d => d.DebitAmount)
                                                   };

                                    if (journals.Any())
                                    {
                                        var advances = from d in journals.ToList()
                                                       select new
                                                       {
                                                           BranchId = d.BranchId,
                                                           AccountId = d.AccountId,
                                                           ArticleId = d.ArticleId,
                                                           DebitAmount = d.DebitAmount,
                                                           CreditAmount = d.CreditAmount,
                                                           BalanceAmount = d.BalanceAmount
                                                       };

                                        if (advances.Any())
                                        {
                                            var payTypes = from d in db.MstPayTypes
                                                           where d.IsLocked == true
                                                           select d;

                                            if (payTypes.Any())
                                            {
                                                var depositoryBanks = from d in db.MstArticles
                                                                      where d.ArticleTypeId == 5
                                                                      && d.IsLocked == true
                                                                      select d;

                                                if (depositoryBanks.Any())
                                                {
                                                    Data.TrnCollectionLine newCollectionLine = new Data.TrnCollectionLine
                                                    {
                                                        ORId = Convert.ToInt32(ORId),
                                                        BranchId = advances.FirstOrDefault().BranchId,
                                                        AccountId = advances.FirstOrDefault().AccountId,
                                                        ArticleId = advances.FirstOrDefault().ArticleId,
                                                        SIId = null,
                                                        Particulars = "Customer Advances",
                                                        Amount = advances.FirstOrDefault().BalanceAmount * -1,
                                                        PayTypeId = payTypes.FirstOrDefault().Id,
                                                        CheckNumber = "NA",
                                                        CheckDate = DateTime.Now,
                                                        CheckBank = "NA",
                                                        DepositoryBankId = depositoryBanks.FirstOrDefault().Id,
                                                        IsClear = false,
                                                    };

                                                    db.TrnCollectionLines.InsertOnSubmit(newCollectionLine);
                                                    db.SubmitChanges();

                                                    return Request.CreateResponse(HttpStatusCode.OK);
                                                }
                                                else
                                                {
                                                    return Request.CreateResponse(HttpStatusCode.NotFound, "No depository bank found. Please setup more depository banks for all transactions.");
                                                }
                                            }
                                            else
                                            {
                                                return Request.CreateResponse(HttpStatusCode.NotFound, "No pay type found. Please setup more pay type for all transactions.");
                                            }
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.BadRequest, "No advances found.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No journal data found.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new collection line if the current collection detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current collection details are not found in the server. Please add new collection first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new collection line in this collection detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this collection detail page.");
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

        // ===================
        // Add Collection Line
        // ===================
        [Authorize, HttpPost, Route("api/collectionLine/add/{ORId}")]
        public HttpResponseMessage AddCollectionLine(Entities.TrnCollectionLine objCollectionLine, String ORId)
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
                                    && d.SysForm.FormName.Equals("CollectionDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var collection = from d in db.TrnCollections
                                             where d.Id == Convert.ToInt32(ORId)
                                             select d;

                            if (collection.Any())
                            {
                                if (!collection.FirstOrDefault().IsLocked)
                                {
                                    var accounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                                                   where d.Id == objCollectionLine.AccountId
                                                   && d.IsLocked == true
                                                   select d;

                                    if (accounts.Any())
                                    {
                                        var articles = from d in db.MstArticles
                                                       where d.Id == objCollectionLine.ArticleId
                                                       && d.IsLocked == true
                                                       select d;

                                        if (articles.Any())
                                        {
                                            var payTypes = from d in db.MstPayTypes
                                                           where d.IsLocked == true
                                                           select d;

                                            if (payTypes.Any())
                                            {
                                                var depositoryBanks = from d in db.MstArticles
                                                                      where d.ArticleTypeId == 5
                                                                      && d.IsLocked == true
                                                                      select d;

                                                if (depositoryBanks.Any())
                                                {
                                                    Data.TrnCollectionLine newCollectionLine = new Data.TrnCollectionLine
                                                    {
                                                        ORId = Convert.ToInt32(ORId),
                                                        BranchId = objCollectionLine.BranchId,
                                                        AccountId = objCollectionLine.AccountId,
                                                        ArticleId = objCollectionLine.ArticleId,
                                                        SIId = objCollectionLine.SIId,
                                                        Particulars = objCollectionLine.Particulars,
                                                        Amount = objCollectionLine.Amount,
                                                        PayTypeId = objCollectionLine.PayTypeId,
                                                        CheckNumber = objCollectionLine.CheckNumber,
                                                        CheckDate = Convert.ToDateTime(objCollectionLine.CheckDate),
                                                        CheckBank = objCollectionLine.CheckBank,
                                                        DepositoryBankId = objCollectionLine.DepositoryBankId,
                                                        IsClear = objCollectionLine.IsClear,
                                                    };

                                                    db.TrnCollectionLines.InsertOnSubmit(newCollectionLine);
                                                    db.SubmitChanges();

                                                    return Request.CreateResponse(HttpStatusCode.OK);
                                                }
                                                else
                                                {
                                                    return Request.CreateResponse(HttpStatusCode.NotFound, "No depository bank found. Please setup more depository banks for all transactions.");
                                                }
                                            }
                                            else
                                            {
                                                return Request.CreateResponse(HttpStatusCode.NotFound, "No pay type found. Please setup more pay type for all transactions.");
                                            }
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.BadRequest, "No Article.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No Account.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new collection line if the current collection detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current collection details are not found in the server. Please add new collection first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add new collection line in this collection detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this collection detail page.");
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

        // ======================
        // Update Collection Line
        // ======================
        [Authorize, HttpPut, Route("api/collectionLine/update/{id}/{ORId}")]
        public HttpResponseMessage UpdateCollectionLine(Entities.TrnCollectionLine objCollectionLine, String id, String ORId)
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
                                    && d.SysForm.FormName.Equals("CollectionDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var collection = from d in db.TrnCollections
                                             where d.Id == Convert.ToInt32(ORId)
                                             select d;

                            if (collection.Any())
                            {
                                if (!collection.FirstOrDefault().IsLocked)
                                {
                                    var collectionLine = from d in db.TrnCollectionLines
                                                         where d.Id == Convert.ToInt32(id)
                                                         select d;

                                    if (collectionLine.Any())
                                    {
                                        var accounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                                                       where d.Id == objCollectionLine.AccountId
                                                       && d.IsLocked == true
                                                       select d;

                                        if (accounts.Any())
                                        {
                                            var articles = from d in db.MstArticles
                                                           where d.Id == objCollectionLine.ArticleId
                                                           && d.IsLocked == true
                                                           select d;

                                            if (articles.Any())
                                            {
                                                var payTypes = from d in db.MstPayTypes
                                                               where d.IsLocked == true
                                                               select d;

                                                if (payTypes.Any())
                                                {
                                                    var depositoryBanks = from d in db.MstArticles
                                                                          where d.ArticleTypeId == 5
                                                                          && d.IsLocked == true
                                                                          select d;

                                                    if (depositoryBanks.Any())
                                                    {
                                                        var updateCollectionLine = collectionLine.FirstOrDefault();
                                                        updateCollectionLine.ORId = Convert.ToInt32(ORId);
                                                        updateCollectionLine.BranchId = objCollectionLine.BranchId;
                                                        updateCollectionLine.AccountId = objCollectionLine.AccountId;
                                                        updateCollectionLine.ArticleId = objCollectionLine.ArticleId;
                                                        updateCollectionLine.SIId = objCollectionLine.SIId;
                                                        updateCollectionLine.Particulars = objCollectionLine.Particulars;
                                                        updateCollectionLine.Amount = objCollectionLine.Amount;
                                                        updateCollectionLine.PayTypeId = objCollectionLine.PayTypeId;
                                                        updateCollectionLine.CheckNumber = objCollectionLine.CheckNumber;
                                                        updateCollectionLine.CheckDate = Convert.ToDateTime(objCollectionLine.CheckDate);
                                                        updateCollectionLine.CheckBank = objCollectionLine.CheckBank;
                                                        updateCollectionLine.DepositoryBankId = objCollectionLine.DepositoryBankId;
                                                        updateCollectionLine.IsClear = objCollectionLine.IsClear;

                                                        db.SubmitChanges();

                                                        return Request.CreateResponse(HttpStatusCode.OK);
                                                    }
                                                    else
                                                    {
                                                        return Request.CreateResponse(HttpStatusCode.NotFound, "No depository bank found. Please setup more depository banks for all transactions.");
                                                    }
                                                }
                                                else
                                                {
                                                    return Request.CreateResponse(HttpStatusCode.NotFound, "No pay type found. Please setup more pay type for all transactions.");
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
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This collection line detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new collection line if the current collection detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current collection details are not found in the server. Please add new collection first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update collection line in this collection detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this collection detail page.");
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

        // ======================
        // Delete Collection Line
        // ======================
        [Authorize, HttpDelete, Route("api/collectionLine/delete/{id}/{ORId}")]
        public HttpResponseMessage DeleteCollectionLine(String id, String ORId)
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
                                    && d.SysForm.FormName.Equals("CollectionDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var collection = from d in db.TrnCollections
                                             where d.Id == Convert.ToInt32(ORId)
                                             select d;

                            if (collection.Any())
                            {
                                if (!collection.FirstOrDefault().IsLocked)
                                {
                                    var collectionLine = from d in db.TrnCollectionLines
                                                         where d.Id == Convert.ToInt32(id)
                                                         select d;

                                    if (collectionLine.Any())
                                    {
                                        db.TrnCollectionLines.DeleteOnSubmit(collectionLine.First());
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "This collection line detail is no longer exist in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot apply purchase order items to collection line if the current collection detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current collection details are not found in the server. Please add new collection first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete collection line in this collection detail page.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access in this collection detail page.");
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
