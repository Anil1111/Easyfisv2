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
                                  };

            return collectionLines.ToList();
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/collectionLine/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListCollectionLineBranch()
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
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
        [Authorize, HttpGet, Route("api/disbursementLine/dropdown/list/salesInvoice/{customerId}")]
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
                                    Amount = d.Amount
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
        public List<Entities.MstPayType> DropdownListCollectionLineDepositoryBank()
        {
            var payTypes = from d in db.MstPayTypes.OrderBy(d => d.PayType)
                           select new Entities.MstPayType
                           {
                               Id = d.Id,
                               PayType = d.PayType
                           };

            return payTypes.ToList();
        }


    }
}
