﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace easyfis.Business
{
    public class PostJournal
    {
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========================
        // Sales Invoice in Journal
        // ========================
        // Insert Sales Invoice in Journal
        public void insertSIJournal(Int32 SIId)
        {
            String JournalDate = "";
            Int32 BranchId = 0;
            String BranchCode = "";
            Int32 CustomerId = 0;
            Int32 AccountId = 0;
            String SINumber = "";
            Decimal Amount;

            // SI Header
            var salesInvoiceHeader = from d in db.TrnSalesInvoices
                                     where d.Id == SIId
                                     select new Models.TrnSalesInvoice
                                     {
                                         Id = d.Id,
                                         BranchId = d.BranchId,
                                         Branch = d.MstBranch.Branch,
                                         BranchCode = d.MstBranch.BranchCode,
                                         SINumber = d.SINumber,
                                         SIDate = d.SIDate.ToShortDateString(),
                                         CustomerId = d.CustomerId,
                                         Amount = d.Amount
                                     };

            Decimal siHeaderAmount = 0;
            foreach (var siHeader in salesInvoiceHeader)
            {
                siHeaderAmount = siHeader.Amount;
            }

            Debug.WriteLine(siHeaderAmount);

            // SI Items - LINES
            var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                    where d.SIId == SIId
                                    group d by new
                                    {
                                        SalesAccountId = d.MstArticle.SalesAccountId,
                                        VATId = d.VATId,
                                        SIId = d.SIId
                                    } into g
                                    select new Models.TrnSalesInvoiceItem
                                    {
                                        SalesAccountId = g.Key.SalesAccountId,
                                        VATId = g.Key.VATId,
                                        SIId = g.Key.SIId,
                                        Amount = g.Sum(d => d.Amount),
                                        VATAmount = g.Sum(d => d.VATAmount)
                                    };

            // SI Items - VAT
            var salesInvoiceItemsForVAT = from d in db.TrnSalesInvoiceItems
                                          where d.SIId == SIId
                                          group d by new
                                          {
                                              VATId = d.VATId,
                                              SIId = d.SIId
                                          } into g
                                          select new Models.TrnSalesInvoiceItem
                                          {
                                              VATId = g.Key.VATId,
                                              SIId = g.Key.SIId,
                                              VATAmount = g.Sum(d => d.VATAmount)
                                          };

            // SI Items - Amount
            var salesInvoiceItemsForAmount = from d in db.TrnSalesInvoiceItems
                                             where d.SIId == SIId
                                             group d by new
                                             {
                                                 SIId = d.SIId,
                                                 CostAccountId = d.MstArticle.CostAccountId
                                             } into g
                                             select new Models.TrnSalesInvoiceItem
                                             {
                                                 SIId = g.Key.SIId,
                                                 CostAccountId = g.Key.CostAccountId,
                                                 Amount = g.Sum(d => d.Amount)
                                             };

            // SI Items - Inventory
            var salesInvoiceItemsForInventory = from d in db.TrnSalesInvoiceItems
                                                where d.SIId == SIId
                                                group d by new
                                                {
                                                    SIId = d.SIId,
                                                    AccountId = d.MstArticle.AccountId
                                                } into g
                                                select new Models.TrnSalesInvoiceItem
                                                {
                                                    SIId = g.Key.SIId,
                                                    AccountId = g.Key.AccountId,
                                                    Amount = g.Sum(d => d.Amount)
                                                };


            try
            {
                if (salesInvoiceHeader.Any())
                {
                    foreach (var salesInvoice in salesInvoiceHeader)
                    {
                        JournalDate = salesInvoice.SIDate;
                        BranchId = salesInvoice.BranchId;
                        BranchCode = salesInvoice.BranchCode;
                        SINumber = salesInvoice.SINumber;
                        CustomerId = salesInvoice.CustomerId;
                    }

                    // Accounts Receivable
                    if (siHeaderAmount > 0)
                    {
                        Data.TrnJournal newSIJournalForAccountItems = new Data.TrnJournal();

                        AccountId = (from d in db.MstArticles where d.Id == CustomerId select d.AccountId).SingleOrDefault();

                        newSIJournalForAccountItems.JournalDate = Convert.ToDateTime(JournalDate);
                        newSIJournalForAccountItems.BranchId = BranchId;
                        newSIJournalForAccountItems.AccountId = AccountId;
                        newSIJournalForAccountItems.ArticleId = CustomerId;
                        newSIJournalForAccountItems.Particulars = "Customer";
                        newSIJournalForAccountItems.DebitAmount = siHeaderAmount;
                        newSIJournalForAccountItems.CreditAmount = 0;
                        newSIJournalForAccountItems.ORId = null;
                        newSIJournalForAccountItems.CVId = null;
                        newSIJournalForAccountItems.JVId = null;
                        newSIJournalForAccountItems.RRId = null;
                        newSIJournalForAccountItems.SIId = SIId;
                        newSIJournalForAccountItems.INId = null;
                        newSIJournalForAccountItems.OTId = null;
                        newSIJournalForAccountItems.STId = null;
                        newSIJournalForAccountItems.DocumentReference = "SI-" + BranchCode + "-" + SINumber;
                        newSIJournalForAccountItems.APRRId = null;
                        newSIJournalForAccountItems.ARSIId = null;

                        db.TrnJournals.InsertOnSubmit(newSIJournalForAccountItems);
                    }

                    // Sales
                    if (salesInvoiceItems.Any())
                    {
                        foreach (var salesItem in salesInvoiceItems)
                        {
                            if (salesItem.Amount > 0)
                            {
                                Boolean IsInclusive;
                                IsInclusive = (from d in db.MstTaxTypes where d.Id == salesItem.VATId select d.IsInclusive).SingleOrDefault();

                                if (IsInclusive == true)
                                {
                                    Amount = salesItem.Amount - salesItem.VATAmount;
                                }
                                else
                                {
                                    Amount = salesItem.Amount;
                                }

                                Data.TrnJournal newSIJournalForSales = new Data.TrnJournal();

                                AccountId = (from d in db.MstArticles where d.Id == CustomerId select d.AccountId).SingleOrDefault();

                                newSIJournalForSales.JournalDate = Convert.ToDateTime(JournalDate);
                                newSIJournalForSales.BranchId = BranchId;
                                newSIJournalForSales.AccountId = AccountId;
                                newSIJournalForSales.ArticleId = CustomerId;
                                newSIJournalForSales.Particulars = "Sales";
                                newSIJournalForSales.DebitAmount = 0;
                                newSIJournalForSales.CreditAmount = Amount;
                                newSIJournalForSales.ORId = null;
                                newSIJournalForSales.CVId = null;
                                newSIJournalForSales.JVId = null;
                                newSIJournalForSales.RRId = null;
                                newSIJournalForSales.SIId = SIId;
                                newSIJournalForSales.INId = null;
                                newSIJournalForSales.OTId = null;
                                newSIJournalForSales.STId = null;
                                newSIJournalForSales.DocumentReference = "SI-" + BranchCode + "-" + SINumber;
                                newSIJournalForSales.APRRId = null;
                                newSIJournalForSales.ARSIId = null;

                                db.TrnJournals.InsertOnSubmit(newSIJournalForSales);
                            }
                        }
                    }

                    // VAT
                    if (salesInvoiceItemsForVAT.Any())
                    {
                        foreach (var siItemVAT in salesInvoiceItemsForVAT)
                        {
                            if (siItemVAT.VATAmount > 0)
                            {
                                Data.TrnJournal newSIJournalForVAT = new Data.TrnJournal();

                                AccountId = (from d in db.MstTaxTypes where d.Id == siItemVAT.VATId select d.AccountId).SingleOrDefault();

                                newSIJournalForVAT.JournalDate = Convert.ToDateTime(JournalDate);
                                newSIJournalForVAT.BranchId = BranchId;
                                newSIJournalForVAT.AccountId = AccountId;
                                newSIJournalForVAT.ArticleId = CustomerId;
                                newSIJournalForVAT.Particulars = "VAT Amount";
                                newSIJournalForVAT.DebitAmount = 0;
                                newSIJournalForVAT.CreditAmount = siItemVAT.VATAmount;
                                newSIJournalForVAT.ORId = null;
                                newSIJournalForVAT.CVId = null;
                                newSIJournalForVAT.JVId = null;
                                newSIJournalForVAT.RRId = null;
                                newSIJournalForVAT.SIId = SIId;
                                newSIJournalForVAT.INId = null;
                                newSIJournalForVAT.OTId = null;
                                newSIJournalForVAT.STId = null;
                                newSIJournalForVAT.DocumentReference = "SI-" + BranchCode + "-" + SINumber;
                                newSIJournalForVAT.APRRId = null;
                                newSIJournalForVAT.ARSIId = null;

                                db.TrnJournals.InsertOnSubmit(newSIJournalForVAT);
                            }
                        }
                    }

                    // cost of goods
                    if (salesInvoiceItemsForAmount.Any())
                    {
                        foreach (var siItemAmount in salesInvoiceItemsForAmount)
                        {
                            if (siItemAmount.Amount > 0)
                            {
                                Data.TrnJournal newSIJournalForAmount = new Data.TrnJournal();

                                newSIJournalForAmount.JournalDate = Convert.ToDateTime(JournalDate);
                                newSIJournalForAmount.BranchId = BranchId;
                                newSIJournalForAmount.AccountId = siItemAmount.CostAccountId;
                                newSIJournalForAmount.ArticleId = CustomerId;
                                newSIJournalForAmount.Particulars = "Item Components";
                                newSIJournalForAmount.DebitAmount = Math.Round((siItemAmount.Amount) * 100) / 100;
                                newSIJournalForAmount.CreditAmount = 0;
                                newSIJournalForAmount.ORId = null;
                                newSIJournalForAmount.CVId = null;
                                newSIJournalForAmount.JVId = null;
                                newSIJournalForAmount.RRId = null;
                                newSIJournalForAmount.SIId = SIId;
                                newSIJournalForAmount.INId = null;
                                newSIJournalForAmount.OTId = null;
                                newSIJournalForAmount.STId = null;
                                newSIJournalForAmount.DocumentReference = "SI-" + BranchCode + "-" + SINumber;
                                newSIJournalForAmount.APRRId = null;
                                newSIJournalForAmount.ARSIId = null;

                                db.TrnJournals.InsertOnSubmit(newSIJournalForAmount);
                            }
                        }
                    }

                    // Inventory
                    if (salesInvoiceItemsForInventory.Any())
                    {
                        foreach(var siItemInventory in salesInvoiceItemsForInventory)
                        {
                            if (siItemInventory.Amount > 0)
                            {
                                Data.TrnJournal newSIJournalForInventory = new Data.TrnJournal();

                                newSIJournalForInventory.JournalDate = Convert.ToDateTime(JournalDate);
                                newSIJournalForInventory.BranchId = BranchId;
                                newSIJournalForInventory.AccountId = siItemInventory.AccountId;
                                newSIJournalForInventory.ArticleId = CustomerId;
                                newSIJournalForInventory.Particulars = "Item Components";
                                newSIJournalForInventory.DebitAmount = 0;
                                newSIJournalForInventory.CreditAmount = Math.Round((siItemInventory.Amount) * 100) / 100;
                                newSIJournalForInventory.ORId = null;
                                newSIJournalForInventory.CVId = null;
                                newSIJournalForInventory.JVId = null;
                                newSIJournalForInventory.RRId = null;
                                newSIJournalForInventory.SIId = SIId;
                                newSIJournalForInventory.INId = null;
                                newSIJournalForInventory.OTId = null;
                                newSIJournalForInventory.STId = null;
                                newSIJournalForInventory.DocumentReference = "SI-" + BranchCode + "-" + SINumber;
                                newSIJournalForInventory.APRRId = null;
                                newSIJournalForInventory.ARSIId = null;

                                db.TrnJournals.InsertOnSubmit(newSIJournalForInventory);
                            }
                        }
                    }

                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

        }
        // delete Sales Invoice in Journal
        public void deleteSIJournal(Int32 SIId)
        {
            try
            {
                var journals = db.TrnJournals.Where(d => d.SIId == SIId).ToList();
                foreach (var j in journals)
                {
                    db.TrnJournals.DeleteOnSubmit(j);
                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ============================
        // Receiving Receipt in Journal
        // ============================
        // Insert Receiving Receipt in Journal
        public void insertRRJournal(Int32 RRId)
        {
            String JournalDate = "";
            Int32 BranchId = 0;
            String BranchCode = "";
            String RRNumber = "";
            Int32 SupplierId = 0;
            Boolean IsInclusive;
            Decimal amount;
            Int32 AccountId = 0;

            // rrheader
            var receivingReceiptHeader = from d in db.TrnReceivingReceipts
                                         where d.Id == RRId
                                         select new Models.TrnReceivingReceipt
                                         {
                                             RRDate = d.RRDate.ToShortDateString(),
                                             BranchId = d.BranchId,
                                             BranchCode = d.MstBranch.BranchCode,
                                             RRNumber = d.RRNumber,
                                             SupplierId = d.SupplierId,
                                             Amount = d.Amount
                                         };

            Decimal rrHeaderTotalAmount = 0;
            foreach (var rrHeader in receivingReceiptHeader)
            {
                rrHeaderTotalAmount = rrHeader.Amount;
            }

            // rritems
            var receivingReceiptItems = from d in db.TrnReceivingReceiptItems
                                        where d.RRId == RRId
                                        group d by new
                                        {
                                            BranchId = d.BranchId,
                                            AccountId = d.MstArticle.AccountId,
                                            VATId = d.VATId,
                                            RRId = d.RRId

                                        } into g
                                        select new Models.TrnReceivingReceiptItem
                                        {
                                            BranchId = g.Key.BranchId,
                                            ItemAccountId = g.Key.AccountId,
                                            VATId = g.Key.VATId,
                                            RRId = g.Key.RRId,
                                            VATAmount = g.Sum(d => d.VATAmount),
                                            WTAXAmount = g.Sum(d => d.WTAXAmount),
                                            Amount = g.Sum(d => d.Amount)
                                        };

            // rritems for VAT
            var receivingReceiptItemsForVAT = from d in db.TrnReceivingReceiptItems
                                              where d.RRId == RRId
                                              group d by new
                                              {
                                                  BranchId = d.BranchId,
                                                  VATId = d.VATId,

                                              } into g
                                              select new Models.TrnReceivingReceiptItem
                                              {
                                                  BranchId = g.Key.BranchId,
                                                  VATId = g.Key.VATId,
                                                  VATAmount = g.Sum(d => d.VATAmount)
                                              };

            // rritems for WTAX
            var receivingReceiptItemsForWTAX = from d in db.TrnReceivingReceiptItems
                                               where d.RRId == RRId
                                               group d by new
                                               {
                                                   BranchId = d.BranchId,
                                                   WTAXId = d.WTAXId,

                                               } into g
                                               select new Models.TrnReceivingReceiptItem
                                               {
                                                   BranchId = g.Key.BranchId,
                                                   WTAXId = g.Key.WTAXId,
                                                   VATAmount = g.Sum(d => d.WTAXAmount)
                                               };

            // rritems for WTAX
            var receivingReceiptItemsForTotalWTAXAmount = from d in db.TrnReceivingReceiptItems
                                                          where d.RRId == RRId
                                                          select new Models.TrnReceivingReceiptItem
                                                          {
                                                              WTAXAmount = d.WTAXAmount
                                                          };

            Decimal rrItemsTotalAmount = 0;
            Decimal rrItemsTotalVATAmount = 0;
            Decimal totalWTAXAmount = 0;

            if (!receivingReceiptItemsForTotalWTAXAmount.Any())
            {
                totalWTAXAmount = 0;
            }
            else
            {
                totalWTAXAmount = receivingReceiptItemsForTotalWTAXAmount.Sum(d => d.WTAXAmount);
            }

            try
            {
                if (receivingReceiptItems.Any())
                {
                    foreach (var rr in receivingReceiptHeader)
                    {
                        JournalDate = rr.RRDate;
                        BranchId = rr.BranchId;
                        BranchCode = rr.BranchCode;
                        RRNumber = rr.RRNumber;
                        SupplierId = rr.SupplierId;
                    }

                    // Items
                    foreach (var rrItems in receivingReceiptItems)
                    {
                        rrItemsTotalAmount = rrItems.Amount;
                        if (rrItemsTotalAmount > 0)
                        {
                            Data.TrnJournal newRRJournalForAccountItems = new Data.TrnJournal();

                            IsInclusive = (from d in db.MstTaxTypes where d.Id == rrItems.VATId select d.IsInclusive).SingleOrDefault();

                            if (IsInclusive == true)
                            {
                                amount = rrItems.Amount - rrItems.VATAmount;
                            }
                            else
                            {
                                amount = rrItems.Amount;
                            }

                            newRRJournalForAccountItems.JournalDate = Convert.ToDateTime(JournalDate);
                            newRRJournalForAccountItems.BranchId = rrItems.BranchId;
                            newRRJournalForAccountItems.AccountId = rrItems.ItemAccountId;
                            newRRJournalForAccountItems.ArticleId = SupplierId;
                            newRRJournalForAccountItems.Particulars = "Items";
                            newRRJournalForAccountItems.DebitAmount = amount;
                            newRRJournalForAccountItems.CreditAmount = 0;
                            newRRJournalForAccountItems.ORId = null;
                            newRRJournalForAccountItems.CVId = null;
                            newRRJournalForAccountItems.JVId = null;
                            newRRJournalForAccountItems.RRId = RRId;
                            newRRJournalForAccountItems.SIId = null;
                            newRRJournalForAccountItems.INId = null;
                            newRRJournalForAccountItems.OTId = null;
                            newRRJournalForAccountItems.STId = null;
                            newRRJournalForAccountItems.DocumentReference = "RR-" + BranchCode + "-" + RRNumber;
                            newRRJournalForAccountItems.APRRId = null;
                            newRRJournalForAccountItems.ARSIId = null;

                            db.TrnJournals.InsertOnSubmit(newRRJournalForAccountItems);
                        }
                    }

                    // VAT
                    foreach (var rrItemVAT in receivingReceiptItemsForVAT)
                    {
                        rrItemsTotalVATAmount = rrItemVAT.VATAmount;
                        if (rrItemsTotalVATAmount > 0)
                        {
                            Data.TrnJournal newRRJournalForVAT = new Data.TrnJournal();

                            AccountId = (from d in db.MstTaxTypes where d.Id == rrItemVAT.VATId select d.AccountId).SingleOrDefault();

                            newRRJournalForVAT.JournalDate = Convert.ToDateTime(JournalDate);
                            newRRJournalForVAT.BranchId = rrItemVAT.BranchId;
                            newRRJournalForVAT.AccountId = AccountId;
                            newRRJournalForVAT.ArticleId = SupplierId;
                            newRRJournalForVAT.Particulars = "VAT";
                            newRRJournalForVAT.DebitAmount = rrItemVAT.VATAmount;
                            newRRJournalForVAT.CreditAmount = 0;
                            newRRJournalForVAT.ORId = null;
                            newRRJournalForVAT.CVId = null;
                            newRRJournalForVAT.JVId = null;
                            newRRJournalForVAT.RRId = RRId;
                            newRRJournalForVAT.SIId = null;
                            newRRJournalForVAT.INId = null;
                            newRRJournalForVAT.OTId = null;
                            newRRJournalForVAT.STId = null;
                            newRRJournalForVAT.DocumentReference = "RR-" + BranchCode + "-" + RRNumber;
                            newRRJournalForVAT.APRRId = null;
                            newRRJournalForVAT.ARSIId = null;

                            db.TrnJournals.InsertOnSubmit(newRRJournalForVAT);
                        }
                    }

                    // Accounts Payable
                    if (rrHeaderTotalAmount > 0)
                    {
                        Data.TrnJournal newRRJournalForAccountsPayable = new Data.TrnJournal();

                        AccountId = (from d in db.MstArticles where d.Id == SupplierId select d.AccountId).SingleOrDefault();

                        newRRJournalForAccountsPayable.JournalDate = Convert.ToDateTime(JournalDate);
                        newRRJournalForAccountsPayable.BranchId = BranchId;
                        newRRJournalForAccountsPayable.AccountId = AccountId;
                        newRRJournalForAccountsPayable.ArticleId = SupplierId;
                        newRRJournalForAccountsPayable.Particulars = "AP";
                        newRRJournalForAccountsPayable.DebitAmount = 0;
                        newRRJournalForAccountsPayable.CreditAmount = receivingReceiptItems.Sum(d => d.Amount) - totalWTAXAmount;
                        newRRJournalForAccountsPayable.ORId = null;
                        newRRJournalForAccountsPayable.CVId = null;
                        newRRJournalForAccountsPayable.JVId = null;
                        newRRJournalForAccountsPayable.RRId = RRId;
                        newRRJournalForAccountsPayable.SIId = null;
                        newRRJournalForAccountsPayable.INId = null;
                        newRRJournalForAccountsPayable.OTId = null;
                        newRRJournalForAccountsPayable.STId = null;
                        newRRJournalForAccountsPayable.DocumentReference = "RR-" + BranchCode + "-" + RRNumber;
                        newRRJournalForAccountsPayable.APRRId = null;
                        newRRJournalForAccountsPayable.ARSIId = null;

                        db.TrnJournals.InsertOnSubmit(newRRJournalForAccountsPayable);
                    }

                    // WTAX
                    foreach (var rrItemWTAX in receivingReceiptItemsForWTAX)
                    {
                        if (totalWTAXAmount > 0)
                        {
                            Data.TrnJournal newRRJournalForWTAX = new Data.TrnJournal();

                            AccountId = (from d in db.MstTaxTypes where d.Id == rrItemWTAX.WTAXId select d.AccountId).SingleOrDefault();

                            newRRJournalForWTAX.JournalDate = Convert.ToDateTime(JournalDate);
                            newRRJournalForWTAX.BranchId = rrItemWTAX.BranchId;
                            newRRJournalForWTAX.AccountId = AccountId;
                            newRRJournalForWTAX.ArticleId = SupplierId;
                            newRRJournalForWTAX.Particulars = "WTAX";
                            newRRJournalForWTAX.DebitAmount = 0;
                            newRRJournalForWTAX.CreditAmount = totalWTAXAmount;
                            newRRJournalForWTAX.ORId = null;
                            newRRJournalForWTAX.CVId = null;
                            newRRJournalForWTAX.JVId = null;
                            newRRJournalForWTAX.RRId = RRId;
                            newRRJournalForWTAX.SIId = null;
                            newRRJournalForWTAX.INId = null;
                            newRRJournalForWTAX.OTId = null;
                            newRRJournalForWTAX.STId = null;
                            newRRJournalForWTAX.DocumentReference = "RR-" + BranchCode + "-" + RRNumber;
                            newRRJournalForWTAX.APRRId = null;
                            newRRJournalForWTAX.ARSIId = null;

                            db.TrnJournals.InsertOnSubmit(newRRJournalForWTAX);
                        }
                    }

                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

        }
        // delete Receiving Receipt in Journal
        public void deleteRRJournal(Int32 RRId)
        {
            try
            {
                var journals = db.TrnJournals.Where(d => d.RRId == RRId).ToList();
                foreach (var j in journals)
                {
                    db.TrnJournals.DeleteOnSubmit(j);
                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ==========================
        // Journal Voucher in Journal
        // ==========================
        // Insert Journal Voucher in Journal
        public void insertJVJournal(Int32 JVId)
        {
            var journalVoucherHeader = from d in db.TrnJournalVouchers
                                       where d.Id == JVId
                                       select new Models.TrnJournalVoucher
                                       {
                                           JVDate = d.JVDate.ToShortDateString(),
                                           BranchId = d.BranchId,
                                           BranchCode = d.MstBranch.BranchCode,
                                           JVNumber = d.JVNumber
                                       };

            String JournalDate = "";
            Int32 BranchId = 0;
            String BranchCode = "";
            String JVNumber = "";
            foreach (var jv in journalVoucherHeader)
            {
                JournalDate = jv.JVDate;
                BranchId = jv.BranchId;
                BranchCode = jv.BranchCode;
                JVNumber = jv.JVNumber;
            }

            var journalVoucherLines = from d in db.TrnJournalVoucherLines
                                      where d.JVId == JVId
                                      group d by new
                                      {
                                          BranchId = d.BranchId,
                                          AccountId = d.MstArticle.AccountId,
                                          ArticleId = d.ArticleId,
                                          JVId = d.JVId,

                                      } into g
                                      select new Models.TrnJournalVoucherLine
                                      {
                                          BranchId = g.Key.BranchId,
                                          AccountId = g.Key.AccountId,
                                          ArticleId = g.Key.ArticleId,
                                          JVId = g.Key.JVId,
                                          DebitAmount = g.Sum(d => d.DebitAmount),
                                          CreditAmount = g.Sum(d => d.CreditAmount),
                                      };

            try
            {
                foreach (var JVLs in journalVoucherLines)
                {
                    Data.TrnJournal newJVJournal = new Data.TrnJournal();

                    newJVJournal.JournalDate = Convert.ToDateTime(JournalDate);
                    newJVJournal.BranchId = JVLs.BranchId;
                    newJVJournal.AccountId = JVLs.AccountId;
                    newJVJournal.ArticleId = JVLs.ArticleId;
                    newJVJournal.Particulars = "JV";
                    newJVJournal.DebitAmount = JVLs.DebitAmount;
                    newJVJournal.CreditAmount = JVLs.CreditAmount;
                    newJVJournal.ORId = null;
                    newJVJournal.CVId = null;
                    newJVJournal.JVId = JVId;
                    newJVJournal.RRId = null;
                    newJVJournal.SIId = null;
                    newJVJournal.INId = null;
                    newJVJournal.OTId = null;
                    newJVJournal.STId = null;
                    newJVJournal.DocumentReference = "JV-" + BranchCode + "-" + JVNumber;
                    newJVJournal.APRRId = null;
                    newJVJournal.ARSIId = null;

                    db.TrnJournals.InsertOnSubmit(newJVJournal);
                }

                db.SubmitChanges();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
        // Delete Journal Voucher in Journal
        public void deleteJVJournal(Int32 JVId)
        {
            try
            {
                var journals = db.TrnJournals.Where(d => d.JVId == JVId).ToList();
                foreach (var j in journals)
                {
                    db.TrnJournals.DeleteOnSubmit(j);
                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}