using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;

namespace easyfis.ApiControllers
{
    public class ApiSysUtilitiesController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =================================
        // Utilities Delete All Transactions
        // =================================
        [Authorize, HttpDelete, Route("api/utilities/delete/allTransactions")]
        public HttpResponseMessage UtilitiesDeleteAllTransactions()
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserName = currentUser.FirstOrDefault().UserName;
                    if (currentUserName.Equals("admin"))
                    {
                        var inventories = from d in db.TrnInventories select d;
                        if (inventories.Any())
                        {
                            db.TrnInventories.DeleteAllOnSubmit(inventories);
                        }

                        var stockTransferItems = from d in db.TrnStockTransferItems
                                                 select d;

                        if (stockTransferItems.Any())
                        {
                            db.TrnStockTransferItems.DeleteAllOnSubmit(stockTransferItems);
                        }

                        var stockTransfers = from d in db.TrnStockTransfers
                                             select d;

                        if (stockTransfers.Any())
                        {
                            db.TrnStockTransfers.DeleteAllOnSubmit(stockTransfers);
                        }

                        var stockOutItems = from d in db.TrnStockOutItems
                                            select d;

                        if (stockOutItems.Any())
                        {
                            db.TrnStockOutItems.DeleteAllOnSubmit(stockOutItems);
                        }

                        var stockOuts = from d in db.TrnStockOuts
                                        select d;

                        if (stockOuts.Any())
                        {
                            db.TrnStockOuts.DeleteAllOnSubmit(stockOuts);
                        }

                        var stockInItems = from d in db.TrnStockInItems
                                           select d;

                        if (stockInItems.Any())
                        {
                            db.TrnStockInItems.DeleteAllOnSubmit(stockInItems);
                        }

                        var stockIns = from d in db.TrnStockIns
                                       select d;

                        if (stockIns.Any())
                        {
                            db.TrnStockIns.DeleteAllOnSubmit(stockIns);
                        }

                        var stockCountItems = from d in db.TrnStockCountItems
                                              select d;

                        if (stockCountItems.Any())
                        {
                            db.TrnStockCountItems.DeleteAllOnSubmit(stockCountItems);
                        }

                        var stockCounts = from d in db.TrnStockCounts
                                          select d;

                        if (stockCounts.Any())
                        {
                            db.TrnStockCounts.DeleteAllOnSubmit(stockCounts);
                        }

                        var journals = from d in db.TrnJournals
                                       select d;

                        if (journals.Any())
                        {
                            db.TrnJournals.DeleteAllOnSubmit(journals);
                        }

                        var journalVoucherLines = from d in db.TrnJournalVoucherLines
                                                  select d;

                        if (journalVoucherLines.Any())
                        {
                            db.TrnJournalVoucherLines.DeleteAllOnSubmit(journalVoucherLines);
                        }

                        var journalVouchers = from d in db.TrnJournalVouchers
                                              select d;

                        if (journalVouchers.Any())
                        {
                            db.TrnJournalVouchers.DeleteAllOnSubmit(journalVouchers);
                        }

                        var collectionLines = from d in db.TrnCollectionLines
                                              select d;

                        if (collectionLines.Any())
                        {
                            db.TrnCollectionLines.DeleteAllOnSubmit(collectionLines);
                        }

                        var collections = from d in db.TrnCollections
                                          select d;

                        if (collections.Any())
                        {
                            db.TrnCollections.DeleteAllOnSubmit(collections);
                        }

                        var disbursementLines = from d in db.TrnDisbursementLines
                                                select d;

                        if (disbursementLines.Any())
                        {
                            db.TrnDisbursementLines.DeleteAllOnSubmit(disbursementLines);
                        }

                        var disbursements = from d in db.TrnDisbursements
                                            select d;

                        if (disbursements.Any())
                        {
                            db.TrnDisbursements.DeleteAllOnSubmit(disbursements);
                        }

                        var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                                select d;

                        if (disbursements.Any())
                        {
                            db.TrnSalesInvoiceItems.DeleteAllOnSubmit(salesInvoiceItems);
                        }

                        var salesInvoices = from d in db.TrnSalesInvoices
                                            select d;

                        if (salesInvoices.Any())
                        {
                            db.TrnSalesInvoices.DeleteAllOnSubmit(salesInvoices);
                        }

                        var receivingReceiptItems = from d in db.TrnReceivingReceiptItems
                                                    select d;

                        if (receivingReceiptItems.Any())
                        {
                            db.TrnReceivingReceiptItems.DeleteAllOnSubmit(receivingReceiptItems);
                        }

                        var receivingReceipts = from d in db.TrnReceivingReceipts
                                                select d;

                        if (receivingReceipts.Any())
                        {
                            db.TrnReceivingReceipts.DeleteAllOnSubmit(receivingReceipts);
                        }

                        var purchaseOrderItems = from d in db.TrnPurchaseOrderItems
                                                 select d;

                        if (purchaseOrderItems.Any())
                        {
                            db.TrnPurchaseOrderItems.DeleteAllOnSubmit(purchaseOrderItems);
                        }

                        var purchaseOrders = from d in db.TrnPurchaseOrders
                                             select d;

                        if (purchaseOrders.Any())
                        {
                            db.TrnPurchaseOrders.DeleteAllOnSubmit(purchaseOrders);
                        }

                        var articleInventories = from d in db.MstArticleInventories
                                                 select d;

                        if (articleInventories.Any())
                        {
                            db.MstArticleInventories.DeleteAllOnSubmit(articleInventories);
                        }

                        db.SubmitChanges();

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No rights.");
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
