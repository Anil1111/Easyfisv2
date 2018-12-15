using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.FolderMonitoring.ApiControllers
{
    public class ApiFolderMonitoringTrnStockInController : ApiController
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

        // ==============================
        // Add Folder Monitoring Stock In
        // ==============================
        [HttpPost, Route("api/folderMonitoring/stockIn/add")]
        public HttpResponseMessage AddFolderMonitoringStockIn(List<Entities.FolderMonitoringTrnStockIn> folderMonitoringTrnStockInObjects)
        {
            try
            {
                if (folderMonitoringTrnStockInObjects.Any())
                {
                    foreach (var folderMonitoringTrnStockInObject in folderMonitoringTrnStockInObjects)
                    {
                        Boolean isBranchExist = false,
                                isAccountExist = false,
                                isArticleExist = false,
                                isUserExist = false,
                                isItemExist = false;

                        var branch = from d in db.MstBranches where d.BranchCode.Equals(folderMonitoringTrnStockInObject.BranchCode) select d;
                        if (branch.Any()) { isBranchExist = true; }

                        List<easyfis.Entities.MstArticle> listArticles = new List<easyfis.Entities.MstArticle>();

                        var account = from d in db.MstAccounts where d.AccountCode.Equals(folderMonitoringTrnStockInObject.AccountCode) select d;
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

                        var article = from d in listArticles where d.ManualArticleCode.Equals(folderMonitoringTrnStockInObject.ArticleCode) select d;
                        if (article.Any()) { isArticleExist = true; }

                        var user = from d in db.MstUsers where d.UserName.Equals(folderMonitoringTrnStockInObject.UserCode) select d;
                        if (user.Any()) { isUserExist = true; }

                        var item = from d in db.MstArticles where d.ArticleTypeId == 1 && d.ManualArticleCode.Equals(folderMonitoringTrnStockInObject.ItemCode) && d.IsLocked == true select d;
                        if (item.Any()) { isItemExist = true; }

                        if (isBranchExist && isUserExist && isAccountExist && isArticleExist && isItemExist)
                        {
                            Int32 INId = 0;

                            var currentStockIn = from d in db.TrnStockIns where d.BranchId == branch.FirstOrDefault().Id && d.ManualINNumber.Equals(folderMonitoringTrnStockInObject.ManualINNumber) && d.IsLocked == true select d;
                            if (currentStockIn.Any())
                            {
                                INId = currentStockIn.FirstOrDefault().Id;

                                var unlockStockIn = currentStockIn.FirstOrDefault();
                                unlockStockIn.IsLocked = false;
                                unlockStockIn.UpdatedById = user.FirstOrDefault().Id;
                                unlockStockIn.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnStockInObject.CreatedDateTime);
                                db.SubmitChanges();

                                journal.DeleteStockInJournal(INId);
                                inventory.DeleteStockInInventory(INId);
                            }
                            else
                            {
                                var defaultINNumber = "0000000001";
                                var lastStockIn = from d in db.TrnStockIns.OrderByDescending(d => d.Id) where d.BranchId == branch.FirstOrDefault().Id select d;
                                if (lastStockIn.Any())
                                {
                                    var INNumber = Convert.ToInt32(lastStockIn.FirstOrDefault().INNumber) + 0000000001;
                                    defaultINNumber = FillLeadingZeroes(INNumber, 10);
                                }

                                Data.TrnStockIn newStockIn = new Data.TrnStockIn
                                {
                                    BranchId = branch.FirstOrDefault().Id,
                                    INNumber = defaultINNumber,
                                    INDate = Convert.ToDateTime(folderMonitoringTrnStockInObject.INDate),
                                    AccountId = account.FirstOrDefault().Id,
                                    ArticleId = article.FirstOrDefault().Id,
                                    Particulars = folderMonitoringTrnStockInObject.Remarks,
                                    ManualINNumber = folderMonitoringTrnStockInObject.ManualINNumber,
                                    IsProduced = folderMonitoringTrnStockInObject.IsProduce,
                                    PreparedById = user.FirstOrDefault().Id,
                                    CheckedById = user.FirstOrDefault().Id,
                                    ApprovedById = user.FirstOrDefault().Id,
                                    IsPrinted = false,
                                    IsLocked = true,
                                    CreatedById = user.FirstOrDefault().Id,
                                    CreatedDateTime = Convert.ToDateTime(folderMonitoringTrnStockInObject.CreatedDateTime),
                                    UpdatedById = user.FirstOrDefault().Id,
                                    UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnStockInObject.CreatedDateTime)
                                };

                                db.TrnStockIns.InsertOnSubmit(newStockIn);
                                db.SubmitChanges();

                                INId = newStockIn.Id;
                            }

                            var unitConversion = from d in item.FirstOrDefault().MstArticleUnits where d.UnitId == item.FirstOrDefault().UnitId select d;
                            if (unitConversion.Any())
                            {
                                Decimal baseQuantity = folderMonitoringTrnStockInObject.Quantity * 1;
                                if (unitConversion.FirstOrDefault().Multiplier > 0)
                                {
                                    baseQuantity = folderMonitoringTrnStockInObject.Quantity * (1 / unitConversion.FirstOrDefault().Multiplier);
                                }

                                Decimal baseCost = folderMonitoringTrnStockInObject.Amount;
                                if (baseQuantity > 0)
                                {
                                    baseCost = folderMonitoringTrnStockInObject.Amount / baseQuantity;
                                }

                                Data.TrnStockInItem newStockInItem = new Data.TrnStockInItem
                                {
                                    INId = INId,
                                    ItemId = item.FirstOrDefault().Id,
                                    Particulars = folderMonitoringTrnStockInObject.Particulars,
                                    UnitId = item.FirstOrDefault().UnitId,
                                    Quantity = folderMonitoringTrnStockInObject.Quantity,
                                    Cost = folderMonitoringTrnStockInObject.Cost,
                                    Amount = folderMonitoringTrnStockInObject.Amount,
                                    BaseUnitId = item.FirstOrDefault().UnitId,
                                    BaseQuantity = baseQuantity,
                                    BaseCost = baseCost
                                };

                                db.TrnStockInItems.InsertOnSubmit(newStockInItem);
                                db.SubmitChanges();

                                var stockIn = from d in db.TrnStockIns where d.Id == INId select d;
                                if (stockIn.Any())
                                {
                                    var lockStockIn = stockIn.FirstOrDefault();
                                    lockStockIn.IsLocked = true;
                                    lockStockIn.UpdatedById = user.FirstOrDefault().Id;
                                    lockStockIn.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnStockInObject.CreatedDateTime);
                                    db.SubmitChanges();

                                    journal.InsertStockInJournal(INId);
                                    inventory.InsertStockInInventory(INId);
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
