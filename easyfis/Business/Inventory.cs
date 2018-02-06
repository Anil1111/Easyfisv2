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
                        if (receivingReceiptItem.BaseQuantity > 0)
                        {
                            // ==============
                            // Moving Average
                            // ==============
                            if (receivingReceiptItem.InventoryType.Equals("Moving Average"))
                            {
                                var articleInventories = from d in db.MstArticleInventories
                                                         where d.BranchId == receivingReceiptItem.BranchId
                                                         && d.ArticleId == receivingReceiptItem.ItemId
                                                         select d;

                                if (articleInventories.Any())
                                {
                                    // ===========================================================
                                    // Insert Inventory (Moving Average) - Article Inventory Exist
                                    // ===========================================================
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
                                    // =============================================
                                    // Insert New Article Inventory (Moving Average)
                                    // =============================================
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

                                    // =================================
                                    // Insert Inventory (Moving Average)
                                    // =================================
                                    Data.TrnInventory newInventory = new Data.TrnInventory
                                    {
                                        BranchId = receivingReceiptItem.BranchId,
                                        InventoryDate = receivingReceiptItem.RRDate,
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
                                }
                            }
                            else
                            {
                                // =======================
                                // Specific Identification
                                // =======================
                                var articleInventories = from d in db.MstArticleInventories
                                                         where d.BranchId == receivingReceiptItem.BranchId
                                                         && d.ArticleId == receivingReceiptItem.ItemId
                                                         && d.InventoryCode.Equals("RR-" + receivingReceiptItem.BranchCode + "-" + receivingReceiptItem.RRNumber)
                                                         select d;

                                if (articleInventories.Any())
                                {
                                    // ====================================================================
                                    // Insert Inventory (Specific Identification) - Article Inventory Exist
                                    // ====================================================================
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
                                    // ======================================================
                                    // Insert New Article Inventory (Specific Identification)
                                    // ======================================================
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

                                    // ==========================================
                                    // Insert Inventory (Specific Identification)
                                    // ==========================================
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

        // ==============================
        // Insert Sales Invoice Inventory
        // ==============================
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
                                // ==========================
                                // Insert New Inventory (Out)
                                // ==========================
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

        // =========================
        // Insert Stock In Inventory
        // =========================
        public void InsertStockInInventory(Int32 INId)
        {
            try
            {
                var stockInItems = from d in db.TrnStockInItems
                                   where d.INId == INId
                                   select d;

                if (stockInItems.Any())
                {
                    foreach (var stockInItem in stockInItems)
                    {
                        if (stockInItem.BaseQuantity > 0)
                        {
                            Int32 articleInventoryId = 0;
                            Int32 componentArticleInventoryId = 0;

                            if (stockInItem.TrnStockIn.IsProduced)
                            {
                                if (stockInItem.MstArticle.MstArticleComponents.Any())
                                {
                                    articleInventoryId = 0;

                                    // =================================================
                                    // Get Artticle Inventory (Stock In - Produced Item)
                                    // =================================================
                                    var articleInventories = from d in db.MstArticleInventories
                                                             where d.BranchId == stockInItem.TrnStockIn.BranchId
                                                             && d.ArticleId == stockInItem.ItemId
                                                             select d;

                                    if (articleInventories.Any())
                                    {
                                        if (stockInItem.TrnStockIn.MstUser4.InventoryType.Equals("Moving Average"))
                                        {
                                            articleInventoryId = articleInventories.FirstOrDefault().Id;
                                        }
                                        else
                                        {
                                            foreach (var articleInventory in articleInventories)
                                            {
                                                if (articleInventory.InventoryCode.Equals("IN-" + stockInItem.TrnStockIn.MstBranch.BranchCode + "-" + stockInItem.TrnStockIn.INNumber))
                                                {
                                                    articleInventoryId = articleInventory.Id;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (articleInventoryId == 0)
                                    {
                                        // ===================================================
                                        // Insert Article Inventory (Stock In - Produced Item)
                                        // ===================================================
                                        Data.MstArticleInventory newArticleInventory = new Data.MstArticleInventory
                                        {
                                            BranchId = stockInItem.TrnStockIn.BranchId,
                                            ArticleId = stockInItem.ItemId,
                                            InventoryCode = "IN-" + stockInItem.TrnStockIn.MstBranch.BranchCode + "-" + stockInItem.TrnStockIn.INNumber,
                                            Quantity = stockInItem.Quantity,
                                            Cost = stockInItem.Amount / stockInItem.Quantity,
                                            Amount = stockInItem.Amount,
                                            Particulars = stockInItem.TrnStockIn.Particulars
                                        };

                                        db.MstArticleInventories.InsertOnSubmit(newArticleInventory);
                                        db.SubmitChanges();

                                        articleInventoryId = newArticleInventory.Id;
                                    }

                                    // ===========================================
                                    // Insert Inventory (Stock In - Produced Item)
                                    // ===========================================
                                    Data.TrnInventory newInventory = new Data.TrnInventory
                                    {
                                        BranchId = stockInItem.TrnStockIn.BranchId,
                                        InventoryDate = Convert.ToDateTime(stockInItem.TrnStockIn.INDate),
                                        ArticleId = stockInItem.ItemId,
                                        ArticleInventoryId = articleInventoryId,
                                        INId = INId,
                                        QuantityIn = stockInItem.BaseQuantity,
                                        QuantityOut = 0,
                                        Quantity = stockInItem.BaseQuantity,
                                        Amount = stockInItem.Amount,
                                        Particulars = stockInItem.TrnStockIn.Particulars
                                    };

                                    db.TrnInventories.InsertOnSubmit(newInventory);
                                    db.SubmitChanges();

                                    // ===================================================
                                    // Update Article Inventory (Stock In - Produced Item)
                                    // ===================================================
                                    UpdateArticleInventory(articleInventoryId, stockInItem.TrnStockIn.MstUser4.InventoryType);

                                    // ===============
                                    // Component Items
                                    // ===============
                                    foreach (var component in stockInItem.MstArticle.MstArticleComponents)
                                    {
                                        componentArticleInventoryId = 0;

                                        // =======================================
                                        // Get Artticle Inventory (Component Item)
                                        // =======================================
                                        var componentArticleInventories = from d in db.MstArticleInventories
                                                                          where d.BranchId == stockInItem.TrnStockIn.BranchId
                                                                          && d.ArticleId == component.ComponentArticleId
                                                                          select d;

                                        if (componentArticleInventories.Any())
                                        {
                                            if (stockInItem.TrnStockIn.MstUser4.InventoryType.Equals("Moving Average"))
                                            {
                                                componentArticleInventoryId = componentArticleInventories.FirstOrDefault().Id;
                                            }
                                            else
                                            {
                                                foreach (var componentArticleInventory in componentArticleInventories)
                                                {
                                                    if (componentArticleInventory.InventoryCode.Equals("IN-" + stockInItem.TrnStockIn.MstBranch.BranchCode + "-" + stockInItem.TrnStockIn.INNumber))
                                                    {
                                                        componentArticleInventoryId = componentArticleInventory.Id;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        if (componentArticleInventoryId != 0)
                                        {
                                            // =================================
                                            // Insert Inventory (Component Item)
                                            // =================================
                                            Data.TrnInventory newComponentInventory = new Data.TrnInventory
                                            {
                                                BranchId = stockInItem.TrnStockIn.BranchId,
                                                InventoryDate = Convert.ToDateTime(stockInItem.TrnStockIn.INDate),
                                                ArticleId = component.ComponentArticleId,
                                                ArticleInventoryId = componentArticleInventoryId,
                                                INId = INId,
                                                QuantityIn = 0,
                                                QuantityOut = (component.Quantity * stockInItem.Quantity) * -1,
                                                Quantity = (component.Quantity * stockInItem.Quantity) * -1,
                                                Amount = ((component.Quantity * stockInItem.Quantity) * -1) * component.MstArticle1.MstArticleInventories.OrderByDescending(c => c.Cost).FirstOrDefault().Cost,
                                                Particulars = stockInItem.TrnStockIn.Particulars
                                            };

                                            db.TrnInventories.InsertOnSubmit(newComponentInventory);
                                            db.SubmitChanges();

                                            // =========================================
                                            // Update Article Inventory (Component Item)
                                            // =========================================
                                            UpdateArticleInventory(articleInventoryId, stockInItem.TrnStockIn.MstUser4.InventoryType);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                articleInventoryId = 0;

                                // ======================================
                                // Get Artticle Inventory (Stock In Item)
                                // ======================================
                                var articleInventories = from d in db.MstArticleInventories
                                                         where d.BranchId == stockInItem.TrnStockIn.BranchId
                                                         && d.ArticleId == stockInItem.ItemId
                                                         select d;

                                if (articleInventories.Any())
                                {
                                    if (stockInItem.TrnStockIn.MstUser4.InventoryType.Equals("Moving Average"))
                                    {
                                        articleInventoryId = articleInventories.FirstOrDefault().Id;
                                    }
                                    else
                                    {
                                        foreach (var articleInventory in articleInventories)
                                        {
                                            if (articleInventory.InventoryCode.Equals("IN-" + stockInItem.TrnStockIn.MstBranch.BranchCode + "-" + stockInItem.TrnStockIn.INNumber))
                                            {
                                                articleInventoryId = articleInventory.Id;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (articleInventoryId == 0)
                                {
                                    // ========================================
                                    // Insert Article Inventory (Stock In Item)
                                    // ========================================
                                    Data.MstArticleInventory newArticleInventory = new Data.MstArticleInventory
                                    {
                                        BranchId = stockInItem.TrnStockIn.BranchId,
                                        ArticleId = stockInItem.ItemId,
                                        InventoryCode = "IN-" + stockInItem.TrnStockIn.MstBranch.BranchCode + "-" + stockInItem.TrnStockIn.INNumber,
                                        Quantity = stockInItem.Quantity,
                                        Cost = stockInItem.Amount / stockInItem.Quantity,
                                        Amount = stockInItem.Amount,
                                        Particulars = stockInItem.TrnStockIn.Particulars
                                    };

                                    db.MstArticleInventories.InsertOnSubmit(newArticleInventory);
                                    db.SubmitChanges();

                                    articleInventoryId = newArticleInventory.Id;
                                }

                                // ================================
                                // Insert Inventory (Stock In Item)
                                // ================================
                                Data.TrnInventory newInventory = new Data.TrnInventory
                                {
                                    BranchId = stockInItem.TrnStockIn.BranchId,
                                    InventoryDate = Convert.ToDateTime(stockInItem.TrnStockIn.INDate),
                                    ArticleId = stockInItem.ItemId,
                                    ArticleInventoryId = articleInventoryId,
                                    INId = INId,
                                    QuantityIn = stockInItem.BaseQuantity,
                                    QuantityOut = 0,
                                    Quantity = stockInItem.BaseQuantity,
                                    Amount = stockInItem.Amount,
                                    Particulars = stockInItem.TrnStockIn.Particulars
                                };

                                db.TrnInventories.InsertOnSubmit(newInventory);
                                db.SubmitChanges();

                                // ========================================
                                // Update Article Inventory (Stock In Item)
                                // ========================================
                                UpdateArticleInventory(articleInventoryId, stockInItem.TrnStockIn.MstUser4.InventoryType);
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

        // =========================
        // Delete Stock In Inventory
        // =========================
        public void DeleteStockInInventory(Int32 INId)
        {
            try
            {
                // ==================
                // Delete Inventories
                // ==================
                var inventories = from d in db.TrnInventories
                                  where d.INId == INId
                                  select d;

                if (inventories.Any())
                {
                    db.TrnInventories.DeleteAllOnSubmit(inventories);
                    db.SubmitChanges();
                }

                Int32 articleInventoryId = 0;

                var stockInItems = from d in db.TrnStockInItems
                                   where d.INId == INId
                                   select d;

                if (stockInItems.Any())
                {
                    foreach (var stockInItem in stockInItems)
                    {
                        articleInventoryId = 0;

                        // ==========================
                        // Update Article Inventories
                        // ==========================
                        var articleInventories = from d in db.MstArticleInventories
                                                 where d.BranchId == stockInItem.TrnStockIn.BranchId
                                                 && d.ArticleId == stockInItem.ItemId
                                                 select d;

                        if (articleInventories.Any())
                        {
                            if (stockInItem.TrnStockIn.MstUser4.InventoryType.Equals("Moving Average"))
                            {
                                articleInventoryId = articleInventories.FirstOrDefault().Id;
                            }
                            else
                            {
                                foreach (var articleInventory in articleInventories)
                                {
                                    if (articleInventory.InventoryCode.Equals("IN-" + stockInItem.TrnStockIn.MstBranch.BranchCode + "-" + stockInItem.TrnStockIn.INNumber))
                                    {
                                        articleInventoryId = articleInventory.Id;
                                        break;
                                    }
                                }
                            }
                        }

                        if (articleInventoryId > 0)
                        {
                            UpdateArticleInventory(articleInventoryId, stockInItem.TrnStockIn.MstUser4.InventoryType);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ==========================
        // Insert Stock Out Inventory
        // ==========================
        public void InsertStockOutInventory(Int32 OTId)
        {
            try
            {
                var stockOutItems = from d in db.TrnStockOutItems
                                    where d.OTId == OTId
                                    select d;

                if (stockOutItems.Any())
                {
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

                        // ======================
                        // Insert Inventory (Out)
                        // ======================
                        Data.TrnInventory newInventory = new Data.TrnInventory
                        {
                            BranchId = stockOutItem.TrnStockOut.BranchId,
                            InventoryDate = stockOutItem.TrnStockOut.OTDate,
                            ArticleId = stockOutItem.ItemId,
                            ArticleInventoryId = stockOutItem.ItemInventoryId,
                            OTId = OTId,
                            QuantityIn = quantityIn,
                            QuantityOut = quantityOut,
                            Quantity = quantity,
                            Amount = amount,
                            Particulars = stockOutItem.Particulars
                        };

                        db.TrnInventories.InsertOnSubmit(newInventory);
                        db.SubmitChanges();

                        // ========================
                        // Update Article Inventory
                        // ========================
                        UpdateArticleInventory(stockOutItem.ItemInventoryId, stockOutItem.TrnStockOut.MstUser4.InventoryType);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ==========================
        // Delete Stock Out Inventory
        // ==========================
        public void DeleteStockOutInventory(Int32 OTId)
        {
            try
            {
                // ==================
                // Delete Inventories
                // ==================
                var inventories = from d in db.TrnInventories
                                  where d.OTId == OTId
                                  select d;

                if (inventories.Any())
                {
                    db.TrnInventories.DeleteAllOnSubmit(inventories);
                    db.SubmitChanges();
                }

                // ==========================
                // Update Article Inventories
                // ==========================
                var stockOutItems = from d in db.TrnStockOutItems
                                    where d.OTId == OTId
                                    select d;

                if (stockOutItems.Any())
                {
                    foreach (var stockOutItem in stockOutItems)
                    {
                        UpdateArticleInventory(stockOutItem.ItemInventoryId, stockOutItem.TrnStockOut.MstUser4.InventoryType);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ===============================
        // Insert Stock Transfer Inventory
        // ===============================
        public void InsertStockTransferInventory(Int32 STId)
        {
            try
            {
                var stockTransferItems = from d in db.TrnStockTransferItems
                                         where d.STId == STId
                                         select d;

                if (stockTransferItems.Any())
                {
                    Int32 articleInventoryId = 0;

                    foreach (var stockTransferItem in stockTransferItems)
                    {
                        // =======================================
                        // Insert Inventory (Stock Transfer - Out)
                        // =======================================
                        Data.TrnInventory newStockTransferItemOutInventory = new Data.TrnInventory
                        {
                            BranchId = stockTransferItem.TrnStockTransfer.BranchId,
                            InventoryDate = stockTransferItem.TrnStockTransfer.STDate,
                            ArticleId = stockTransferItem.ItemId,
                            ArticleInventoryId = stockTransferItem.ItemInventoryId,
                            STId = STId,
                            QuantityIn = 0,
                            QuantityOut = stockTransferItem.BaseQuantity,
                            Quantity = stockTransferItem.BaseQuantity * -1,
                            Amount = stockTransferItem.Amount * -1,
                            Particulars = stockTransferItem.TrnStockTransfer.Particulars
                        };

                        db.TrnInventories.InsertOnSubmit(newStockTransferItemOutInventory);
                        db.SubmitChanges();

                        // ========================
                        // Update Article Inventory
                        // ========================
                        UpdateArticleInventory(stockTransferItem.ItemInventoryId, stockTransferItem.TrnStockTransfer.MstUser4.InventoryType);

                        articleInventoryId = 0;

                        var articleInventories = from d in db.MstArticleInventories
                                                 where d.BranchId == stockTransferItem.TrnStockTransfer.ToBranchId
                                                 && d.ArticleId == stockTransferItem.ItemId
                                                 select d;

                        if (articleInventories.Any())
                        {
                            if (stockTransferItem.TrnStockTransfer.MstUser4.InventoryType.Equals("Moving Average"))
                            {
                                articleInventoryId = articleInventories.FirstOrDefault().Id;
                            }
                            else
                            {
                                foreach (var articleInventory in articleInventories)
                                {
                                    if (articleInventory.InventoryCode.Equals("ST-" + stockTransferItem.TrnStockTransfer.MstBranch1.BranchCode + "-" + stockTransferItem.TrnStockTransfer.STNumber))
                                    {
                                        articleInventoryId = articleInventory.Id;
                                        break;
                                    }
                                }
                            }
                        }

                        if (articleInventoryId == 0)
                        {
                            // ==================================================
                            // Insert New Article Inventory (Stock Transfer - In)
                            // ==================================================
                            Data.MstArticleInventory newArticleInventory = new Data.MstArticleInventory
                            {
                                BranchId = stockTransferItem.TrnStockTransfer.ToBranchId,
                                ArticleId = stockTransferItem.ItemId,
                                InventoryCode = "ST-" + stockTransferItem.TrnStockTransfer.MstBranch1.BranchCode + "-" + stockTransferItem.TrnStockTransfer.STNumber,
                                Quantity = stockTransferItem.Quantity,
                                Cost = stockTransferItem.BaseCost,
                                Amount = stockTransferItem.Amount,
                                Particulars = stockTransferItem.TrnStockTransfer.Particulars
                            };

                            db.MstArticleInventories.InsertOnSubmit(newArticleInventory);
                            db.SubmitChanges();

                            articleInventoryId = newArticleInventory.Id;
                        }

                        // ======================================
                        // Insert Inventory (Stock Transfer - In)
                        // ======================================
                        Data.TrnInventory newStockTransferItemInInventory = new Data.TrnInventory
                        {
                            BranchId = stockTransferItem.TrnStockTransfer.ToBranchId,
                            InventoryDate = Convert.ToDateTime(stockTransferItem.TrnStockTransfer.STDate),
                            ArticleId = stockTransferItem.ItemId,
                            ArticleInventoryId = articleInventoryId,
                            STId = STId,
                            QuantityIn = stockTransferItem.BaseQuantity,
                            QuantityOut = 0,
                            Quantity = stockTransferItem.BaseQuantity,
                            Amount = stockTransferItem.Amount,
                            Particulars = stockTransferItem.TrnStockTransfer.Particulars
                        };

                        db.TrnInventories.InsertOnSubmit(newStockTransferItemInInventory);
                        db.SubmitChanges();

                        // ========================
                        // Update Article Inventory
                        // ========================
                        UpdateArticleInventory(articleInventoryId, stockTransferItem.TrnStockTransfer.MstUser4.InventoryType);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ===============================
        // Delete Stock Transfer Inventory
        // ===============================
        public void DeleteStockTransferInventory(Int32 STId)
        {
            try
            {
                // ==================
                // Delete Inventories
                // ==================
                var inventories = from d in db.TrnInventories
                                  where d.STId == STId
                                  select d;

                if (inventories.Any())
                {
                    db.TrnInventories.DeleteAllOnSubmit(inventories);
                    db.SubmitChanges();
                }

                Int32 articleInventoryId = 0;

                var stockTransferItems = from d in db.TrnStockTransferItems
                                         where d.STId == STId
                                         select d;

                foreach (var stockTransferItem in stockTransferItems)
                {
                    // ==========================
                    // Update Article Inventories
                    // ==========================
                    UpdateArticleInventory(stockTransferItem.ItemInventoryId, stockTransferItem.TrnStockTransfer.MstUser4.InventoryType);

                    articleInventoryId = 0;

                    var articleInventories = from d in db.MstArticleInventories
                                             where d.BranchId == stockTransferItem.TrnStockTransfer.ToBranchId
                                             && d.ArticleId == stockTransferItem.ItemId
                                             select d;

                    if (articleInventories.Any())
                    {
                        if (stockTransferItem.TrnStockTransfer.MstUser4.InventoryType.Equals("Moving Average"))
                        {
                            articleInventoryId = articleInventories.FirstOrDefault().Id;
                        }
                        else
                        {
                            foreach (var articleInventory in articleInventories)
                            {
                                if (articleInventory.InventoryCode.Equals("ST-" + stockTransferItem.TrnStockTransfer.MstBranch1.BranchCode + "-" + stockTransferItem.TrnStockTransfer.STNumber))
                                {
                                    articleInventoryId = articleInventory.Id;
                                    break;
                                }
                            }
                        }
                    }

                    if (articleInventoryId > 0)
                    {
                        UpdateArticleInventory(articleInventoryId, stockTransferItem.TrnStockTransfer.MstUser4.InventoryType);
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