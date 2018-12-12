using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.FolderMonitoring.ApiControllers
{
    public class ApiFolderMonitoringTrnStockOutController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========
        // Business
        // ========
        private Business.Inventory inventory = new Business.Inventory();
        private Business.Journal journal = new Business.Journal();

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

        // ===============================
        // Add Folder Monitoring Stock Out
        // ===============================
        [HttpPost, Route("api/folderMonitoring/stockOut/add")]
        public HttpResponseMessage AddFolderMonitoringStockOut(List<Entities.FolderMonitoringTrnStockOut> folderMonitoringTrnStockOutObjects)
        {
            try
            {
                if (folderMonitoringTrnStockOutObjects.Any())
                {
                    foreach (var folderMonitoringTrnStockOutObject in folderMonitoringTrnStockOutObjects)
                    {
                        Boolean isBranchExist = false,
                                isAccountExist = false,
                                isArticleExist = false,
                                isUserExist = false,
                                isItemExist = false;

                        var branch = from d in db.MstBranches where d.BranchCode.Equals(folderMonitoringTrnStockOutObject.BranchCode) select d;
                        if (branch.Any()) { isBranchExist = true; }

                        List<easyfis.Entities.MstArticle> listArticles = new List<easyfis.Entities.MstArticle>();

                        var account = from d in db.MstAccounts where d.AccountCode.Equals(folderMonitoringTrnStockOutObject.AccountCode) select d;
                        if (account.Any())
                        {
                            isAccountExist = true;

                            var accountArticleTypes = from d in db.MstAccountArticleTypes where d.AccountId == Convert.ToInt32(account.FirstOrDefault().Id) select d;
                            if (accountArticleTypes.Any())
                            {
                                foreach (var accountArticleType in accountArticleTypes)
                                {
                                    var articles = from d in db.MstArticles where d.ArticleTypeId == accountArticleType.ArticleTypeId && d.IsLocked == true select d;
                                    if (articles.Any())
                                    {
                                        foreach (var articleObject in articles)
                                        {
                                            listArticles.Add(new easyfis.Entities.MstArticle()
                                            {
                                                Id = articleObject.Id,
                                                ManualArticleCode = articleObject.ManualArticleCode,
                                                Article = articleObject.Article
                                            });
                                        }
                                    }
                                }
                            }
                        }

                        var article = from d in listArticles where d.ManualArticleCode.Equals(folderMonitoringTrnStockOutObject.ArticleCode) select d;
                        if (article.Any()) { isArticleExist = true; }

                        var user = from d in db.MstUsers where d.UserName.Equals(folderMonitoringTrnStockOutObject.UserCode) select d;
                        if (user.Any()) { isUserExist = true; }

                        var item = from d in db.MstArticles where d.ArticleTypeId == 1 && d.ManualArticleCode.Equals(folderMonitoringTrnStockOutObject.ItemCode) && d.IsLocked == true select d;
                        if (item.Any()) { isItemExist = true; }

                        if (isBranchExist && isUserExist && isAccountExist && isArticleExist && isItemExist)
                        {
                            Int32 OTId = 0;

                            var currentStockOut = from d in db.TrnStockOuts where d.BranchId == branch.FirstOrDefault().Id && d.ManualOTNumber.Equals(folderMonitoringTrnStockOutObject.ManualOTNumber) && d.IsLocked == true select d;
                            if (currentStockOut.Any())
                            {
                                OTId = currentStockOut.FirstOrDefault().Id;

                                var unlockStockOut = currentStockOut.FirstOrDefault();
                                unlockStockOut.IsLocked = false;
                                unlockStockOut.UpdatedById = user.FirstOrDefault().Id;
                                unlockStockOut.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnStockOutObject.CreatedDateTime);
                                db.SubmitChanges();

                                journal.DeleteStockOutJournal(OTId);
                                inventory.DeleteStockOutInventory(OTId);
                            }
                            else
                            {
                                var defaultOTNumber = "0000000001";
                                var lastStockOut = from d in db.TrnStockOuts.OrderByDescending(d => d.Id) where d.BranchId == branch.FirstOrDefault().Id select d;
                                if (lastStockOut.Any())
                                {
                                    var OTNumber = Convert.ToInt32(lastStockOut.FirstOrDefault().OTNumber) + 0000000001;
                                    defaultOTNumber = FillLeadingZeroes(OTNumber, 10);
                                }

                                Data.TrnStockOut newStockOut = new Data.TrnStockOut
                                {
                                    BranchId = branch.FirstOrDefault().Id,
                                    OTNumber = defaultOTNumber,
                                    OTDate = Convert.ToDateTime(folderMonitoringTrnStockOutObject.OTDate),
                                    AccountId = account.FirstOrDefault().Id,
                                    ArticleId = article.FirstOrDefault().Id,
                                    Particulars = folderMonitoringTrnStockOutObject.Remarks,
                                    ManualOTNumber = folderMonitoringTrnStockOutObject.ManualOTNumber,
                                    PreparedById = user.FirstOrDefault().Id,
                                    CheckedById = user.FirstOrDefault().Id,
                                    ApprovedById = user.FirstOrDefault().Id,
                                    IsPrinted = false,
                                    IsLocked = true,
                                    CreatedById = user.FirstOrDefault().Id,
                                    CreatedDateTime = Convert.ToDateTime(folderMonitoringTrnStockOutObject.CreatedDateTime),
                                    UpdatedById = user.FirstOrDefault().Id,
                                    UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnStockOutObject.CreatedDateTime)
                                };

                                db.TrnStockOuts.InsertOnSubmit(newStockOut);
                                db.SubmitChanges();

                                OTId = newStockOut.Id;
                            }

                            var unitConversion = from d in item.FirstOrDefault().MstArticleUnits where d.UnitId == item.FirstOrDefault().UnitId select d;
                            if (unitConversion.Any())
                            {
                                Decimal baseQuantity = folderMonitoringTrnStockOutObject.Quantity * 1;
                                if (unitConversion.FirstOrDefault().Multiplier > 0)
                                {
                                    baseQuantity = folderMonitoringTrnStockOutObject.Quantity * (1 / unitConversion.FirstOrDefault().Multiplier);
                                }

                                Decimal baseCost = folderMonitoringTrnStockOutObject.Amount;
                                if (baseQuantity > 0)
                                {
                                    baseCost = folderMonitoringTrnStockOutObject.Amount / baseQuantity;
                                }

                                Int32 itemInventoryId = 0;

                                var itemInventory = from d in db.MstArticleInventories where d.BranchId == branch.FirstOrDefault().Id && d.ArticleId == item.FirstOrDefault().Id select d;
                                if (itemInventory.Any()) { itemInventoryId = itemInventory.FirstOrDefault().Id; }

                                if (itemInventoryId > 0)
                                {
                                    Data.TrnStockOutItem newStockOutItem = new Data.TrnStockOutItem
                                    {
                                        OTId = OTId,
                                        ItemId = item.FirstOrDefault().Id,
                                        ItemInventoryId = itemInventoryId,
                                        Particulars = folderMonitoringTrnStockOutObject.Particulars,
                                        UnitId = item.FirstOrDefault().UnitId,
                                        Quantity = folderMonitoringTrnStockOutObject.Quantity,
                                        Cost = folderMonitoringTrnStockOutObject.Cost,
                                        Amount = folderMonitoringTrnStockOutObject.Amount,
                                        BaseUnitId = item.FirstOrDefault().UnitId,
                                        BaseQuantity = baseQuantity,
                                        BaseCost = baseCost
                                    };

                                    db.TrnStockOutItems.InsertOnSubmit(newStockOutItem);
                                    db.SubmitChanges();
                                }

                                var stockOut = from d in db.TrnStockOuts where d.Id == OTId select d;
                                if (stockOut.Any())
                                {
                                    var lockStockOut = stockOut.FirstOrDefault();
                                    lockStockOut.IsLocked = true;
                                    lockStockOut.UpdatedById = user.FirstOrDefault().Id;
                                    lockStockOut.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnStockOutObject.CreatedDateTime);
                                    db.SubmitChanges();

                                    journal.InsertStockOutJournal(OTId);
                                    inventory.InsertStockOutInventory(OTId);
                                }
                            }
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No data found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }
    }
}
