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
    public class ApiTrnStockTransferController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===================
        // List Stock Transfer
        // ===================
        [Authorize, HttpGet, Route("api/stockTransfer/list/{startDate}/{endDate}")]
        public List<Entities.TrnStockTransfer> ListStockTransfer(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var stockTransfers = from d in db.TrnStockTransfers.OrderByDescending(d => d.Id)
                                 where d.BranchId == branchId
                                 && d.STDate >= Convert.ToDateTime(startDate)
                                 && d.STDate <= Convert.ToDateTime(endDate)
                                 select new Entities.TrnStockTransfer
                                 {
                                     Id = d.Id,
                                     STNumber = d.STNumber,
                                     STDate = d.STDate.ToShortDateString(),
                                     Branch = d.MstBranch.Branch,
                                     ToBranch = d.MstBranch1.Branch,
                                     Particulars = d.Particulars,
                                     IsLocked = d.IsLocked,
                                     CreatedBy = d.MstUser2.FullName,
                                     CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                     UpdatedBy = d.MstUser4.FullName,
                                     UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                 };

            return stockTransfers.ToList();
        }

        // =====================
        // Detail Stock Transfer
        // =====================
        [Authorize, HttpGet, Route("api/stockTransfer/detail/{id}")]
        public Entities.TrnStockTransfer DetailStockTransfer(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var stockTransfer = from d in db.TrnStockTransfers
                                where d.BranchId == branchId
                                && d.Id == Convert.ToInt32(id)
                                select new Entities.TrnStockTransfer
                                {
                                    Id = d.Id,
                                    BranchId = d.BranchId,
                                    STNumber = d.STNumber,
                                    STDate = d.STDate.ToShortDateString(),
                                    ToBranchId = d.ToBranchId,
                                    ArticleId = d.ArticleId,
                                    Particulars = d.Particulars,
                                    ManualSTNumber = d.ManualSTNumber,
                                    PreparedById = d.PreparedById,
                                    CheckedById = d.CheckedById,
                                    ApprovedById = d.ApprovedById,
                                    IsLocked = d.IsLocked,
                                    CreatedBy = d.MstUser2.FullName,
                                    CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                    UpdatedBy = d.MstUser4.FullName,
                                    UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                };

            if (stockTransfer.Any())
            {
                return stockTransfer.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/stockTransfer/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListStockTransferBranch()
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // =================================
        // Dropdown List - To Branch (Field)
        // =================================
        [Authorize, HttpGet, Route("api/stockTransfer/dropdown/list/toBranch/{fromBranchId}")]
        public List<Entities.MstBranch> DropdownListStockTransferToBranch(String fromBranchId)
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           where d.Id != Convert.ToInt32(fromBranchId)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // ===============================
        // Dropdown List - Article (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/stockTransfer/dropdown/list/article")]
        public List<Entities.MstArticle> DropdownListStockTransferArticle()
        {
            var articles = from d in db.MstArticles
                           where d.ArticleTypeId == 6
                           && d.IsLocked == true
                           select new Entities.MstArticle
                           {
                               Id = d.Id,
                               Article = d.Article
                           };

            return articles.ToList();
        }

        // ============================
        // Dropdown List - User (Field)
        // ============================
        [Authorize, HttpGet, Route("api/stockTransfer/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListStockTransferUsers()
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

        // ==================
        // Add Stock Transfer
        // ==================
        [Authorize, HttpPost, Route("api/stockTransfer/add")]
        public HttpResponseMessage AddStockTransfer()
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
                                    && d.SysForm.FormName.Equals("StockTransferList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultSTNumber = "0000000001";
                            var lastStockTransfer = from d in db.TrnStockTransfers.OrderByDescending(d => d.Id)
                                                    where d.BranchId == currentBranchId
                                                    select d;

                            if (lastStockTransfer.Any())
                            {
                                var STNumber = Convert.ToInt32(lastStockTransfer.FirstOrDefault().STNumber) + 0000000001;
                                defaultSTNumber = FillLeadingZeroes(STNumber, 10);
                            }

                            var toBranches = from d in db.MstBranches
                                             where d.Id != currentBranchId
                                             select d;

                            if (toBranches.Any())
                            {
                                var articles = from d in db.MstArticles
                                               where d.ArticleTypeId == 6
                                               && d.IsLocked == true
                                               select d;

                                if (articles.Any())
                                {
                                    var users = from d in db.MstUsers.OrderBy(d => d.FullName)
                                                where d.IsLocked == true
                                                select d;

                                    if (users.Any())
                                    {
                                        Data.TrnStockTransfer newStockTransfer = new Data.TrnStockTransfer
                                        {
                                            BranchId = currentBranchId,
                                            STNumber = defaultSTNumber,
                                            STDate = DateTime.Today,
                                            ToBranchId = toBranches.FirstOrDefault().Id,
                                            ArticleId = articles.FirstOrDefault().Id,
                                            Particulars = "NA",
                                            ManualSTNumber = "NA",
                                            PreparedById = currentUserId,
                                            CheckedById = currentUserId,
                                            ApprovedById = currentUserId,
                                            IsLocked = false,
                                            CreatedById = currentUserId,
                                            CreatedDateTime = DateTime.Now,
                                            UpdatedById = currentUserId,
                                            UpdatedDateTime = DateTime.Now
                                        };

                                        db.TrnStockTransfers.InsertOnSubmit(newStockTransfer);
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK, newStockTransfer.Id);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "No user found. Please setup more users for all transactions.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.NotFound, "No article found. Please setup more articles for all transactions.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No branch found. Please setup more branches for all transactions.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add stock transfer.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock transfer page.");
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
        // Lock Stock Transfer
        // ===================
        [Authorize, HttpPut, Route("api/stockTransfer/lock/{id}")]
        public HttpResponseMessage LockStockTransfer(Entities.TrnStockTransfer objStockTransfer, String id)
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
                                    && d.SysForm.FormName.Equals("StockTransferDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var stockTransfer = from d in db.TrnStockTransfers
                                                where d.Id == Convert.ToInt32(id)
                                                select d;

                            if (stockTransfer.Any())
                            {
                                if (!stockTransfer.FirstOrDefault().IsLocked)
                                {
                                    var lockStockTransfer = stockTransfer.FirstOrDefault();
                                    lockStockTransfer.STDate = Convert.ToDateTime(objStockTransfer.STDate);
                                    lockStockTransfer.ToBranchId = objStockTransfer.ToBranchId;
                                    lockStockTransfer.ArticleId = objStockTransfer.ArticleId;
                                    lockStockTransfer.Particulars = objStockTransfer.Particulars;
                                    lockStockTransfer.ManualSTNumber = objStockTransfer.ManualSTNumber;
                                    lockStockTransfer.CheckedById = objStockTransfer.CheckedById;
                                    lockStockTransfer.ApprovedById = objStockTransfer.ApprovedById;
                                    lockStockTransfer.IsLocked = true;
                                    lockStockTransfer.UpdatedById = currentUserId;
                                    lockStockTransfer.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    // =====================
                                    // Journal and Inventory
                                    // =====================
                                    Business.Journal journal = new Business.Journal();
                                    Business.Inventory inventory = new Business.Inventory();

                                    if (lockStockTransfer.IsLocked)
                                    {
                                        journal.insertSTJournal(Convert.ToInt32(id));
                                        inventory.InsertSTInventory(Convert.ToInt32(id));
                                    }

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These stock transfer details are already locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These stock transfer details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock stock transfer.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock transfer page.");
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

        // =====================
        // Unlock Stock Transfer
        // =====================
        [Authorize, HttpPut, Route("api/stockTransfer/unlock/{id}")]
        public HttpResponseMessage UnlockStockTransfer(String id)
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
                                    && d.SysForm.FormName.Equals("StockTransferDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var stockTransfer = from d in db.TrnStockTransfers
                                                where d.Id == Convert.ToInt32(id)
                                                select d;

                            if (stockTransfer.Any())
                            {
                                if (stockTransfer.FirstOrDefault().IsLocked)
                                {
                                    var unlockStockTransfer = stockTransfer.FirstOrDefault();
                                    unlockStockTransfer.IsLocked = false;
                                    unlockStockTransfer.UpdatedById = currentUserId;
                                    unlockStockTransfer.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    // =====================
                                    // Journal and Inventory
                                    // =====================
                                    Business.Journal journal = new Business.Journal();
                                    Business.Inventory inventory = new Business.Inventory();

                                    if (!unlockStockTransfer.IsLocked)
                                    {
                                        journal.deleteSTJournal(Convert.ToInt32(id));
                                        inventory.deleteSTInventory(Convert.ToInt32(id));
                                    }

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These stock transfer details are already unlocked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These stock transfer details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock stock transfer.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock transfer page.");
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

        // =====================
        // Delete Stock Transfer
        // =====================
        [Authorize, HttpDelete, Route("api/stockTransfer/delete/{id}")]
        public HttpResponseMessage DeleteStockTransfer(String id)
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
                                    && d.SysForm.FormName.Equals("StockTransferList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var stockTransfer = from d in db.TrnStockTransfers
                                                where d.Id == Convert.ToInt32(id)
                                                select d;

                            if (stockTransfer.Any())
                            {
                                if (!stockTransfer.FirstOrDefault().IsLocked)
                                {
                                    db.TrnStockTransfers.DeleteOnSubmit(stockTransfer.First());
                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete stock transfer if the current stock transfer record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These stock transfer details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete stock transfer.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this stock transfer page.");
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