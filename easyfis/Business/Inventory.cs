using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace easyfis.Business
{
    public class Inventory
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =========================
        // Update Ariticle Inventory
        // =========================
        public void UpdateArticleInventory(Int32 ArticleInventoryId, String InventoryType)
        {
            var inventories = from d in db.TrnInventories
                              where d.ArticleInventoryId == ArticleInventoryId
                              group d by new
                              {
                                  ArticleInventoryId = d.ArticleInventoryId
                              } into g
                              select new
                              {
                                  ArticleInventoryId = g.Key.ArticleInventoryId,
                                  Quantity = g.Sum(d => d.Quantity),
                                  Amount = g.Sum(d => d.Amount),
                                  PositiveQuantity = g.Sum(d => d.Quantity >= 0 ? d.Quantity : 0),
                                  PositiveAmount = g.Sum(d => d.Amount >= 0 ? d.Amount : 0)
                              };

            if (inventories.Any())
            {
                if (InventoryType.Equals("Moving Average"))
                {
                    // ==============
                    // Moving Average
                    // ==============
                    var updateArticleInventories = from d in db.MstArticleInventories
                                                   where d.Id == ArticleInventoryId
                                                   select d;

                    if (updateArticleInventories.Any())
                    {
                        Decimal cost = inventories.FirstOrDefault().PositiveAmount / inventories.FirstOrDefault().PositiveQuantity;
                        Decimal amount = (inventories.FirstOrDefault().PositiveAmount / inventories.FirstOrDefault().PositiveQuantity) * inventories.FirstOrDefault().Quantity;

                        if (inventories.FirstOrDefault().PositiveQuantity == 0)
                        {
                            cost = 0;
                            amount = 0;
                        }

                        var updateArticleInventory = updateArticleInventories.FirstOrDefault();

                        updateArticleInventory.Quantity = inventories.FirstOrDefault().Quantity;
                        updateArticleInventory.Cost = cost;
                        updateArticleInventory.Amount = amount;

                        db.SubmitChanges();
                    }
                }
                else
                {
                    // =======================
                    // Specific Identification
                    // =======================
                    var updateArticleInventories = from d in db.MstArticleInventories
                                                   where d.Id == ArticleInventoryId
                                                   select d;

                    if (updateArticleInventories.Any())
                    {
                        var updateArticleInventory = updateArticleInventories.FirstOrDefault();

                        updateArticleInventory.Quantity = inventories.FirstOrDefault().Quantity;
                        updateArticleInventory.Amount = inventories.FirstOrDefault().Quantity * updateArticleInventory.Cost;

                        db.SubmitChanges();
                    }
                }
            }
            else
            {
                var updateArticleInventories = from d in db.MstArticleInventories
                                               where d.Id == ArticleInventoryId
                                               select d;

                if (updateArticleInventories.Any())
                {
                    var updateArticleInventory = updateArticleInventories.FirstOrDefault();

                    updateArticleInventory.Quantity = 0;
                    updateArticleInventory.Cost = 0;
                    updateArticleInventory.Amount = 0;

                    db.SubmitChanges();
                }
            }
        }

        // ==================================
        // Insert Receiving Receipt Inventory
        // ==================================
        public void InsertReceivingReceiptInventory(Int32 RRId)
        {
            try
            {
                var receivingReceiptItems = from d in db.TrnReceivingReceiptItems
                                            where d.RRId == RRId
                                            && d.MstArticle.IsInventory == true
                                            select new
                                            {
                                                RRId = d.RRId,
                                                RRNumber = d.TrnReceivingReceipt.RRNumber,
                                                RRDate = d.TrnReceivingReceipt.RRDate,
                                                ItemId = d.ItemId,
                                                Particulars = d.Particulars,
                                                Quantity = d.Quantity,
                                                Cost = d.Cost,
                                                Amount = d.Amount,
                                                VATAmount = d.VATAmount,
                                                WTAXAmount = d.WTAXAmount,
                                                BranchId = d.BranchId,
                                                BranchCode = d.MstBranch.BranchCode,
                                                BaseQuantity = d.BaseQuantity,
                                                BaseCost = d.BaseCost,
                                                InventoryType = d.TrnReceivingReceipt.MstUser5.InventoryType
                                            };

                if (receivingReceiptItems.Any())
                {
                    foreach (var receivingReceiptItem in receivingReceiptItems)
                    {
                        if (receivingReceiptItem.InventoryType.Equals("Moving Average"))
                        {
                            var articleInventories = from d in db.MstArticleInventories
                                                     where d.BranchId == receivingReceiptItem.BranchId
                                                     && d.ArticleId == receivingReceiptItem.ItemId
                                                     select d;

                            if (receivingReceiptItem.BaseQuantity > 0)
                            {
                                if (articleInventories.Any())
                                {
                                    Data.TrnInventory newInventory = new Data.TrnInventory
                                    {
                                        BranchId = receivingReceiptItem.BranchId,
                                        InventoryDate = receivingReceiptItem.RRDate,
                                        ArticleId = receivingReceiptItem.ItemId,
                                        ArticleInventoryId = articleInventories.FirstOrDefault().Id,
                                        RRId = receivingReceiptItem.RRId,
                                        QuantityIn = receivingReceiptItem.BaseQuantity,
                                        QuantityOut = 0,
                                        Quantity = receivingReceiptItem.BaseQuantity,
                                        Amount = receivingReceiptItem.Amount - receivingReceiptItem.VATAmount,
                                        Particulars = receivingReceiptItem.Particulars
                                    };

                                    db.TrnInventories.InsertOnSubmit(newInventory);
                                    db.SubmitChanges();

                                    UpdateArticleInventory(articleInventories.FirstOrDefault().Id, "Moving Average");
                                }
                                else
                                {
                                    Data.MstArticleInventory newArticleInventory = new Data.MstArticleInventory
                                    {
                                        BranchId = receivingReceiptItem.BranchId,
                                        ArticleId = receivingReceiptItem.ItemId,
                                        InventoryCode = "RR-" + receivingReceiptItem.BranchCode + "-" + receivingReceiptItem.RRNumber,
                                        Quantity = receivingReceiptItem.Quantity,
                                        Cost = (receivingReceiptItem.Amount - receivingReceiptItem.VATAmount) / receivingReceiptItem.Quantity,
                                        Amount = receivingReceiptItem.Amount - receivingReceiptItem.VATAmount,
                                        Particulars = "MOVING AVERAGE"
                                    };

                                    db.MstArticleInventories.InsertOnSubmit(newArticleInventory);
                                    db.SubmitChanges();

                                    Data.TrnInventory newInventory = new Data.TrnInventory
                                    {
                                        BranchId = receivingReceiptItem.BranchId,
                                        InventoryDate = receivingReceiptItem.RRDate,
                                        ArticleId = receivingReceiptItem.ItemId,
                                        ArticleInventoryId = newArticleInventory.Id,
                                        RRId = RRId,
                                        QuantityIn = receivingReceiptItem.BaseQuantity,
                                        QuantityOut = 0,
                                        Quantity = receivingReceiptItem.BaseQuantity,
                                        Amount = receivingReceiptItem.Amount - receivingReceiptItem.VATAmount,
                                        Particulars = receivingReceiptItem.Particulars
                                    };

                                    db.TrnInventories.InsertOnSubmit(newInventory);
                                    db.SubmitChanges();
                                }
                            }
                        }
                        else
                        {
                            var articleInventories = from d in db.MstArticleInventories
                                                     where d.BranchId == receivingReceiptItem.BranchId
                                                     && d.ArticleId == receivingReceiptItem.ItemId
                                                     && d.InventoryCode.Equals("RR-" + receivingReceiptItem.BranchCode + "-" + receivingReceiptItem.RRNumber)
                                                     select new
                                                     {
                                                         Id = d.Id,
                                                         BranchId = d.BranchId,
                                                         ArticleId = d.ArticleId,
                                                         InventoryCode = d.InventoryCode,
                                                         Quantity = d.Quantity,
                                                         Cost = d.Cost,
                                                         Amount = d.Amount,
                                                         Particulars = d.Particulars
                                                     };

                            if (receivingReceiptItem.BaseQuantity > 0)
                            {
                                if (articleInventories.Any())
                                {
                                    Data.TrnInventory newInventory = new Data.TrnInventory
                                    {
                                        BranchId = receivingReceiptItem.BranchId,
                                        InventoryDate = Convert.ToDateTime(receivingReceiptItem.RRDate),
                                        ArticleId = receivingReceiptItem.ItemId,
                                        ArticleInventoryId = articleInventories.FirstOrDefault().Id,
                                        RRId = RRId,
                                        QuantityIn = receivingReceiptItem.BaseQuantity,
                                        QuantityOut = 0,
                                        Quantity = receivingReceiptItem.BaseQuantity,
                                        Amount = receivingReceiptItem.Amount - receivingReceiptItem.VATAmount,
                                        Particulars = receivingReceiptItem.Particulars
                                    };

                                    db.TrnInventories.InsertOnSubmit(newInventory);
                                    db.SubmitChanges();

                                    UpdateArticleInventory(articleInventories.FirstOrDefault().Id, "");
                                }
                                else
                                {
                                    Data.MstArticleInventory newArticleInventory = new Data.MstArticleInventory
                                    {
                                        BranchId = receivingReceiptItem.BranchId,
                                        ArticleId = receivingReceiptItem.ItemId,
                                        InventoryCode = "RR-" + receivingReceiptItem.BranchCode + "-" + receivingReceiptItem.RRNumber,
                                        Quantity = receivingReceiptItem.Quantity,
                                        Cost = (receivingReceiptItem.Amount - receivingReceiptItem.VATAmount) / receivingReceiptItem.Quantity,
                                        Amount = receivingReceiptItem.Amount - receivingReceiptItem.VATAmount,
                                        Particulars = "SPECIFIC IDENTIFICATION"
                                    };

                                    db.MstArticleInventories.InsertOnSubmit(newArticleInventory);
                                    db.SubmitChanges();

                                    Data.TrnInventory newInventory = new Data.TrnInventory
                                    {
                                        BranchId = receivingReceiptItem.BranchId,
                                        InventoryDate = Convert.ToDateTime(receivingReceiptItem.RRDate),
                                        ArticleId = receivingReceiptItem.ItemId,
                                        ArticleInventoryId = newArticleInventory.Id,
                                        RRId = RRId,
                                        QuantityIn = receivingReceiptItem.BaseQuantity,
                                        QuantityOut = 0,
                                        Quantity = receivingReceiptItem.BaseQuantity,
                                        Amount = receivingReceiptItem.Amount - receivingReceiptItem.VATAmount,
                                        Particulars = receivingReceiptItem.Particulars
                                    };

                                    db.TrnInventories.InsertOnSubmit(newInventory);
                                    db.SubmitChanges();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ==================================
        // Delete Receiving Receipt Inventory
        // ==================================
        public void DeleteReceivingReceiptInventory(Int32 RRId)
        {
            try
            {
                // ==================
                // Delete Inventories
                // ==================
                var inventories = from d in db.TrnInventories
                                  where d.RRId == RRId
                                  select d;

                if (inventories.Any())
                {
                    db.TrnInventories.DeleteAllOnSubmit(inventories);
                    db.SubmitChanges();
                }

                var receivingReceiptItems = from d in db.TrnReceivingReceiptItems
                                            where d.RRId == RRId
                                            && d.MstArticle.IsInventory == true
                                            select new
                                            {
                                                ItemId = d.ItemId,
                                                BranchId = d.BranchId
                                            };

                if (receivingReceiptItems.Any())
                {
                    foreach (var receivingReceiptItem in receivingReceiptItems)
                    {
                        // ==========================
                        // Update Article Inventories
                        // ==========================
                        var articleInventories = from d in db.MstArticleInventories
                                                 where d.BranchId == receivingReceiptItem.BranchId
                                                 && d.ArticleId == receivingReceiptItem.ItemId
                                                 select new
                                                 {
                                                     Id = d.Id
                                                 };

                        if (articleInventories.Any())
                        {
                            foreach (var articleInventory in articleInventories)
                            {
                                UpdateArticleInventory(articleInventory.Id, "");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // =======================
        // Sales Invoice Inventory
        // =======================
        public void InsertSalesInvoiceInventory(Int32 SIId)
        {
            try
            {
                var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                        where d.SIId == SIId
                                        && d.MstArticle.IsInventory == true
                                        select new
                                        {
                                            SIId = d.SIId,
                                            SINumber = d.TrnSalesInvoice.SINumber,
                                            SIDate = d.TrnSalesInvoice.SIDate,
                                            BranchId = d.TrnSalesInvoice.BranchId,
                                            ItemId = d.ItemId,
                                            ItemInventoryId = d.ItemInventoryId,
                                            Particulars = d.Particulars,
                                            Quantity = d.Quantity,
                                            BaseQuantity = d.BaseQuantity,
                                            Cost = d.MstArticleInventory.Cost
                                        };

                if (salesInvoiceItems.Any())
                {
                    foreach (var salesInvoiceItem in salesInvoiceItems)
                    {
                        if (salesInvoiceItem.ItemInventoryId != null)
                        {
                            if (salesInvoiceItem.BaseQuantity > 0)
                            {
                                Data.TrnInventory newInventory = new Data.TrnInventory
                                {
                                    BranchId = salesInvoiceItem.BranchId,
                                    InventoryDate = salesInvoiceItem.SIDate,
                                    ArticleId = salesInvoiceItem.ItemId,
                                    ArticleInventoryId = Convert.ToInt32(salesInvoiceItem.ItemInventoryId),
                                    SIId = SIId,
                                    QuantityIn = 0,
                                    QuantityOut = salesInvoiceItem.BaseQuantity,
                                    Quantity = salesInvoiceItem.BaseQuantity * -1,
                                    Amount = (salesInvoiceItem.Cost * salesInvoiceItem.BaseQuantity) * -1,
                                    Particulars = salesInvoiceItem.Particulars
                                };

                                db.TrnInventories.InsertOnSubmit(newInventory);
                                db.SubmitChanges();

                                UpdateArticleInventory(Convert.ToInt32(salesInvoiceItem.ItemInventoryId), "");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ==============================
        // Delete Sales Invoice Inventory
        // ==============================
        public void DeleteSalesInvoiceInventory(Int32 SIId)
        {
            try
            {
                // ==================
                // Delete Inventories
                // ==================
                var inventories = from d in db.TrnInventories
                                  where d.SIId == SIId
                                  select d;

                if (inventories.Any())
                {
                    db.TrnInventories.DeleteAllOnSubmit(inventories);
                    db.SubmitChanges();
                }

                var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                        where d.SIId == SIId
                                        select new
                                        {
                                            BranchId = d.TrnSalesInvoice.BranchId,
                                            ItemId = d.ItemId,
                                        };

                if (salesInvoiceItems.Any())
                {
                    foreach (var salesInvoiceItem in salesInvoiceItems)
                    {
                        // ==========================
                        // Update Article Inventories
                        // ==========================
                        var articleInventories = from d in db.MstArticleInventories
                                                 where d.BranchId == salesInvoiceItem.BranchId
                                                 && d.ArticleId == salesInvoiceItem.ItemId
                                                 select new
                                                 {
                                                     Id = d.Id
                                                 };

                        if (articleInventories.Any())
                        {
                            foreach (var articleInventory in articleInventories)
                            {
                                UpdateArticleInventory(articleInventory.Id, "");
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ========================
        // Stock Transfer Inventory
        // ========================
        public void InsertSTInventory(Int32 STId)
        {
            try
            {
                Int32 articleInventoryId;

                var stockTransfers = from d in db.TrnStockTransfers
                                     where d.Id == STId
                                     && d.IsLocked == true
                                     select d;

                if (stockTransfers.Any())
                {
                    var stockTransferItems = from d in db.TrnStockTransferItems
                                             where d.STId == stockTransfers.FirstOrDefault().Id
                                             select d;

                    foreach (var stockTransferItem in stockTransferItems)
                    {
                        // =========
                        // Stock out
                        // =========
                        Data.TrnInventory newStockTransferItemStockOutInventory = new Data.TrnInventory();
                        newStockTransferItemStockOutInventory.BranchId = stockTransfers.FirstOrDefault().BranchId;
                        newStockTransferItemStockOutInventory.InventoryDate = Convert.ToDateTime(stockTransfers.FirstOrDefault().STDate);
                        newStockTransferItemStockOutInventory.ArticleId = stockTransferItem.ItemId;
                        newStockTransferItemStockOutInventory.ArticleInventoryId = stockTransferItem.ItemInventoryId;
                        newStockTransferItemStockOutInventory.STId = STId;
                        newStockTransferItemStockOutInventory.QuantityIn = 0;
                        newStockTransferItemStockOutInventory.QuantityOut = stockTransferItem.BaseQuantity;
                        newStockTransferItemStockOutInventory.Quantity = stockTransferItem.BaseQuantity * -1;
                        newStockTransferItemStockOutInventory.Amount = stockTransferItem.Amount * -1;
                        newStockTransferItemStockOutInventory.Particulars = stockTransfers.FirstOrDefault().Particulars;
                        db.TrnInventories.InsertOnSubmit(newStockTransferItemStockOutInventory);
                        db.SubmitChanges();

                        UpdateArticleInventory(stockTransferItem.ItemInventoryId, stockTransfers.FirstOrDefault().MstUser4.InventoryType);

                        // ========
                        // Stock In
                        // ========
                        articleInventoryId = 0;
                        var articleInventories = from d in db.MstArticleInventories
                                                 where d.BranchId == stockTransfers.FirstOrDefault().ToBranchId &&
                                                       d.ArticleId == stockTransferItem.ItemId
                                                 select d;

                        // Search for article inventory id
                        if (articleInventories.Any())
                        {
                            if (stockTransfers.FirstOrDefault().MstUser4.InventoryType.Equals("Moving Average"))
                            {
                                articleInventoryId = articleInventories.FirstOrDefault().Id;
                            }
                            else
                            {
                                foreach (var articleInventory in articleInventories)
                                {
                                    if (articleInventory.InventoryCode.Equals("ST-" + stockTransfers.FirstOrDefault().MstBranch1.BranchCode + "-" + stockTransfers.FirstOrDefault().STNumber))
                                    {
                                        articleInventoryId = articleInventory.Id;
                                        break;
                                    }
                                }
                            }
                        }

                        // If no article inventory id
                        if (articleInventoryId == 0)
                        {
                            Data.MstArticleInventory newArticleInventory = new Data.MstArticleInventory();
                            newArticleInventory.BranchId = stockTransfers.FirstOrDefault().ToBranchId;
                            newArticleInventory.ArticleId = stockTransferItem.ItemId;
                            newArticleInventory.InventoryCode = "ST-" + stockTransfers.FirstOrDefault().MstBranch1.BranchCode + "-" + stockTransfers.FirstOrDefault().STNumber;
                            newArticleInventory.Quantity = stockTransferItem.Quantity;
                            newArticleInventory.Cost = stockTransferItem.BaseCost;
                            newArticleInventory.Amount = stockTransferItem.Amount;
                            newArticleInventory.Particulars = stockTransfers.FirstOrDefault().Particulars;
                            db.MstArticleInventories.InsertOnSubmit(newArticleInventory);
                            db.SubmitChanges();

                            articleInventoryId = newArticleInventory.Id;
                        }

                        // Stock In Proper
                        Data.TrnInventory newStockTransferItemStockInInventory = new Data.TrnInventory();
                        newStockTransferItemStockInInventory.BranchId = stockTransfers.FirstOrDefault().ToBranchId;
                        newStockTransferItemStockInInventory.InventoryDate = Convert.ToDateTime(stockTransfers.FirstOrDefault().STDate);
                        newStockTransferItemStockInInventory.ArticleId = stockTransferItem.ItemId;
                        newStockTransferItemStockInInventory.ArticleInventoryId = articleInventoryId;
                        newStockTransferItemStockInInventory.STId = STId;
                        newStockTransferItemStockInInventory.QuantityIn = stockTransferItem.BaseQuantity;
                        newStockTransferItemStockInInventory.QuantityOut = 0;
                        newStockTransferItemStockInInventory.Quantity = stockTransferItem.BaseQuantity;
                        newStockTransferItemStockInInventory.Amount = stockTransferItem.Amount;
                        newStockTransferItemStockInInventory.Particulars = stockTransfers.FirstOrDefault().Particulars;
                        db.TrnInventories.InsertOnSubmit(newStockTransferItemStockInInventory);
                        db.SubmitChanges();

                        // ==========================================
                        // Update article inventory quantity and cost
                        // ==========================================
                        UpdateArticleInventory(articleInventoryId, stockTransfers.FirstOrDefault().MstUser4.InventoryType);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // Delete Stock Transfer Inventory
        public void deleteSTInventory(Int32 STId)
        {
            try
            {
                Int32 articleInventoryId;

                var stockTransfers = from d in db.TrnStockTransfers
                                     where d.Id == STId
                                     select d;

                if (stockTransfers.Any())
                {
                    // Delete TrnInventory
                    var deleteInventory = from d in db.TrnInventories
                                          where d.STId == stockTransfers.FirstOrDefault().Id
                                          select d;

                    db.TrnInventories.DeleteAllOnSubmit(deleteInventory);
                    db.SubmitChanges();

                    var stockTransferItems = from d in db.TrnStockTransferItems
                                             where d.STId == stockTransfers.FirstOrDefault().Id
                                             select d;

                    // Update article inventory cost and quantity
                    foreach (var stockTransferItem in stockTransferItems)
                    {
                        UpdateArticleInventory(stockTransferItem.ItemInventoryId, stockTransfers.FirstOrDefault().MstUser4.InventoryType);

                        articleInventoryId = 0;

                        var articleInventories = from d in db.MstArticleInventories
                                                 where d.BranchId == stockTransfers.FirstOrDefault().ToBranchId
                                                 && d.ArticleId == stockTransferItem.ItemId
                                                 select d;

                        // Search for article inventory id
                        if (articleInventories.Any())
                        {
                            if (stockTransfers.FirstOrDefault().MstUser4.InventoryType.Equals("Moving Average"))
                            {
                                articleInventoryId = articleInventories.FirstOrDefault().Id;
                            }
                            else
                            {
                                foreach (var articleInventory in articleInventories)
                                {
                                    if (articleInventory.InventoryCode.Equals("ST-" + stockTransfers.FirstOrDefault().MstBranch1.BranchCode + "-" + stockTransfers.FirstOrDefault().STNumber))
                                    {
                                        articleInventoryId = articleInventory.Id;
                                        break;
                                    }
                                }
                            }
                        }

                        if (articleInventoryId > 0)
                        {
                            UpdateArticleInventory(articleInventoryId, stockTransfers.FirstOrDefault().MstUser4.InventoryType);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ===================
        // Stock Out Inventory
        // ===================
        public void InsertOTInventory(Int32 OTId)
        {
            try
            {
                var stockOut = from d in db.TrnStockOuts
                               where d.Id == OTId
                               && d.IsLocked == true
                               select d;

                if (stockOut.Any())
                {
                    var stockOutItems = from d in db.TrnStockOutItems
                                        where d.OTId == OTId
                                        select d;

                    if (stockOutItems.Any())
                    {
                        Debug.WriteLine(stockOutItems.Count());

                        var countItemSaved = 0;

                        foreach (var stockOutItem in stockOutItems)
                        {
                            Decimal quantityIn = 0;
                            Decimal quantityOut = stockOutItem.BaseQuantity;
                            Decimal quantity = quantityIn - quantityOut;
                            Decimal amount = stockOutItem.Amount * -1;

                            if (stockOutItem.BaseQuantity < 0)
                            {
                                quantityIn = stockOutItem.BaseQuantity * -1;
                                quantityOut = 0;
                                amount = stockOutItem.Amount;
                            }

                            Data.TrnInventory newInventory = new Data.TrnInventory();
                            newInventory.BranchId = stockOut.FirstOrDefault().BranchId;
                            newInventory.InventoryDate = Convert.ToDateTime(stockOut.FirstOrDefault().OTDate);
                            newInventory.ArticleId = stockOutItem.ItemId;
                            newInventory.ArticleInventoryId = stockOutItem.ItemInventoryId;
                            newInventory.OTId = OTId;
                            newInventory.QuantityIn = quantityIn;
                            newInventory.QuantityOut = quantityOut;
                            newInventory.Quantity = quantity;
                            newInventory.Amount = amount;
                            newInventory.Particulars = stockOut.FirstOrDefault().Particulars;
                            db.TrnInventories.InsertOnSubmit(newInventory);
                            db.SubmitChanges();

                            UpdateArticleInventory(stockOutItem.ItemInventoryId, stockOut.FirstOrDefault().MstUser4.InventoryType);
                            countItemSaved += 1;
                        }

                        Debug.WriteLine(countItemSaved);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // Delete Stock Out Inventory
        public void deleteOTInventory(Int32 OTId)
        {
            try
            {
                // stock headers
                var stockOutHeaders = from d in db.TrnStockOuts
                                      where d.Id == OTId
                                      select d;

                if (stockOutHeaders.Any())
                {
                    var deleteInventory = from d in db.TrnInventories
                                          where d.OTId == stockOutHeaders.FirstOrDefault().Id
                                          select d;

                    db.TrnInventories.DeleteAllOnSubmit(deleteInventory);
                    db.SubmitChanges();

                    // stock out items
                    var stockOutItems = from d in db.TrnStockOutItems
                                        where d.OTId == OTId
                                        select d;

                    if (stockOutItems.Any())
                    {
                        foreach (var stockOutItem in stockOutItems)
                        {
                            UpdateArticleInventory(stockOutItem.ItemInventoryId, stockOutHeaders.FirstOrDefault().MstUser4.InventoryType);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ==================
        // Stock in Inventory
        // ==================
        public void insertINInventory(Int32 INId, Boolean isProduce)
        {
            try
            {
                Int32 articleInventoryId;
                Int32 componentArticleInventoryId;

                // ===============
                // Stock In Header
                // ===============
                var stockIns = from d in db.TrnStockIns
                               where d.Id == INId
                               && d.IsLocked == true
                               select d;

                if (stockIns.Any())
                {
                    // ==============
                    // Stock In Items
                    // ==============
                    var stockInItems = from d in db.TrnStockInItems
                                       where d.INId == INId
                                       select d;

                    if (stockInItems.Any())
                    {
                        foreach (var stockInItem in stockInItems)
                        {
                            if (stockInItem.TrnStockIn.IsProduced)
                            {
                                if (stockInItem.MstArticle.MstArticleComponents.Any())
                                {
                                    articleInventoryId = 0;

                                    // ======================
                                    // Get Artticle Inventory
                                    // ======================
                                    var articleInventories = from d in db.MstArticleInventories
                                                             where d.BranchId == stockIns.FirstOrDefault().BranchId
                                                             && d.ArticleId == stockInItem.ItemId
                                                             select d;

                                    if (articleInventories.Any())
                                    {
                                        if (stockIns.FirstOrDefault().MstUser4.InventoryType.Equals("Moving Average"))
                                        {
                                            articleInventoryId = articleInventories.FirstOrDefault().Id;
                                        }
                                        else
                                        {
                                            foreach (var articleInventory in articleInventories)
                                            {
                                                if (articleInventory.InventoryCode.Equals("IN-" + stockIns.FirstOrDefault().MstBranch.BranchCode + "-" + stockIns.FirstOrDefault().INNumber))
                                                {
                                                    articleInventoryId = articleInventory.Id;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (articleInventoryId == 0)
                                    {
                                        // ========================
                                        // Insert Article Inventory
                                        // ========================
                                        Data.MstArticleInventory newArticleInventory = new Data.MstArticleInventory();
                                        newArticleInventory.BranchId = stockIns.FirstOrDefault().BranchId;
                                        newArticleInventory.ArticleId = stockInItem.ItemId;
                                        newArticleInventory.InventoryCode = "IN-" + stockIns.FirstOrDefault().MstBranch.BranchCode + "-" + stockIns.FirstOrDefault().INNumber;
                                        newArticleInventory.Quantity = stockInItem.Quantity;
                                        newArticleInventory.Cost = stockInItem.Amount / stockInItem.Quantity;
                                        newArticleInventory.Amount = stockInItem.Amount;
                                        newArticleInventory.Particulars = stockIns.FirstOrDefault().Particulars;
                                        db.MstArticleInventories.InsertOnSubmit(newArticleInventory);
                                        db.SubmitChanges();

                                        articleInventoryId = newArticleInventory.Id;
                                    }

                                    // ================
                                    // Insert Inventory
                                    // ================
                                    Data.TrnInventory newInventory = new Data.TrnInventory();
                                    newInventory.BranchId = stockIns.FirstOrDefault().BranchId;
                                    newInventory.InventoryDate = Convert.ToDateTime(stockIns.FirstOrDefault().INDate);
                                    newInventory.ArticleId = stockInItem.ItemId;
                                    newInventory.ArticleInventoryId = articleInventoryId;
                                    newInventory.INId = INId;
                                    newInventory.QuantityIn = stockInItem.BaseQuantity;
                                    newInventory.QuantityOut = 0;
                                    newInventory.Quantity = stockInItem.BaseQuantity;
                                    newInventory.Amount = stockInItem.Amount;
                                    newInventory.Particulars = stockIns.FirstOrDefault().Particulars;
                                    db.TrnInventories.InsertOnSubmit(newInventory);
                                    db.SubmitChanges();

                                    // ======================================================
                                    // Update article inventory quantity and cost (Component) 
                                    // ======================================================
                                    UpdateArticleInventory(articleInventoryId, stockIns.FirstOrDefault().MstUser4.InventoryType);

                                    // =========
                                    // Component
                                    // =========
                                    foreach (var component in stockInItem.MstArticle.MstArticleComponents)
                                    {
                                        componentArticleInventoryId = 0;

                                        // ======================
                                        // Get Artticle Inventory
                                        // ======================
                                        var componentArticleInventories = from d in db.MstArticleInventories
                                                                          where d.BranchId == stockIns.FirstOrDefault().BranchId
                                                                          && d.ArticleId == component.ComponentArticleId
                                                                          select d;

                                        if (componentArticleInventories.Any())
                                        {
                                            if (stockIns.FirstOrDefault().MstUser4.InventoryType.Equals("Moving Average"))
                                            {
                                                componentArticleInventoryId = componentArticleInventories.FirstOrDefault().Id;
                                            }
                                            else
                                            {
                                                foreach (var componentArticleInventory in componentArticleInventories)
                                                {
                                                    if (componentArticleInventory.InventoryCode.Equals("IN-" + stockIns.FirstOrDefault().MstBranch.BranchCode + "-" + stockIns.FirstOrDefault().INNumber))
                                                    {
                                                        componentArticleInventoryId = componentArticleInventory.Id;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        if (componentArticleInventoryId != 0)
                                        {
                                            // ============================
                                            // Insert Inventory (Component)
                                            // ============================
                                            Data.TrnInventory newComponentInventory = new Data.TrnInventory();
                                            newComponentInventory.BranchId = stockIns.FirstOrDefault().BranchId;
                                            newComponentInventory.InventoryDate = Convert.ToDateTime(stockIns.FirstOrDefault().INDate);
                                            newComponentInventory.ArticleId = component.ComponentArticleId;
                                            newComponentInventory.ArticleInventoryId = componentArticleInventoryId;
                                            newComponentInventory.INId = INId;
                                            newComponentInventory.QuantityIn = 0;
                                            newComponentInventory.QuantityOut = (component.Quantity * stockInItem.Quantity) * -1;
                                            newComponentInventory.Quantity = (component.Quantity * stockInItem.Quantity) * -1;
                                            newComponentInventory.Amount = (newComponentInventory.Quantity * component.MstArticle1.MstArticleInventories.OrderByDescending(c => c.Cost).FirstOrDefault().Cost);
                                            newComponentInventory.Particulars = stockIns.FirstOrDefault().Particulars;
                                            db.TrnInventories.InsertOnSubmit(newComponentInventory);
                                            db.SubmitChanges();

                                            // ======================================================
                                            // Update article inventory quantity and cost (Component) 
                                            // ======================================================
                                            UpdateArticleInventory(articleInventoryId, stockIns.FirstOrDefault().MstUser4.InventoryType);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // ======================
                                // Get Artticle Inventory
                                // ======================
                                var articleInventories = from d in db.MstArticleInventories
                                                         where d.BranchId == stockIns.FirstOrDefault().BranchId
                                                         && d.ArticleId == stockInItem.ItemId
                                                         select d;

                                articleInventoryId = 0;
                                if (articleInventories.Any())
                                {
                                    if (stockIns.FirstOrDefault().MstUser4.InventoryType.Equals("Moving Average"))
                                    {
                                        articleInventoryId = articleInventories.FirstOrDefault().Id;
                                    }
                                    else
                                    {
                                        foreach (var articleInventory in articleInventories)
                                        {
                                            if (articleInventory.InventoryCode.Equals("IN-" + stockIns.FirstOrDefault().MstBranch.BranchCode + "-" + stockIns.FirstOrDefault().INNumber))
                                            {
                                                articleInventoryId = articleInventory.Id;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (articleInventoryId == 0)
                                {
                                    // ========================
                                    // Insert Article Inventory
                                    // ========================
                                    Data.MstArticleInventory newArticleInventory = new Data.MstArticleInventory();
                                    newArticleInventory.BranchId = stockIns.FirstOrDefault().BranchId;
                                    newArticleInventory.ArticleId = stockInItem.ItemId;
                                    newArticleInventory.InventoryCode = "IN-" + stockIns.FirstOrDefault().MstBranch.BranchCode + "-" + stockIns.FirstOrDefault().INNumber;
                                    newArticleInventory.Quantity = stockInItem.Quantity;
                                    newArticleInventory.Cost = stockInItem.Amount / stockInItem.Quantity;
                                    newArticleInventory.Amount = stockInItem.Amount;
                                    newArticleInventory.Particulars = stockIns.FirstOrDefault().Particulars;
                                    db.MstArticleInventories.InsertOnSubmit(newArticleInventory);
                                    db.SubmitChanges();

                                    articleInventoryId = newArticleInventory.Id;
                                }

                                // ================
                                // Insert Inventory
                                // ================
                                Data.TrnInventory newInventory = new Data.TrnInventory();
                                newInventory.BranchId = stockIns.FirstOrDefault().BranchId;
                                newInventory.InventoryDate = Convert.ToDateTime(stockIns.FirstOrDefault().INDate);
                                newInventory.ArticleId = stockInItem.ItemId;
                                newInventory.ArticleInventoryId = articleInventoryId;
                                newInventory.INId = INId;
                                newInventory.QuantityIn = stockInItem.BaseQuantity;
                                newInventory.QuantityOut = 0;
                                newInventory.Quantity = stockInItem.BaseQuantity;
                                newInventory.Amount = stockInItem.Amount;
                                newInventory.Particulars = stockIns.FirstOrDefault().Particulars;
                                db.TrnInventories.InsertOnSubmit(newInventory);
                                db.SubmitChanges();

                                // ==========================================
                                // Update article inventory quantity and cost
                                // ==========================================
                                UpdateArticleInventory(articleInventoryId, stockIns.FirstOrDefault().MstUser4.InventoryType);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // Delete stock in Inventory
        public void deleteINInventory(Int32 INId)
        {
            try
            {
                Int32 articleInventoryId;

                // Stock in header
                var stockIns = from d in db.TrnStockIns
                               where d.Id == INId
                               //&& d.IsLocked == true
                               select d;

                if (stockIns.Any())
                {
                    var deleteInventory = from d in db.TrnInventories
                                          where d.INId == stockIns.FirstOrDefault().Id
                                          select d;

                    db.TrnInventories.DeleteAllOnSubmit(deleteInventory);
                    db.SubmitChanges();

                    // Stock in items
                    var stockInItems = from d in db.TrnStockInItems
                                       where d.INId == INId
                                       select d;

                    if (stockInItems.Any())
                    {
                        foreach (var stockInItem in stockInItems)
                        {
                            // retrieve Artticle Inventory
                            var articleInventories = from d in db.MstArticleInventories
                                                     where d.BranchId == stockIns.FirstOrDefault().BranchId
                                                     && d.ArticleId == stockInItem.ItemId
                                                     select d;

                            articleInventoryId = 0;
                            if (articleInventories.Any())
                            {
                                if (stockIns.FirstOrDefault().MstUser4.InventoryType.Equals("Moving Average"))
                                {
                                    articleInventoryId = articleInventories.FirstOrDefault().Id;
                                }
                                else
                                {
                                    foreach (var articleInventory in articleInventories)
                                    {
                                        if (articleInventory.InventoryCode.Equals("IN-" + stockIns.FirstOrDefault().MstBranch.BranchCode + "-" + stockIns.FirstOrDefault().INNumber))
                                        {
                                            articleInventoryId = articleInventory.Id;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (articleInventoryId > 0)
                            {
                                UpdateArticleInventory(articleInventoryId, stockIns.FirstOrDefault().MstUser4.InventoryType);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}