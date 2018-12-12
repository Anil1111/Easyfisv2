using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.FolderMonitoring.ApiControllers
{
    public class ApiFolderMonitoringTrnStockTransferController : ApiController
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
        [HttpPost, Route("api/folderMonitoring/stockTransfer/add")]
        public HttpResponseMessage AddFolderMonitoringStockTransfer(List<Entities.FolderMonitoringTrnStockTransfer> folderMonitoringTrnStockTransferObjects)
        {
            try
            {
                if (folderMonitoringTrnStockTransferObjects.Any())
                {
                    foreach (var folderMonitoringTrnStockTransferObject in folderMonitoringTrnStockTransferObjects)
                    {
                        Boolean isBranchExist = false,
                                isToBranchExist = false,
                                isAccountExist = false,
                                isArticleExist = false,
                                isUserExist = false,
                                isItemExist = false;

                        var branch = from d in db.MstBranches where d.BranchCode.Equals(folderMonitoringTrnStockTransferObject.BranchCode) select d;
                        if (branch.Any()) { isBranchExist = true; }

                        var toBranch = from d in db.MstBranches where d.BranchCode.Equals(folderMonitoringTrnStockTransferObject.ToBranchCode) select d;
                        if (toBranch.Any()) { isToBranchExist = true; }

                        var article = from d in db.MstArticles where d.ArticleTypeId == 6 && d.ManualArticleCode.Equals(folderMonitoringTrnStockTransferObject.ArticleCode) select d;
                        if (article.Any()) { isArticleExist = true; }

                        var user = from d in db.MstUsers where d.UserName.Equals(folderMonitoringTrnStockTransferObject.UserCode) select d;
                        if (user.Any()) { isUserExist = true; }

                        var item = from d in db.MstArticles where d.ArticleTypeId == 1 && d.ManualArticleCode.Equals(folderMonitoringTrnStockTransferObject.ItemCode) && d.IsLocked == true select d;
                        if (item.Any()) { isItemExist = true; }

                        if (isBranchExist && isToBranchExist && isUserExist && isAccountExist && isArticleExist && isItemExist)
                        {
                            Int32 STId = 0;

                            var currentStockTransfer = from d in db.TrnStockTransfers where d.BranchId == branch.FirstOrDefault().Id && d.ManualSTNumber.Equals(folderMonitoringTrnStockTransferObject.ManualSTNumber) && d.IsLocked == true select d;
                            if (currentStockTransfer.Any())
                            {
                                STId = currentStockTransfer.FirstOrDefault().Id;

                                var unlockStockTransfer = currentStockTransfer.FirstOrDefault();
                                unlockStockTransfer.IsLocked = false;
                                unlockStockTransfer.UpdatedById = user.FirstOrDefault().Id;
                                unlockStockTransfer.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnStockTransferObject.CreatedDateTime);
                                db.SubmitChanges();

                                journal.DeleteStockTransferJournal(STId);
                                inventory.DeleteStockTransferInventory(STId);
                            }
                            else
                            {
                                var defaultSTNumber = "0000000001";
                                var lastStockTransfer = from d in db.TrnStockTransfers.OrderByDescending(d => d.Id) where d.BranchId == branch.FirstOrDefault().Id select d;
                                if (lastStockTransfer.Any())
                                {
                                    var STNumber = Convert.ToInt32(lastStockTransfer.FirstOrDefault().STNumber) + 0000000001;
                                    defaultSTNumber = FillLeadingZeroes(STNumber, 10);
                                }

                                Data.TrnStockTransfer newStockTransfer = new Data.TrnStockTransfer
                                {
                                    BranchId = branch.FirstOrDefault().Id,
                                    STNumber = defaultSTNumber,
                                    STDate = Convert.ToDateTime(folderMonitoringTrnStockTransferObject.STDate),
                                    ToBranchId = toBranch.FirstOrDefault().Id,
                                    ArticleId = article.FirstOrDefault().Id,
                                    Particulars = folderMonitoringTrnStockTransferObject.Remarks,
                                    ManualSTNumber = folderMonitoringTrnStockTransferObject.ManualSTNumber,
                                    PreparedById = user.FirstOrDefault().Id,
                                    CheckedById = user.FirstOrDefault().Id,
                                    ApprovedById = user.FirstOrDefault().Id,
                                    IsPrinted = false,
                                    IsLocked = true,
                                    CreatedById = user.FirstOrDefault().Id,
                                    CreatedDateTime = Convert.ToDateTime(folderMonitoringTrnStockTransferObject.CreatedDateTime),
                                    UpdatedById = user.FirstOrDefault().Id,
                                    UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnStockTransferObject.CreatedDateTime)
                                };

                                db.TrnStockTransfers.InsertOnSubmit(newStockTransfer);
                                db.SubmitChanges();

                                STId = newStockTransfer.Id;
                            }

                            var unitConversion = from d in item.FirstOrDefault().MstArticleUnits where d.UnitId == item.FirstOrDefault().UnitId select d;
                            if (unitConversion.Any())
                            {
                                Decimal baseQuantity = folderMonitoringTrnStockTransferObject.Quantity * 1;
                                if (unitConversion.FirstOrDefault().Multiplier > 0)
                                {
                                    baseQuantity = folderMonitoringTrnStockTransferObject.Quantity * (1 / unitConversion.FirstOrDefault().Multiplier);
                                }

                                Decimal baseCost = folderMonitoringTrnStockTransferObject.Amount;
                                if (baseQuantity > 0)
                                {
                                    baseCost = folderMonitoringTrnStockTransferObject.Amount / baseQuantity;
                                }

                                Int32 itemInventoryId = 0;

                                var itemInventory = from d in db.MstArticleInventories where d.BranchId == branch.FirstOrDefault().Id && d.ArticleId == item.FirstOrDefault().Id select d;
                                if (itemInventory.Any()) { itemInventoryId = itemInventory.FirstOrDefault().Id; }

                                if (itemInventoryId > 0)
                                {
                                    Data.TrnStockTransferItem newStockTransferItem = new Data.TrnStockTransferItem
                                    {
                                        STId = STId,
                                        ItemId = item.FirstOrDefault().Id,
                                        ItemInventoryId = itemInventoryId,
                                        Particulars = folderMonitoringTrnStockTransferObject.Particulars,
                                        UnitId = item.FirstOrDefault().UnitId,
                                        Quantity = folderMonitoringTrnStockTransferObject.Quantity,
                                        Cost = folderMonitoringTrnStockTransferObject.Cost,
                                        Amount = folderMonitoringTrnStockTransferObject.Amount,
                                        BaseUnitId = item.FirstOrDefault().UnitId,
                                        BaseQuantity = baseQuantity,
                                        BaseCost = baseCost
                                    };

                                    db.TrnStockTransferItems.InsertOnSubmit(newStockTransferItem);
                                    db.SubmitChanges();
                                }

                                var stockTransfer = from d in db.TrnStockTransfers where d.Id == STId select d;
                                if (stockTransfer.Any())
                                {
                                    var lockStockTransfer = stockTransfer.FirstOrDefault();
                                    lockStockTransfer.IsLocked = true;
                                    lockStockTransfer.UpdatedById = user.FirstOrDefault().Id;
                                    lockStockTransfer.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnStockTransferObject.CreatedDateTime);
                                    db.SubmitChanges();

                                    journal.InsertStockTransferJournal(STId);
                                    inventory.InsertStockTransferInventory(STId);
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
