using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;

namespace easyfis.Controllers
{
    public class ApiStockCountController : ApiController
    {
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // current branch Id
        public Int32 currentBranchId()
        {
            return (from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d.BranchId).SingleOrDefault();
        }

        public String zeroFill(Int32 number, Int32 length)
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


        //// list stock count
        //[Authorize]
        //[HttpGet]
        //[Route("api/stockCount/list")]
        //public List<Models.TrnStockCount> listStockCount()
        //{
        //    var stockCounts = from d in db.TrnStockCounts.OrderByDescending(d => d.Id)
        //                      where d.BranchId == currentBranchId()
        //                      select new Models.TrnStockCount
        //                      {
        //                          Id = d.Id,
        //                          BranchId = d.BranchId,
        //                          Branch = d.MstBranch.Branch,
        //                          SCNumber = d.SCNumber,
        //                          SCDate = d.SCDate.ToShortDateString(),
        //                          Particulars = d.Particulars,
        //                          PreparedBy = d.MstUser3.FullName,
        //                          PreparedById = d.PreparedById,
        //                          CheckedBy = d.MstUser1.FullName,
        //                          CheckedById = d.CheckedById,
        //                          ApprovedBy = d.MstUser.FullName,
        //                          ApprovedById = d.ApprovedById,
        //                          IsLocked = d.IsLocked,
        //                          CreatedById = d.CreatedById,
        //                          CreatedBy = d.MstUser2.FullName,
        //                          CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
        //                          UpdatedById = d.UpdatedById,
        //                          UpdatedBy = d.MstUser4.FullName,
        //                          UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
        //                      };

        //    return stockCounts.ToList();
        //}

        //// list stock count by SCDate
        //[Authorize]
        //[HttpGet]
        //[Route("api/stockCount/listBySCDateByBranchId/{SCStartDate}/{SCEndDate}")]
        //public List<Models.TrnStockCount> listStockCountBySCDate(String SCStartDate, String SCEndDate)
        //{
        //    var stockCounts = from d in db.TrnStockCounts.OrderByDescending(d => d.Id)
        //                      where d.SCDate >= Convert.ToDateTime(SCStartDate)
        //                      && d.SCDate <= Convert.ToDateTime(SCEndDate)
        //                      && d.BranchId == currentBranchId()
        //                      select new Models.TrnStockCount
        //                      {
        //                          Id = d.Id,
        //                          BranchId = d.BranchId,
        //                          Branch = d.MstBranch.Branch,
        //                          SCNumber = d.SCNumber,
        //                          SCDate = d.SCDate.ToShortDateString(),
        //                          Particulars = d.Particulars,
        //                          PreparedBy = d.MstUser3.FullName,
        //                          PreparedById = d.PreparedById,
        //                          CheckedBy = d.MstUser1.FullName,
        //                          CheckedById = d.CheckedById,
        //                          ApprovedBy = d.MstUser.FullName,
        //                          ApprovedById = d.ApprovedById,
        //                          IsLocked = d.IsLocked,
        //                          CreatedById = d.CreatedById,
        //                          CreatedBy = d.MstUser2.FullName,
        //                          CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
        //                          UpdatedById = d.UpdatedById,
        //                          UpdatedBy = d.MstUser4.FullName,
        //                          UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
        //                      };

        //    return stockCounts.ToList();
        //}

        //// get stock count by Id
        //[Authorize]
        //[HttpGet]
        //[Route("api/stockCount/getById/{Id}")]
        //public Models.TrnStockCount getStockCountById(String Id)
        //{
        //    var stockCounts = from d in db.TrnStockCounts
        //                      where d.Id == Convert.ToInt32(Id)
        //                      select new Models.TrnStockCount
        //                      {
        //                          Id = d.Id,
        //                          BranchId = d.BranchId,
        //                          Branch = d.MstBranch.Branch,
        //                          SCNumber = d.SCNumber,
        //                          SCDate = d.SCDate.ToShortDateString(),
        //                          Particulars = d.Particulars,
        //                          PreparedBy = d.MstUser3.FullName,
        //                          PreparedById = d.PreparedById,
        //                          CheckedBy = d.MstUser1.FullName,
        //                          CheckedById = d.CheckedById,
        //                          ApprovedBy = d.MstUser.FullName,
        //                          ApprovedById = d.ApprovedById,
        //                          IsLocked = d.IsLocked,
        //                          CreatedById = d.CreatedById,
        //                          CreatedBy = d.MstUser2.FullName,
        //                          CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
        //                          UpdatedById = d.UpdatedById,
        //                          UpdatedBy = d.MstUser4.FullName,
        //                          UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
        //                      };

        //    return (Models.TrnStockCount)stockCounts.FirstOrDefault();
        //}

        // get last Stock Count
        //[Authorize]
        //[HttpGet]
        //[Route("api/stockCount/getLast")]
        //public Models.TrnStockCount getStockCountLast()
        //{
        //    var stockCounts = from d in db.TrnStockCounts.OrderByDescending(d => d.Id)
        //                      select new Models.TrnStockCount
        //                      {
        //                          Id = d.Id,
        //                          BranchId = d.BranchId,
        //                          Branch = d.MstBranch.Branch,
        //                          SCNumber = d.SCNumber,
        //                          SCDate = d.SCDate.ToShortDateString(),
        //                          Particulars = d.Particulars,
        //                          PreparedBy = d.MstUser3.FullName,
        //                          PreparedById = d.PreparedById,
        //                          CheckedBy = d.MstUser1.FullName,
        //                          CheckedById = d.CheckedById,
        //                          ApprovedBy = d.MstUser.FullName,
        //                          ApprovedById = d.ApprovedById,
        //                          IsLocked = d.IsLocked,
        //                          CreatedById = d.CreatedById,
        //                          CreatedBy = d.MstUser2.FullName,
        //                          CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
        //                          UpdatedById = d.UpdatedById,
        //                          UpdatedBy = d.MstUser4.FullName,
        //                          UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
        //                      };

        //    return (Models.TrnStockCount)stockCounts.FirstOrDefault();
        //}

        //// add stock count
        //[Authorize]
        //[HttpPost]
        //[Route("api/stockCount/save")]
        //public Int32 insertStockCount(Models.TrnStockCount stockCount)
        //{
        //    try
        //    {
        //        var userId = (from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d.Id).SingleOrDefault();

        //        var lastSCNumber = from d in db.TrnStockCounts.OrderByDescending(d => d.Id) where d.BranchId == currentBranchId() select d;
        //        var SCNumberResult = "0000000001";

        //        if (lastSCNumber.Any())
        //        {
        //            var SCNumber = Convert.ToInt32(lastSCNumber.FirstOrDefault().SCNumber) + 0000000001;
        //            SCNumberResult = zeroFill(SCNumber, 10);
        //        }

        //        Data.TrnStockCount newStockCount = new Data.TrnStockCount();
        //        newStockCount.BranchId = currentBranchId();
        //        newStockCount.SCNumber = SCNumberResult;
        //        newStockCount.SCDate = DateTime.Today;
        //        newStockCount.Particulars = "NA";
        //        newStockCount.PreparedById = userId;
        //        newStockCount.CheckedById = userId;
        //        newStockCount.ApprovedById = userId;
        //        newStockCount.IsLocked = false;
        //        newStockCount.CreatedById = userId;
        //        newStockCount.CreatedDateTime = DateTime.Now;
        //        newStockCount.UpdatedById = userId;
        //        newStockCount.UpdatedDateTime = DateTime.Now;

        //        db.TrnStockCounts.InsertOnSubmit(newStockCount);
        //        db.SubmitChanges();

        //        return newStockCount.Id;
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}

        //// update stock count
        //[Authorize]
        //[HttpPut]
        //[Route("api/stockCount/lock/{id}")]
        //public HttpResponseMessage updateStockCount(String id, Models.TrnStockCount stockCount)
        //{
        //    try
        //    {
        //        var userId = (from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d.Id).SingleOrDefault();

        //        var stockCounts = from d in db.TrnStockCounts where d.Id == Convert.ToInt32(id) select d;
        //        if (stockCounts.Any())
        //        {
        //            var updateStockCount = stockCounts.FirstOrDefault();
        //            updateStockCount.BranchId = stockCount.BranchId;
        //            updateStockCount.SCNumber = stockCount.SCNumber;
        //            updateStockCount.SCDate = Convert.ToDateTime(stockCount.SCDate);
        //            updateStockCount.Particulars = stockCount.Particulars;
        //            updateStockCount.PreparedById = stockCount.PreparedById;
        //            updateStockCount.CheckedById = stockCount.CheckedById;
        //            updateStockCount.ApprovedById = stockCount.ApprovedById;
        //            updateStockCount.IsLocked = true;
        //            updateStockCount.UpdatedById = userId;
        //            updateStockCount.UpdatedDateTime = DateTime.Now;

        //            db.SubmitChanges();

        //            return Request.CreateResponse(HttpStatusCode.OK);
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound);
        //        }
        //    }
        //    catch
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest);
        //    }
        //}

        //// unlock stock count
        //[Authorize]
        //[HttpPut]
        //[Route("api/stockCount/unlock/{id}")]
        //public HttpResponseMessage unlockStockCount(String id)
        //{
        //    try
        //    {
        //        var userId = (from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d.Id).SingleOrDefault();

        //        var stockCounts = from d in db.TrnStockCounts where d.Id == Convert.ToInt32(id) select d;
        //        if (stockCounts.Any())
        //        {
        //            var updateStockCount = stockCounts.FirstOrDefault();

        //            updateStockCount.IsLocked = false;
        //            updateStockCount.UpdatedById = userId;
        //            updateStockCount.UpdatedDateTime = DateTime.Now;

        //            db.SubmitChanges();

        //            return Request.CreateResponse(HttpStatusCode.OK);
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound);
        //        }
        //    }
        //    catch
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest);
        //    }
        //}

        //// delete stock count
        //[Authorize]
        //[HttpDelete]
        //[Route("api/stockCount/delete/{id}")]
        //public HttpResponseMessage deleteStockCount(String id)
        //{
        //    try
        //    {
        //        var stockCounts = from d in db.TrnStockCounts where d.Id == Convert.ToInt32(id) select d;
        //        if (stockCounts.Any())
        //        {
        //            db.TrnStockCounts.DeleteOnSubmit(stockCounts.First());
        //            db.SubmitChanges();

        //            return Request.CreateResponse(HttpStatusCode.OK);
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound);
        //        }
        //    }
        //    catch
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest);
        //    }
        //}

        //// ================
        //// Post Stock Count
        //// ================
        //[Authorize]
        //[HttpPost]
        //[Route("api/postStockCount/{SCId}")]
        //public Int32 postStockCout(String SCId)
        //{
        //    try
        //    {
        //        var lastOTNumber = from d in db.TrnStockOuts.OrderByDescending(d => d.Id)
        //                           where d.BranchId == currentBranchId()
        //                           select d;

        //        var OTNumberResult = "0000000001";

        //        if (lastOTNumber.Any())
        //        {
        //            var OTNumber = Convert.ToInt32(lastOTNumber.FirstOrDefault().OTNumber) + 0000000001;
        //            OTNumberResult = zeroFill(OTNumber, 10);
        //        }

        //        var users = (from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d).FirstOrDefault();

        //        Data.TrnStockOut newStockOut = new Data.TrnStockOut();

        //        var stockCount = from d in db.TrnStockCounts
        //                         where d.Id == Convert.ToInt32(SCId)
        //                         && d.IsLocked == true
        //                         select d;

        //        if (stockCount.Any())
        //        {
        //            newStockOut.BranchId = currentBranchId();
        //            newStockOut.OTNumber = OTNumberResult;
        //            newStockOut.OTDate = DateTime.Today;
        //            newStockOut.AccountId = users.IncomeAccountId;
        //            newStockOut.ArticleId = (from d in db.MstArticles where d.ArticleTypeId == 6 select d.Id).FirstOrDefault();
        //            newStockOut.Particulars = stockCount.FirstOrDefault().Particulars;
        //            newStockOut.ManualOTNumber = "SC-" + stockCount.FirstOrDefault().SCNumber;
        //            newStockOut.PreparedById = users.Id;
        //            newStockOut.CheckedById = users.Id;
        //            newStockOut.ApprovedById = users.Id;
        //            newStockOut.IsLocked = false;
        //            newStockOut.CreatedById = users.Id;
        //            newStockOut.CreatedDateTime = DateTime.Now;
        //            newStockOut.UpdatedById = users.Id;
        //            newStockOut.UpdatedDateTime = DateTime.Now;

        //            db.TrnStockOuts.InsertOnSubmit(newStockOut);
        //            db.SubmitChanges();

        //            if (stockCount.FirstOrDefault().TrnStockCountItems.Any())
        //            {
        //                foreach (var stockCountItem in stockCount.FirstOrDefault().TrnStockCountItems)
        //                {
        //                    var articleInventory = from d in db.MstArticleInventories
        //                                           where d.ArticleId == stockCountItem.ItemId
        //                                           && d.BranchId == currentBranchId()
        //                                           select d;
        //                    if (articleInventory.Any())
        //                    {
        //                        Data.TrnStockOutItem newStockOutItems = new Data.TrnStockOutItem();

        //                        newStockOutItems.OTId = newStockOut.Id;
        //                        newStockOutItems.ExpenseAccountId = articleInventory.FirstOrDefault().MstArticle.ExpenseAccountId;
        //                        newStockOutItems.ItemId = stockCountItem.ItemId;
        //                        newStockOutItems.ItemInventoryId = articleInventory.FirstOrDefault().Id;
        //                        newStockOutItems.Particulars = stockCountItem.Particulars;
        //                        newStockOutItems.UnitId = articleInventory.FirstOrDefault().MstArticle.UnitId;
        //                        newStockOutItems.Quantity = articleInventory.FirstOrDefault().Quantity - stockCountItem.Quantity;
        //                        newStockOutItems.Cost = articleInventory.FirstOrDefault().Cost;
        //                        newStockOutItems.Amount = (articleInventory.FirstOrDefault().Quantity - stockCountItem.Quantity) * articleInventory.FirstOrDefault().Cost;
        //                        newStockOutItems.BaseUnitId = articleInventory.FirstOrDefault().MstArticle.UnitId;
        //                        newStockOutItems.BaseQuantity = articleInventory.FirstOrDefault().Quantity - stockCountItem.Quantity;
        //                        newStockOutItems.BaseCost = articleInventory.FirstOrDefault().Cost;

        //                        db.TrnStockOutItems.InsertOnSubmit(newStockOutItems);
        //                    }
        //                }
        //                db.SubmitChanges();
        //            }
        //        }

        //        return newStockOut.Id;
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}
    }
}
