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
    public class ApiTrnStockInController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =============
        // List Stock In
        // =============
        [Authorize, HttpGet, Route("api/stockIn/list/{startDate}/{endDate}")]
        public List<Entities.TrnStockIn> ListStockIn(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var stockIns = from d in db.TrnStockIns.OrderByDescending(d => d.Id)
                           where d.BranchId == branchId
                           && d.INDate >= Convert.ToDateTime(startDate)
                           && d.INDate <= Convert.ToDateTime(endDate)
                           select new Entities.TrnStockIn
                           {
                               Id = d.Id,
                               INNumber = d.INNumber,
                               INDate = d.INDate.ToShortDateString(),
                               ManualINNumber = d.ManualINNumber,
                               Account = d.MstAccount.Account,
                               Article = d.MstArticle.Article,
                               Particulars = d.Particulars,
                               IsProduced = d.IsProduced,
                               IsLocked = d.IsLocked,
                               CreatedBy = d.MstUser2.FullName,
                               CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                               UpdatedBy = d.MstUser4.FullName,
                               UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                           };

            return stockIns.ToList();
        }

        // ===============
        // Detail Stock In
        // ===============
        [Authorize, HttpGet, Route("api/stockIn/detail/{id}")]
        public Entities.TrnStockIn DetailStockIn(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var stockIn = from d in db.TrnStockIns
                          where d.BranchId == branchId
                          && d.Id == Convert.ToInt32(id)
                          select new Entities.TrnStockIn
                          {
                              Id = d.Id,
                              BranchId = d.BranchId,
                              INNumber = d.INNumber,
                              INDate = d.INDate.ToShortDateString(),
                              AccountId = d.AccountId,
                              ArticleId = d.ArticleId,
                              Particulars = d.Particulars,
                              ManualINNumber = d.ManualINNumber,
                              IsProduced = d.IsProduced,
                              PreparedById = d.PreparedById,
                              CheckedById = d.CheckedById,
                              ApprovedById = d.ApprovedById,
                              IsLocked = d.IsLocked,
                              CreatedBy = d.MstUser2.FullName,
                              CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                              UpdatedBy = d.MstUser4.FullName,
                              UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                          };

            if (stockIn.Any())
            {
                return stockIn.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/stockIn/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListStockInBranch()
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
        [Authorize, HttpGet, Route("api/stockIn/dropdown/list/account")]
        public List<Entities.MstAccount> DropdownListStockInAccount()
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
        [Authorize, HttpGet, Route("api/stockIn/dropdown/list/article/{accountId}")]
        public List<Entities.MstArticle> DropdownListStockInArticle(String accountId)
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

        // ============================
        // Dropdown List - User (Field)
        // ============================
        [Authorize, HttpGet, Route("api/stockIn/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListStockInUsers()
        {
            var users = from d in db.MstUsers.OrderBy(d => d.FullName)
                        where d.IsLocked == true
                        select new Entities.MstUser
                        {
                            Id = d.Id,
                            FullName = d.FullName
                        };

            return users.ToList();
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

        // ============
        // Add Stock In
        // ============
        [Authorize, HttpPost, Route("api/stockIn/add")]
        public HttpResponseMessage AddStockIn()
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
                                    && d.SysForm.FormName.Equals("StockInList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultINNumber = "0000000001";
                            var lastStockIn = from d in db.TrnStockIns.OrderByDescending(d => d.Id)
                                              where d.BranchId == currentBranchId
                                              select d;

                            if (lastStockIn.Any())
                            {
                                var INNumber = Convert.ToInt32(lastStockIn.FirstOrDefault().INNumber) + 0000000001;
                                defaultINNumber = FillLeadingZeroes(INNumber, 10);
                            }

                            var accounts = from d in db.MstAccounts
                                           where d.IsLocked == true
                                           select d;

                            if (accounts.Any())
                            {
                                List<Int32> listArticleIds = new List<Int32>();

                                var accountArticleTypes = from d in db.MstAccountArticleTypes
                                                          where d.AccountId == accounts.FirstOrDefault().Id
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
                                                listArticleIds.Add(article.Id);
                                            }
                                        }
                                    }
                                }

                                if (listArticleIds.Any())
                                {
                                    var articleLists = from d in listArticleIds.ToList()
                                                       select d;

                                    if (articleLists.Any())
                                    {
                                        var users = from d in db.MstUsers.OrderBy(d => d.FullName)
                                                    where d.IsLocked == true
                                                    select d;

                                        if (users.Any())
                                        {
                                            Data.TrnStockIn newStockIn = new Data.TrnStockIn
                                            {
                                                BranchId = currentBranchId,
                                                INNumber = defaultINNumber,
                                                INDate = DateTime.Today,
                                                AccountId = accounts.FirstOrDefault().Id,
                                                ArticleId = listArticleIds.FirstOrDefault(),
                                                Particulars = "NA",
                                                ManualINNumber = "NA",
                                                IsProduced = false,
                                                PreparedById = currentUserId,
                                                CheckedById = currentUserId,
                                                ApprovedById = currentUserId,
                                                IsLocked = false,
                                                CreatedById = currentUserId,
                                                CreatedDateTime = DateTime.Now,
                                                UpdatedById = currentUserId,
                                                UpdatedDateTime = DateTime.Now
                                            };

                                            db.TrnStockIns.InsertOnSubmit(newStockIn);
                                            db.SubmitChanges();

                                            return Request.CreateResponse(HttpStatusCode.OK, newStockIn.Id);
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.NotFound, "No user found. Please setup more users for all transactions.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "No pay type found. Please setup more pay types for all transactions.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.NotFound, "No article found. Please setup more articles for all transactions.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No supplier found. Please setup more suppliers for all transactions.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add stockIn.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock in page.");
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

        // =============
        // Lock Stock In
        // =============
        [Authorize, HttpPut, Route("api/stockIn/lock/{id}")]
        public HttpResponseMessage LockStockIn(Entities.TrnStockIn objStockIn, String id)
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
                                    && d.SysForm.FormName.Equals("StockInDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var stockIn = from d in db.TrnStockIns
                                          where d.Id == Convert.ToInt32(id)
                                          select d;

                            if (stockIn.Any())
                            {
                                if (!stockIn.FirstOrDefault().IsLocked)
                                {
                                    var lockStockIn = stockIn.FirstOrDefault();
                                    lockStockIn.INDate = Convert.ToDateTime(objStockIn.INDate);
                                    lockStockIn.AccountId = objStockIn.AccountId;
                                    lockStockIn.ArticleId = objStockIn.ArticleId;
                                    lockStockIn.Particulars = objStockIn.Particulars;
                                    lockStockIn.ManualINNumber = objStockIn.ManualINNumber;
                                    lockStockIn.IsProduced = objStockIn.IsProduced;
                                    lockStockIn.CheckedById = objStockIn.CheckedById;
                                    lockStockIn.ApprovedById = objStockIn.ApprovedById;
                                    lockStockIn.IsLocked = true;
                                    lockStockIn.UpdatedById = currentUserId;
                                    lockStockIn.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    // =====================
                                    // Journal and Inventory
                                    // =====================
                                    Business.Journal journal = new Business.Journal();
                                    Business.Inventory inventory = new Business.Inventory();

                                    if (lockStockIn.IsLocked)
                                    {
                                        journal.InsertStockInJournal(Convert.ToInt32(id));
                                        inventory.InsertStockInInventory(Convert.ToInt32(id));
                                    }

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These stock in details are already locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These stock in details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock stockIn.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock in page.");
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

        // ===============
        // Unlock Stock In
        // ===============
        [Authorize, HttpPut, Route("api/stockIn/unlock/{id}")]
        public HttpResponseMessage UnlockStockIn(String id)
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
                                    && d.SysForm.FormName.Equals("StockInDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var stockIn = from d in db.TrnStockIns
                                          where d.Id == Convert.ToInt32(id)
                                          select d;

                            if (stockIn.Any())
                            {
                                if (stockIn.FirstOrDefault().IsLocked)
                                {
                                    var unlockStockIn = stockIn.FirstOrDefault();
                                    unlockStockIn.IsLocked = false;
                                    unlockStockIn.UpdatedById = currentUserId;
                                    unlockStockIn.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    // =====================
                                    // Journal and Inventory
                                    // =====================
                                    Business.Journal journal = new Business.Journal();
                                    Business.Inventory inventory = new Business.Inventory();

                                    if (!unlockStockIn.IsLocked)
                                    {
                                        journal.DeleteStockInJournal(Convert.ToInt32(id));
                                        inventory.DeleteStockInInventory(Convert.ToInt32(id));
                                    }

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These stock in details are already unlocked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These stock in details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock stock in.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock in page.");
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

        // ===============
        // Delete Stock In
        // ===============
        [Authorize, HttpDelete, Route("api/stockIn/delete/{id}")]
        public HttpResponseMessage DeleteStockIn(String id)
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
                                    && d.SysForm.FormName.Equals("StockInList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var stockIn = from d in db.TrnStockIns
                                          where d.Id == Convert.ToInt32(id)
                                          select d;

                            if (stockIn.Any())
                            {
                                if (!stockIn.FirstOrDefault().IsLocked)
                                {
                                    db.TrnStockIns.DeleteOnSubmit(stockIn.First());
                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete stock in if the current stock in record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These stock in details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete stock in.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock in page.");
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
