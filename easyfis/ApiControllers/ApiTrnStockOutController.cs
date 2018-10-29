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
    public class ApiTrnStockOutController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

        // ==============
        // List Stock Out
        // ==============
        [Authorize, HttpGet, Route("api/stockOut/list/{startDate}/{endDate}")]
        public List<Entities.TrnStockOut> ListStockOut(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var stockOuts = from d in db.TrnStockOuts.OrderByDescending(d => d.Id)
                            where d.BranchId == branchId
                            && d.OTDate >= Convert.ToDateTime(startDate)
                            && d.OTDate <= Convert.ToDateTime(endDate)
                            select new Entities.TrnStockOut
                            {
                                Id = d.Id,
                                OTNumber = d.OTNumber,
                                OTDate = d.OTDate.ToShortDateString(),
                                ManualOTNumber = d.ManualOTNumber,
                                Account = d.MstAccount.Account,
                                Article = d.MstArticle.Article,
                                Particulars = d.Particulars,
                                IsLocked = d.IsLocked,
                                CreatedBy = d.MstUser2.FullName,
                                CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                UpdatedBy = d.MstUser4.FullName,
                                UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                            };

            return stockOuts.ToList();
        }

        // ================
        // Detail Stock Out
        // ================
        [Authorize, HttpGet, Route("api/stockOut/detail/{id}")]
        public Entities.TrnStockOut DetailStockOut(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var stockOut = from d in db.TrnStockOuts
                           where d.BranchId == branchId
                           && d.Id == Convert.ToInt32(id)
                           select new Entities.TrnStockOut
                           {
                               Id = d.Id,
                               BranchId = d.BranchId,
                               OTNumber = d.OTNumber,
                               OTDate = d.OTDate.ToShortDateString(),
                               AccountId = d.AccountId,
                               ArticleId = d.ArticleId,
                               Particulars = d.Particulars,
                               ManualOTNumber = d.ManualOTNumber,
                               PreparedById = d.PreparedById,
                               CheckedById = d.CheckedById,
                               ApprovedById = d.ApprovedById,
                               Status = d.Status,
                               IsLocked = d.IsLocked,
                               CreatedBy = d.MstUser2.FullName,
                               CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                               UpdatedBy = d.MstUser4.FullName,
                               UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                           };

            if (stockOut.Any())
            {
                return stockOut.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/stockOut/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListStockOutBranch()
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
        [Authorize, HttpGet, Route("api/stockOut/dropdown/list/account")]
        public List<Entities.MstAccount> DropdownListStockOutAccount()
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
        [Authorize, HttpGet, Route("api/stockOut/dropdown/list/article/{accountId}")]
        public List<Entities.MstArticle> DropdownListStockOutArticle(String accountId)
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
        [Authorize, HttpGet, Route("api/stockOut/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListStockOutUsers()
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

        // ==============================
        // Dropdown List - Status (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/stockOut/dropdown/list/status")]
        public List<Entities.MstStatus> DropdownListStockOutStatus()
        {
            var statuses = from d in db.MstStatus.OrderBy(d => d.Status)
                           where d.IsLocked == true
                           && d.Category.Equals("OT")
                           select new Entities.MstStatus
                           {
                               Id = d.Id,
                               Status = d.Status
                           };

            return statuses.ToList();
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

        // =============
        // Add Stock Out
        // =============
        [Authorize, HttpPost, Route("api/stockOut/add")]
        public HttpResponseMessage AddStockOut()
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
                                    && d.SysForm.FormName.Equals("StockOutList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultOTNumber = "0000000001";
                            var lastStockOut = from d in db.TrnStockOuts.OrderByDescending(d => d.Id)
                                               where d.BranchId == currentBranchId
                                               select d;

                            if (lastStockOut.Any())
                            {
                                var OTNumber = Convert.ToInt32(lastStockOut.FirstOrDefault().OTNumber) + 0000000001;
                                defaultOTNumber = FillLeadingZeroes(OTNumber, 10);
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
                                            Data.TrnStockOut newStockOut = new Data.TrnStockOut
                                            {
                                                BranchId = currentBranchId,
                                                OTNumber = defaultOTNumber,
                                                OTDate = DateTime.Today,
                                                AccountId = accounts.FirstOrDefault().Id,
                                                ArticleId = listArticleIds.FirstOrDefault(),
                                                Particulars = "NA",
                                                ManualOTNumber = "NA",
                                                PreparedById = currentUserId,
                                                CheckedById = currentUserId,
                                                ApprovedById = currentUserId,
                                                IsLocked = false,
                                                CreatedById = currentUserId,
                                                CreatedDateTime = DateTime.Now,
                                                UpdatedById = currentUserId,
                                                UpdatedDateTime = DateTime.Now
                                            };

                                            db.TrnStockOuts.InsertOnSubmit(newStockOut);
                                            db.SubmitChanges();

                                            String newObject = at.GetObjectString(newStockOut);
                                            at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                            return Request.CreateResponse(HttpStatusCode.OK, newStockOut.Id);
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
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add stock out.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock out page.");
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

        // ==============
        // Lock Stock Out
        // ==============
        [Authorize, HttpPut, Route("api/stockOut/lock/{id}")]
        public HttpResponseMessage LockStockOut(Entities.TrnStockOut objStockOut, String id)
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
                                    && d.SysForm.FormName.Equals("StockOutDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var stockOut = from d in db.TrnStockOuts
                                           where d.Id == Convert.ToInt32(id)
                                           select d;

                            if (stockOut.Any())
                            {
                                if (!stockOut.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = at.GetObjectString(stockOut.FirstOrDefault());

                                    var lockStockOut = stockOut.FirstOrDefault();
                                    lockStockOut.OTDate = Convert.ToDateTime(objStockOut.OTDate);
                                    lockStockOut.AccountId = objStockOut.AccountId;
                                    lockStockOut.ArticleId = objStockOut.ArticleId;
                                    lockStockOut.Particulars = objStockOut.Particulars;
                                    lockStockOut.ManualOTNumber = objStockOut.ManualOTNumber;
                                    lockStockOut.CheckedById = objStockOut.CheckedById;
                                    lockStockOut.ApprovedById = objStockOut.ApprovedById;
                                    lockStockOut.Status = objStockOut.Status;
                                    lockStockOut.IsLocked = true;
                                    lockStockOut.UpdatedById = currentUserId;
                                    lockStockOut.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    // =====================
                                    // Journal and Inventory
                                    // =====================
                                    Business.Journal journal = new Business.Journal();
                                    Business.Inventory inventory = new Business.Inventory();

                                    if (lockStockOut.IsLocked)
                                    {
                                        journal.InsertStockOutJournal(Convert.ToInt32(id));
                                        inventory.InsertStockOutInventory(Convert.ToInt32(id));
                                    }

                                    String newObject = at.GetObjectString(stockOut.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These stock out details are already locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These stock out details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock stock out.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock out page.");
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

        // ================
        // Unlock Stock Out
        // ================
        [Authorize, HttpPut, Route("api/stockOut/unlock/{id}")]
        public HttpResponseMessage UnlockStockOut(String id)
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
                                    && d.SysForm.FormName.Equals("StockOutDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var stockOut = from d in db.TrnStockOuts
                                           where d.Id == Convert.ToInt32(id)
                                           select d;

                            if (stockOut.Any())
                            {
                                if (stockOut.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = at.GetObjectString(stockOut.FirstOrDefault());

                                    var unlockStockOut = stockOut.FirstOrDefault();
                                    unlockStockOut.IsLocked = false;
                                    unlockStockOut.UpdatedById = currentUserId;
                                    unlockStockOut.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    // =====================
                                    // Journal and Inventory
                                    // =====================
                                    Business.Journal journal = new Business.Journal();
                                    Business.Inventory inventory = new Business.Inventory();

                                    if (!unlockStockOut.IsLocked)
                                    {
                                        journal.DeleteStockOutJournal(Convert.ToInt32(id));
                                        inventory.DeleteStockOutInventory(Convert.ToInt32(id));
                                    }

                                    String newObject = at.GetObjectString(stockOut.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These stock out details are already unlocked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These stock out details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock stock out.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock out page.");
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

        // ================
        // Delete Stock Out
        // ================
        [Authorize, HttpDelete, Route("api/stockOut/delete/{id}")]
        public HttpResponseMessage DeleteStockOut(String id)
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
                                    && d.SysForm.FormName.Equals("StockOutList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var stockOut = from d in db.TrnStockOuts
                                           where d.Id == Convert.ToInt32(id)
                                           select d;

                            if (stockOut.Any())
                            {
                                if (!stockOut.FirstOrDefault().IsLocked)
                                {
                                    db.TrnStockOuts.DeleteOnSubmit(stockOut.First());

                                    String oldObject = at.GetObjectString(stockOut.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete stock out if the current stock out record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These stock out details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete stock out.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock out page.");
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
