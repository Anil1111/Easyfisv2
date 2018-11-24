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
    public class ApiTrnStockCountController : ApiController
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
        // List Stock Count
        // ================
        [Authorize, HttpGet, Route("api/stockCount/list/{startDate}/{endDate}")]
        public List<Entities.TrnStockCount> ListStockCount(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var stockCounts = from d in db.TrnStockCounts.OrderByDescending(d => d.Id)
                              where d.SCDate >= Convert.ToDateTime(startDate)
                              && d.SCDate <= Convert.ToDateTime(endDate)
                              && d.BranchId == branchId
                              select new Entities.TrnStockCount
                              {
                                  Id = d.Id,
                                  SCNumber = d.SCNumber,
                                  SCDate = d.SCDate.ToShortDateString(),
                                  Particulars = d.Particulars,
                                  IsLocked = d.IsLocked,
                                  CreatedBy = d.MstUser2.FullName,
                                  CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                  UpdatedBy = d.MstUser4.FullName,
                                  UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                              };

            return stockCounts.ToList();
        }

        // ==================
        // Detail Stock Count
        // ==================
        [Authorize, HttpGet, Route("api/stockCount/detail/{id}")]
        public Entities.TrnStockCount DetailStockCount(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var stockCount = from d in db.TrnStockCounts
                             where d.BranchId == branchId
                             && d.Id == Convert.ToInt32(id)
                             select new Entities.TrnStockCount
                             {
                                 Id = d.Id,
                                 BranchId = d.BranchId,
                                 SCNumber = d.SCNumber,
                                 SCDate = d.SCDate.ToShortDateString(),
                                 Particulars = d.Particulars,
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

            if (stockCount.Any())
            {
                return stockCount.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/stockCount/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListStockCountBranch()
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // ============================
        // Dropdown List - User (Field)
        // ============================
        [Authorize, HttpGet, Route("api/stockCount/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListStockCountUsers()
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
        [Authorize, HttpGet, Route("api/stockCount/dropdown/list/status")]
        public List<Entities.MstStatus> DropdownListStockCountStatus()
        {
            var statuses = from d in db.MstStatus.OrderBy(d => d.Status)
                           where d.IsLocked == true
                           && d.Category.Equals("SC")
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

        // ================
        // Post Stock Count
        // ================
        [Authorize, HttpPost, Route("api/stockCount/post/{id}")]
        public HttpResponseMessage PostStockCount(String id)
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
                    var currentDefaultIncomeAccountId = currentUser.FirstOrDefault().IncomeAccountId;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("StockCountList")
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

                            var stockCount = from d in db.TrnStockCounts
                                             where d.Id == Convert.ToInt32(id)
                                             select d;

                            if (stockCount.Any())
                            {
                                if (stockCount.FirstOrDefault().IsLocked)
                                {
                                    List<Int32> listArticleIds = new List<Int32>();

                                    var accountArticleTypes = from d in db.MstAccountArticleTypes
                                                              where d.AccountId == currentDefaultIncomeAccountId
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

                                    if (stockCount.FirstOrDefault().TrnStockCountItems.Any())
                                    {
                                        Data.TrnStockOut newStockOut = new Data.TrnStockOut
                                        {
                                            BranchId = currentBranchId,
                                            OTNumber = defaultOTNumber,
                                            OTDate = DateTime.Today,
                                            AccountId = currentDefaultIncomeAccountId,
                                            ArticleId = listArticleIds.FirstOrDefault(),
                                            Particulars = stockCount.FirstOrDefault().Particulars,
                                            ManualOTNumber = "SC-" + stockCount.FirstOrDefault().SCNumber,
                                            PreparedById = currentUserId,
                                            CheckedById = currentUserId,
                                            ApprovedById = currentUserId,
                                            Status = stockCount.FirstOrDefault().Status,
                                            IsPrinted = false,
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

                                        foreach (var stockCountItem in stockCount.FirstOrDefault().TrnStockCountItems)
                                        {
                                            var articleInventory = from d in db.MstArticleInventories
                                                                   where d.ArticleId == stockCountItem.ItemId
                                                                   && d.BranchId == currentBranchId
                                                                   select d;
                                            if (articleInventory.Any())
                                            {
                                                Data.TrnStockOutItem newStockOutItems = new Data.TrnStockOutItem
                                                {
                                                    OTId = newStockOut.Id,
                                                    ExpenseAccountId = articleInventory.FirstOrDefault().MstArticle.ExpenseAccountId,
                                                    ItemId = stockCountItem.ItemId,
                                                    ItemInventoryId = articleInventory.FirstOrDefault().Id,
                                                    Particulars = stockCountItem.Particulars,
                                                    UnitId = articleInventory.FirstOrDefault().MstArticle.UnitId,
                                                    Quantity = articleInventory.FirstOrDefault().Quantity - stockCountItem.Quantity,
                                                    Cost = articleInventory.FirstOrDefault().Cost,
                                                    Amount = (articleInventory.FirstOrDefault().Quantity - stockCountItem.Quantity) * articleInventory.FirstOrDefault().Cost,
                                                    BaseUnitId = articleInventory.FirstOrDefault().MstArticle.UnitId,
                                                    BaseQuantity = articleInventory.FirstOrDefault().Quantity - stockCountItem.Quantity,
                                                    BaseCost = articleInventory.FirstOrDefault().Cost
                                                };

                                                db.TrnStockOutItems.InsertOnSubmit(newStockOutItems);

                                                String newObject2 = at.GetObjectString(newStockOutItems);
                                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject2);

                                            }
                                        }

                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK, newStockOut.Id);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "stock count items empty.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Cannot post stock count if the current stock count is not locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. These stock count details are no longer exist in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add stock count.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock count page.");
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
        // Add Stock Count
        // ===============
        [Authorize, HttpPost, Route("api/stockCount/add")]
        public HttpResponseMessage AddStockCount()
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
                                    && d.SysForm.FormName.Equals("StockCountList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultSCNumber = "0000000001";
                            var lastStockCount = from d in db.TrnStockCounts.OrderByDescending(d => d.Id)
                                                 where d.BranchId == currentBranchId
                                                 select d;

                            if (lastStockCount.Any())
                            {
                                var SCNumber = Convert.ToInt32(lastStockCount.FirstOrDefault().SCNumber) + 0000000001;
                                defaultSCNumber = FillLeadingZeroes(SCNumber, 10);
                            }

                            var users = from d in db.MstUsers.OrderBy(d => d.FullName)
                                        where d.IsLocked == true
                                        select d;

                            if (users.Any())
                            {
                                Data.TrnStockCount newStockCount = new Data.TrnStockCount
                                {
                                    BranchId = currentBranchId,
                                    SCNumber = defaultSCNumber,
                                    SCDate = DateTime.Today,
                                    Particulars = "NA",
                                    PreparedById = currentUserId,
                                    CheckedById = currentUserId,
                                    ApprovedById = currentUserId,
                                    IsPrinted = false,
                                    IsLocked = false,
                                    CreatedById = currentUserId,
                                    CreatedDateTime = DateTime.Now,
                                    UpdatedById = currentUserId,
                                    UpdatedDateTime = DateTime.Now
                                };

                                db.TrnStockCounts.InsertOnSubmit(newStockCount);
                                db.SubmitChanges();

                                String newObject = at.GetObjectString(newStockCount);
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                                return Request.CreateResponse(HttpStatusCode.OK, newStockCount.Id);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No user found. Please setup more users for all transactions.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add stock count.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock count page.");
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
        // Save Stock Count
        // ================
        [Authorize, HttpPut, Route("api/stockCount/save/{id}")]
        public HttpResponseMessage SaveStockCount(Entities.TrnStockCount objStockCount, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var stockCount = from d in db.TrnStockCounts where d.Id == Convert.ToInt32(id) select d;
                    if (stockCount.Any())
                    {
                        if (!stockCount.FirstOrDefault().IsLocked)
                        {
                            String oldObject = at.GetObjectString(stockCount.FirstOrDefault());

                            var saveStockCount = stockCount.FirstOrDefault();
                            saveStockCount.SCDate = Convert.ToDateTime(objStockCount.SCDate);
                            saveStockCount.Particulars = objStockCount.Particulars;
                            saveStockCount.CheckedById = objStockCount.CheckedById;
                            saveStockCount.ApprovedById = objStockCount.ApprovedById;
                            saveStockCount.Status = objStockCount.Status;
                            saveStockCount.UpdatedById = currentUserId;
                            saveStockCount.UpdatedDateTime = DateTime.Now;

                            db.SubmitChanges();

                            String newObject = at.GetObjectString(stockCount.FirstOrDefault());
                            at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Saving Error. These stock count details are already locked.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These stock count details are not found in the server.");
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
        // Lock Stock Count
        // ================
        [Authorize, HttpPut, Route("api/stockCount/lock/{id}")]
        public HttpResponseMessage LockStockCount(Entities.TrnStockCount objStockCount, String id)
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
                                    && d.SysForm.FormName.Equals("StockCountDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var stockCount = from d in db.TrnStockCounts
                                             where d.Id == Convert.ToInt32(id)
                                             select d;

                            if (stockCount.Any())
                            {
                                if (!stockCount.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = at.GetObjectString(stockCount.FirstOrDefault());

                                    var lockStockCount = stockCount.FirstOrDefault();
                                    lockStockCount.SCDate = Convert.ToDateTime(objStockCount.SCDate);
                                    lockStockCount.Particulars = objStockCount.Particulars;
                                    lockStockCount.CheckedById = objStockCount.CheckedById;
                                    lockStockCount.ApprovedById = objStockCount.ApprovedById;
                                    lockStockCount.Status = objStockCount.Status;
                                    lockStockCount.IsLocked = true;
                                    lockStockCount.UpdatedById = currentUserId;
                                    lockStockCount.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    String newObject = at.GetObjectString(stockCount.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These stock count details are already locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These stock count details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock stock count.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock count page.");
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

        // ==================
        // Unlock Stock Count
        // ==================
        [Authorize, HttpPut, Route("api/stockCount/unlock/{id}")]
        public HttpResponseMessage UnlockStockCount(String id)
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
                                    && d.SysForm.FormName.Equals("StockCountDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var stockCount = from d in db.TrnStockCounts
                                             where d.Id == Convert.ToInt32(id)
                                             select d;

                            if (stockCount.Any())
                            {
                                if (stockCount.FirstOrDefault().IsLocked)
                                {
                                    String oldObject = at.GetObjectString(stockCount.FirstOrDefault());

                                    var unlockStockCount = stockCount.FirstOrDefault();
                                    unlockStockCount.IsLocked = false;
                                    unlockStockCount.UpdatedById = currentUserId;
                                    unlockStockCount.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    String newObject = at.GetObjectString(stockCount.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These stock count details are already unlocked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These stock count details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock stock count.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock count page.");
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

        // ==================
        // Delete Stock Count
        // ==================
        [Authorize, HttpDelete, Route("api/stockCount/delete/{id}")]
        public HttpResponseMessage DeleteStockCount(String id)
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
                                    && d.SysForm.FormName.Equals("StockCountList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var stockCount = from d in db.TrnStockCounts
                                             where d.Id == Convert.ToInt32(id)
                                             select d;

                            if (stockCount.Any())
                            {
                                if (!stockCount.FirstOrDefault().IsLocked)
                                {
                                    db.TrnStockCounts.DeleteOnSubmit(stockCount.First());

                                    String oldObject = at.GetObjectString(stockCount.FirstOrDefault());
                                    at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete stock count if the current stock count record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These stock count details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete stock count.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock count page.");
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
