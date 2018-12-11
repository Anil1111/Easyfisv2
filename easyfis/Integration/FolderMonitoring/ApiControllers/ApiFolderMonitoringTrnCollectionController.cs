using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.FolderMonitoring.ApiControllers
{
    public class ApiFolderMonitoringTrnCollectionController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========
        // Business
        // ========
        private Business.AccountsReceivable accountsReceivable = new Business.AccountsReceivable();
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

        // ================================
        // Add Folder Monitoring Collection
        // ================================
        [HttpPost, Route("api/folderMonitoring/collection/add")]
        public HttpResponseMessage AddFolderMonitoringCollection(List<Entities.FolderMonitoringTrnCollection> folderMonitoringTrnCollectionObjects)
        {
            try
            {
                if (folderMonitoringTrnCollectionObjects.Any())
                {
                    foreach (var folderMonitoringTrnCollectionObject in folderMonitoringTrnCollectionObjects)
                    {
                        Boolean isBranchExist = false,
                                isCustomerExist = false,
                                isUserExist = false,
                                isAccountExist = false,
                                isArticleExist = false,
                                isSalesInvoiceExist = false,
                                isPayTypeExist = false,
                                isDepositoryBankExist = false;

                        IQueryable<Data.TrnSalesInvoice> salesInvoice = null;

                        var branch = from d in db.MstBranches where d.BranchCode.Equals(folderMonitoringTrnCollectionObject.BranchCode) select d;
                        if (branch.Any())
                        {
                            isBranchExist = true;

                            if (!folderMonitoringTrnCollectionObject.SINumber.Equals("") || !folderMonitoringTrnCollectionObject.SINumber.Equals("NA"))
                            {
                                salesInvoice = from d in db.TrnSalesInvoices where d.BranchId == branch.FirstOrDefault().Id && d.SINumber.Equals(folderMonitoringTrnCollectionObject.SINumber) && d.IsLocked == true select d;
                                if (salesInvoice.Any()) { isSalesInvoiceExist = true; }
                            }
                        }

                        var customer = from d in db.MstArticles where d.ArticleTypeId == 2 && d.ManualArticleCode.Equals(folderMonitoringTrnCollectionObject.CustomerCode) && d.IsLocked == true select d;
                        if (customer.Any()) { isCustomerExist = true; }

                        var user = from d in db.MstUsers where d.UserName.Equals(folderMonitoringTrnCollectionObject.UserCode) select d;
                        if (user.Any()) { isUserExist = true; }

                        List<easyfis.Entities.MstArticle> listArticles = new List<easyfis.Entities.MstArticle>();

                        var account = from d in db.MstAccounts where d.AccountCode.Equals(folderMonitoringTrnCollectionObject.AccountCode) select d;
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

                        var article = from d in listArticles where d.ManualArticleCode.Equals(folderMonitoringTrnCollectionObject.ArticleCode) select d;
                        if (article.Any()) { isArticleExist = true; }

                        var payType = from d in db.MstPayTypes where d.PayType.Equals(folderMonitoringTrnCollectionObject.PayType) select d;
                        if (payType.Any()) { isPayTypeExist = true; }

                        var depositoryBank = from d in db.MstArticles where d.ArticleTypeId == 5 && d.ManualArticleCode.Equals(folderMonitoringTrnCollectionObject.DepositoryBankCode) select d;
                        if (depositoryBank.Any()) { isDepositoryBankExist = true; }

                        if (isBranchExist && isCustomerExist && isUserExist && isAccountExist && isArticleExist && isPayTypeExist && isDepositoryBankExist)
                        {
                            Int32 ORId = 0;

                            var currentCollection = from d in db.TrnCollections where d.ManualORNumber.Equals(folderMonitoringTrnCollectionObject.ManualORNumber) && d.IsLocked == true select d;
                            if (currentCollection.Any())
                            {
                                ORId = currentCollection.FirstOrDefault().Id;

                                var unlockCollection = currentCollection.FirstOrDefault();
                                unlockCollection.IsLocked = false;
                                unlockCollection.UpdatedById = user.FirstOrDefault().Id;
                                unlockCollection.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnCollectionObject.CreatedDateTime);
                                db.SubmitChanges();

                                journal.DeleteOfficialReceiptJournal(ORId);
                            }
                            else
                            {
                                var defaultORNumber = "0000000001";
                                var lastCollection = from d in db.TrnCollections.OrderByDescending(d => d.Id) where d.BranchId == branch.FirstOrDefault().Id select d;
                                if (lastCollection.Any())
                                {
                                    var ORNumber = Convert.ToInt32(lastCollection.FirstOrDefault().ORNumber) + 0000000001;
                                    defaultORNumber = FillLeadingZeroes(ORNumber, 10);
                                }

                                Data.TrnCollection newCollection = new Data.TrnCollection
                                {
                                    BranchId = branch.FirstOrDefault().Id,
                                    ORNumber = defaultORNumber,
                                    ORDate = Convert.ToDateTime(folderMonitoringTrnCollectionObject.ORDate),
                                    ManualORNumber = folderMonitoringTrnCollectionObject.ManualORNumber,
                                    CustomerId = customer.FirstOrDefault().Id,
                                    Particulars = folderMonitoringTrnCollectionObject.Remarks,
                                    PreparedById = user.FirstOrDefault().Id,
                                    CheckedById = user.FirstOrDefault().Id,
                                    ApprovedById = user.FirstOrDefault().Id,
                                    Status = null,
                                    IsCancelled = false,
                                    IsPrinted = false,
                                    IsLocked = false,
                                    CreatedById = user.FirstOrDefault().Id,
                                    CreatedDateTime = Convert.ToDateTime(folderMonitoringTrnCollectionObject.CreatedDateTime),
                                    UpdatedById = user.FirstOrDefault().Id,
                                    UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnCollectionObject.CreatedDateTime)
                                };

                                db.TrnCollections.InsertOnSubmit(newCollection);
                                db.SubmitChanges();

                                ORId = newCollection.Id;
                            }

                            Int32? SIId = null;
                            if (isSalesInvoiceExist) { SIId = salesInvoice.FirstOrDefault().Id; }

                            Data.TrnCollectionLine newCollectionLine = new Data.TrnCollectionLine
                            {
                                ORId = ORId,
                                BranchId = branch.FirstOrDefault().Id,
                                AccountId = account.FirstOrDefault().Id,
                                ArticleId = article.FirstOrDefault().Id,
                                SIId = SIId,
                                Particulars = folderMonitoringTrnCollectionObject.Particulars,
                                Amount = folderMonitoringTrnCollectionObject.Amount,
                                PayTypeId = payType.FirstOrDefault().Id,
                                CheckNumber = folderMonitoringTrnCollectionObject.CheckNumber,
                                CheckDate = Convert.ToDateTime(folderMonitoringTrnCollectionObject.CheckDate),
                                CheckBank = folderMonitoringTrnCollectionObject.CheckBank,
                                DepositoryBankId = depositoryBank.FirstOrDefault().Id,
                                IsClear = folderMonitoringTrnCollectionObject.IsClear
                            };

                            db.TrnCollectionLines.InsertOnSubmit(newCollectionLine);
                            db.SubmitChanges();

                            var collection = from d in db.TrnCollections where d.Id == ORId && d.IsLocked == true select d;
                            if (collection.Any())
                            {
                                var lockCollection = collection.FirstOrDefault();
                                lockCollection.IsLocked = true;
                                lockCollection.UpdatedById = user.FirstOrDefault().Id;
                                lockCollection.UpdatedDateTime = Convert.ToDateTime(folderMonitoringTrnCollectionObject.CreatedDateTime);
                                db.SubmitChanges();

                                var collectionLines = from d in collection.FirstOrDefault().TrnCollectionLines where d.SIId != null select d;
                                if (collectionLines.Any())
                                {
                                    foreach (var collectionLine in collectionLines) { accountsReceivable.UpdateAccountsReceivable(Convert.ToInt32(collectionLine.SIId)); }
                                }

                                journal.InsertOfficialReceiptJournal(ORId);
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
