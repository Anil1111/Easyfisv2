using System;
using System.Diagnostics;
using System.Linq;

namespace easyfis.Business
{
    public class Journal
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =======================================
        // Get Account on a specific Article Group
        // =======================================
        public Int32 GetAccountId(Int32 articleGroupId, Int32 branchId, String type)
        {
            var articleGroups = from d in db.MstArticleGroups
                                where d.Id == articleGroupId
                                select d;

            if (articleGroups.Any())
            {
                if (articleGroups.FirstOrDefault().MstArticleGroupBranches.Count() > 0)
                {
                    var articleGroupBranch = articleGroups.FirstOrDefault().MstArticleGroupBranches.Where(d => d.BranchId == branchId);
                    if (articleGroupBranch.Any())
                    {
                        switch (type)
                        {
                            case "Account":
                                return articleGroupBranch.FirstOrDefault().AccountId;
                            case "Sales":
                                return articleGroupBranch.FirstOrDefault().SalesAccountId;
                            case "Cost":
                                return articleGroupBranch.FirstOrDefault().CostAccountId;
                            case "Expense":
                                return articleGroupBranch.FirstOrDefault().ExpenseAccountId;
                            case "Asset":
                                return articleGroupBranch.FirstOrDefault().AssetAccountId;
                            default:
                                return 0;
                        }
                    }
                    else
                    {
                        switch (type)
                        {
                            case "Account":
                                return articleGroups.FirstOrDefault().AccountId;
                            case "Sales":
                                return articleGroups.FirstOrDefault().SalesAccountId;
                            case "Cost":
                                return articleGroups.FirstOrDefault().CostAccountId;
                            case "Expense":
                                return articleGroups.FirstOrDefault().ExpenseAccountId;
                            case "Asset":
                                return articleGroups.FirstOrDefault().AssetAccountId;
                            default:
                                return 0;
                        }
                    }
                }
                else
                {
                    switch (type)
                    {
                        case "Account":
                            return articleGroups.FirstOrDefault().AccountId;
                        case "Sales":
                            return articleGroups.FirstOrDefault().SalesAccountId;
                        case "Cost":
                            return articleGroups.FirstOrDefault().CostAccountId;
                        case "Expense":
                            return articleGroups.FirstOrDefault().ExpenseAccountId;
                        case "Asset":
                            return articleGroups.FirstOrDefault().AssetAccountId;
                        default:
                            return 0;
                    }
                }
            }
            else
            {
                return 0;
            }
        }

        // ================================
        // Insert Receiving Receipt Journal
        // ================================
        public void InsertReceivingReceiptJournal(Int32 RRId)
        {
            try
            {

                var receivingReceipts = from d in db.TrnReceivingReceipts
                                        where d.Id == RRId
                                        select d;

                if (receivingReceipts.Any())
                {
                    // ===========
                    // Debit: Item
                    // ===========
                    var receivingReceiptItems = from d in db.TrnReceivingReceiptItems
                                                where d.RRId == RRId
                                                group d by new
                                                {
                                                    ReceivingReceipt = d.TrnReceivingReceipt,
                                                    ArticleGroupId = d.MstArticle.ArticleGroupId
                                                } into g
                                                select new
                                                {
                                                    ArticleGroupId = g.Key.ArticleGroupId,
                                                    Particulars = g.Key.ReceivingReceipt.Remarks,
                                                    Amount = g.Sum(d => d.Amount - d.VATAmount)
                                                };

                    if (receivingReceiptItems.Any())
                    {
                        foreach (var receivingReceiptItem in receivingReceiptItems)
                        {
                            Data.TrnJournal newRRItemJournal = new Data.TrnJournal
                            {
                                JournalDate = receivingReceipts.FirstOrDefault().RRDate,
                                BranchId = receivingReceipts.FirstOrDefault().BranchId,
                                AccountId = GetAccountId(receivingReceiptItem.ArticleGroupId, receivingReceipts.FirstOrDefault().BranchId, "Account"),
                                ArticleId = receivingReceipts.FirstOrDefault().SupplierId,
                                Particulars = receivingReceiptItem.Particulars,
                                DebitAmount = receivingReceiptItem.Amount,
                                CreditAmount = 0,
                                RRId = RRId,
                                DocumentReference = "RR-" + receivingReceipts.FirstOrDefault().MstBranch.BranchCode + "-" + receivingReceipts.FirstOrDefault().RRNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newRRItemJournal);
                        }

                    }

                    // ==========
                    // Debit: VAT 
                    // ==========
                    var receivingReceiptTaxes = from d in db.TrnReceivingReceiptItems
                                                where d.RRId == RRId
                                                group d by new
                                                {
                                                    ReceivingReceipt = d.TrnReceivingReceipt,
                                                    TaxAccountId = d.MstTaxType.AccountId
                                                } into g
                                                select new
                                                {
                                                    TaxAccountId = g.Key.TaxAccountId,
                                                    Particulars = g.Key.ReceivingReceipt.Remarks,
                                                    TaxAmount = g.Sum(d => d.VATAmount)
                                                };

                    if (receivingReceiptTaxes.Any())
                    {
                        foreach (var receivingReceiptTax in receivingReceiptTaxes)
                        {
                            Data.TrnJournal newRRItemTaxJournal = new Data.TrnJournal
                            {
                                JournalDate = receivingReceipts.FirstOrDefault().RRDate,
                                BranchId = receivingReceipts.FirstOrDefault().BranchId,
                                AccountId = receivingReceiptTax.TaxAccountId,
                                ArticleId = receivingReceipts.FirstOrDefault().SupplierId,
                                Particulars = receivingReceiptTax.Particulars,
                                DebitAmount = receivingReceiptTax.TaxAmount,
                                CreditAmount = 0,
                                RRId = RRId,
                                DocumentReference = "RR-" + receivingReceipts.FirstOrDefault().MstBranch.BranchCode + "-" + receivingReceipts.FirstOrDefault().RRNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newRRItemTaxJournal);
                        }

                    }

                    // ====================================
                    // Credit - Supplier (Accounts Payable)
                    // ====================================
                    Data.TrnJournal newRRSupplierJournal = new Data.TrnJournal
                    {
                        JournalDate = receivingReceipts.FirstOrDefault().RRDate,
                        BranchId = receivingReceipts.FirstOrDefault().BranchId,
                        AccountId = receivingReceipts.FirstOrDefault().MstArticle.AccountId,
                        ArticleId = receivingReceipts.FirstOrDefault().SupplierId,
                        Particulars = receivingReceipts.FirstOrDefault().Remarks,
                        DebitAmount = 0,
                        CreditAmount = receivingReceipts.FirstOrDefault().Amount,
                        RRId = RRId,
                        DocumentReference = "RR-" + receivingReceipts.FirstOrDefault().MstBranch.BranchCode + "-" + receivingReceipts.FirstOrDefault().RRNumber
                    };

                    db.TrnJournals.InsertOnSubmit(newRRSupplierJournal);

                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ================================
        // Delete Receiving Receipt Journal
        // ================================
        public void DeleteReceivingReceiptJournal(Int32 RRId)
        {
            try
            {
                var journals = from d in db.TrnJournals
                               where d.RRId == RRId
                               select d;

                if (journals.Any())
                {
                    db.TrnJournals.DeleteAllOnSubmit(journals);
                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ============================
        // Insert Sales Invoice Journal
        // ============================
        public void InsertSalesInvoiceJournal(Int32 SIId)
        {
            try
            {
                var salesInvoices = from d in db.TrnSalesInvoices
                                    where d.Id == SIId
                                    select d;

                if (salesInvoices.Any())
                {
                    // ====================================
                    // Debit: Account Receivable (Customer)
                    // ====================================
                    Data.TrnJournal newSalesJournal = new Data.TrnJournal
                    {
                        JournalDate = salesInvoices.FirstOrDefault().SIDate,
                        BranchId = salesInvoices.FirstOrDefault().BranchId,
                        AccountId = GetAccountId(salesInvoices.FirstOrDefault().MstArticle.ArticleGroupId, salesInvoices.FirstOrDefault().BranchId, "Account"),
                        ArticleId = salesInvoices.FirstOrDefault().CustomerId,
                        Particulars = salesInvoices.FirstOrDefault().Remarks,
                        DebitAmount = salesInvoices.FirstOrDefault().Amount,
                        CreditAmount = 0,
                        SIId = SIId,
                        DocumentReference = "SI-" + salesInvoices.FirstOrDefault().MstBranch.BranchCode + "-" + salesInvoices.FirstOrDefault().SINumber
                    };

                    db.TrnJournals.InsertOnSubmit(newSalesJournal);

                    // =======================================
                    // Credit: Sales Line Amount Excluding VAT
                    // =======================================
                    var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                            where d.SIId == SIId
                                            group d by new
                                            {
                                                SalesInvoice = d.TrnSalesInvoice,
                                                ArticleGroupId = d.MstArticle.ArticleGroupId
                                            } into g
                                            select new
                                            {
                                                ArticleGroupId = g.Key.ArticleGroupId,
                                                Amount = g.Sum(d => d.Amount - d.VATAmount)
                                            };

                    if (salesInvoiceItems.Any())
                    {
                        foreach (var salesInvoiceItem in salesInvoiceItems)
                        {
                            Data.TrnJournal newSalesInvoiceItemsJournal = new Data.TrnJournal
                            {
                                JournalDate = salesInvoices.FirstOrDefault().SIDate,
                                BranchId = salesInvoices.FirstOrDefault().BranchId,
                                AccountId = GetAccountId(salesInvoiceItem.ArticleGroupId, salesInvoices.FirstOrDefault().BranchId, "Sales"),
                                ArticleId = salesInvoices.FirstOrDefault().CustomerId,
                                Particulars = salesInvoices.FirstOrDefault().Remarks,
                                DebitAmount = 0,
                                CreditAmount = salesInvoiceItem.Amount,
                                SIId = SIId,
                                DocumentReference = "SI-" + salesInvoices.FirstOrDefault().MstBranch.BranchCode + "-" + salesInvoices.FirstOrDefault().SINumber
                            };

                            db.TrnJournals.InsertOnSubmit(newSalesInvoiceItemsJournal);
                        }
                    }

                    // ======================
                    // Credit: Sales Line VAT
                    // ======================
                    var salesInvoiceVATItems = from d in db.TrnSalesInvoiceItems
                                               where d.SIId == SIId
                                               group d by new
                                               {
                                                   SalesInvoice = d.TrnSalesInvoice,
                                                   AccountId = d.MstTaxType.AccountId
                                               } into g
                                               select new
                                               {
                                                   AccountId = g.Key.AccountId,
                                                   VATAmount = g.Sum(d => d.VATAmount)
                                               };

                    if (salesInvoiceVATItems.Any())
                    {
                        foreach (var salesInvoiceVATItem in salesInvoiceVATItems)
                        {
                            Data.TrnJournal newSalesInvoiceVATItemsJournal = new Data.TrnJournal
                            {
                                JournalDate = salesInvoices.FirstOrDefault().SIDate,
                                BranchId = salesInvoices.FirstOrDefault().BranchId,
                                AccountId = salesInvoiceVATItem.AccountId,
                                ArticleId = salesInvoices.FirstOrDefault().CustomerId,
                                Particulars = salesInvoices.FirstOrDefault().Remarks,
                                DebitAmount = 0,
                                CreditAmount = salesInvoiceVATItem.VATAmount,
                                SIId = SIId,
                                DocumentReference = "SI-" + salesInvoices.FirstOrDefault().MstBranch.BranchCode + "-" + salesInvoices.FirstOrDefault().SINumber
                            };

                            db.TrnJournals.InsertOnSubmit(newSalesInvoiceVATItemsJournal);
                        }
                    }

                    // ====================
                    // Debit: Cost of Sales
                    // ====================
                    var salesInvoiceCostItems = from d in db.TrnSalesInvoiceItems
                                                where d.SIId == SIId && d.MstArticle.IsInventory == true && d.ItemInventoryId > 0
                                                group d by new
                                                {
                                                    SalesInvoice = d.TrnSalesInvoice,
                                                    ArticleGroupId = d.MstArticle.ArticleGroupId
                                                } into g
                                                select new
                                                {
                                                    ArticleGroupId = g.Key.ArticleGroupId,
                                                    Amount = g.Sum(d => d.MstArticle.IsConsignment == true ? ((d.NetPrice * (d.MstArticle.ConsignmentCostPercentage / 100)) + d.MstArticle.ConsignmentCostValue) * d.Quantity : d.MstArticleInventory.Cost * d.Quantity)
                                                };

                    if (salesInvoiceCostItems.Any())
                    {
                        foreach (var salesInvoiceCostItem in salesInvoiceCostItems)
                        {
                            Data.TrnJournal newSalesInvoiceCostItemsJournal = new Data.TrnJournal
                            {
                                JournalDate = salesInvoices.FirstOrDefault().SIDate,
                                BranchId = salesInvoices.FirstOrDefault().BranchId,
                                AccountId = GetAccountId(salesInvoiceCostItem.ArticleGroupId, salesInvoices.FirstOrDefault().BranchId, "Cost"),
                                ArticleId = salesInvoices.FirstOrDefault().CustomerId,
                                Particulars = salesInvoices.FirstOrDefault().Remarks,
                                DebitAmount = salesInvoiceCostItem.Amount,
                                CreditAmount = 0,
                                SIId = SIId,
                                DocumentReference = "SI-" + salesInvoices.FirstOrDefault().MstBranch.BranchCode + "-" + salesInvoices.FirstOrDefault().SINumber
                            };

                            db.TrnJournals.InsertOnSubmit(newSalesInvoiceCostItemsJournal);
                        }
                    }

                    // =================
                    // Credit: Inventory
                    // =================
                    var salesInvoiceInventoryItems = from d in db.TrnSalesInvoiceItems
                                                     where d.SIId == SIId && d.MstArticle.IsInventory == true && d.ItemInventoryId > 0
                                                     group d by new
                                                     {
                                                         SalesInvoice = d.TrnSalesInvoice,
                                                         ArticleGroupId = d.MstArticle.ArticleGroupId
                                                     } into g
                                                     select new
                                                     {
                                                         ArticleGroupId = g.Key.ArticleGroupId,
                                                         Amount = g.Sum(d => d.MstArticle.IsConsignment == true ? ((d.NetPrice * (d.MstArticle.ConsignmentCostPercentage / 100)) + d.MstArticle.ConsignmentCostValue) * d.Quantity : d.MstArticleInventory.Cost * d.Quantity)
                                                     };

                    if (salesInvoiceInventoryItems.Any())
                    {
                        foreach (var salesInvoiceInventoryItem in salesInvoiceInventoryItems)
                        {
                            Data.TrnJournal newSalesInvoiceInventoryItemsJournal = new Data.TrnJournal
                            {
                                JournalDate = salesInvoices.FirstOrDefault().SIDate,
                                BranchId = salesInvoices.FirstOrDefault().BranchId,
                                AccountId = GetAccountId(salesInvoiceInventoryItem.ArticleGroupId, salesInvoices.FirstOrDefault().BranchId, "Account"),
                                ArticleId = salesInvoices.FirstOrDefault().CustomerId,
                                Particulars = salesInvoices.FirstOrDefault().Remarks,
                                DebitAmount = 0,
                                CreditAmount = salesInvoiceInventoryItem.Amount,
                                SIId = SIId,
                                DocumentReference = "SI-" + salesInvoices.FirstOrDefault().MstBranch.BranchCode + "-" + salesInvoices.FirstOrDefault().SINumber
                            };

                            db.TrnJournals.InsertOnSubmit(newSalesInvoiceInventoryItemsJournal);
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

        // ============================
        // Delete Sales Invoice Journal
        // ============================
        public void DeleteSalesInvoiceJournal(Int32 SIId)
        {
            try
            {
                var journals = from d in db.TrnJournals
                               where d.SIId == SIId
                               select d;

                if (journals.Any())
                {
                    db.TrnJournals.DeleteAllOnSubmit(journals);
                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ============================================
        // Insert Official Receipt (Collection) Journal
        // ============================================
        public void InsertOfficialReceiptJournal(Int32 ORId)
        {
            try
            {
                var collections = from d in db.TrnCollections
                                  where d.Id == ORId
                                  select d;

                if (collections.Any())
                {
                    // =============================
                    // Debit: Lines Pay Type Account
                    // =============================
                    var collectionLinesDebitPayTypes = from d in db.TrnCollectionLines
                                                       where d.ORId == ORId && d.Amount > 0
                                                       group d by new
                                                       {
                                                           AccountId = d.MstPayType.AccountId,
                                                           ArticleId = d.ArticleId
                                                       } into g
                                                       select new
                                                       {
                                                           AccountId = g.Key.AccountId,
                                                           ArticleId = g.Key.ArticleId,
                                                           Amount = g.Sum(d => d.Amount)
                                                       };

                    if (collectionLinesDebitPayTypes.Any())
                    {
                        foreach (var collectionLinesDebitPayType in collectionLinesDebitPayTypes)
                        {
                            Data.TrnJournal newCollectionLinesDebitPayTypeJournal = new Data.TrnJournal
                            {
                                JournalDate = collections.FirstOrDefault().ORDate,
                                BranchId = collections.FirstOrDefault().MstBranch.Id,
                                AccountId = collectionLinesDebitPayType.AccountId,
                                ArticleId = collectionLinesDebitPayType.ArticleId,
                                Particulars = collections.FirstOrDefault().Particulars,
                                DebitAmount = collectionLinesDebitPayType.Amount,
                                CreditAmount = 0,
                                ORId = ORId,
                                DocumentReference = "OR-" + collections.FirstOrDefault().MstBranch.BranchCode + "-" + collections.FirstOrDefault().ORNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newCollectionLinesDebitPayTypeJournal);
                        }
                    }

                    // ================================================
                    // Credit: Lines Pay Type Account (Negative Amount)
                    // ================================================
                    var collectionLinesCreditPayTypes = from d in db.TrnCollectionLines
                                                        where d.ORId == ORId && d.Amount < 0
                                                        group d by new
                                                        {
                                                            AccountId = d.MstPayType.AccountId,
                                                            ArticleId = d.ArticleId
                                                        } into g
                                                        select new
                                                        {
                                                            AccountId = g.Key.AccountId,
                                                            ArticleId = g.Key.ArticleId,
                                                            Amount = g.Sum(d => d.Amount)
                                                        };

                    if (collectionLinesCreditPayTypes.Any())
                    {
                        foreach (var collectionLinesCreditPayType in collectionLinesCreditPayTypes)
                        {
                            Data.TrnJournal newCollectionLinesDebitPayTypeJournal = new Data.TrnJournal
                            {
                                JournalDate = collections.FirstOrDefault().ORDate,
                                BranchId = collections.FirstOrDefault().MstBranch.Id,
                                AccountId = collectionLinesCreditPayType.AccountId,
                                ArticleId = collectionLinesCreditPayType.ArticleId,
                                Particulars = collections.FirstOrDefault().Particulars,
                                DebitAmount = 0,
                                CreditAmount = collectionLinesCreditPayType.Amount * -1,
                                ORId = ORId,
                                DocumentReference = "OR-" + collections.FirstOrDefault().MstBranch.BranchCode + "-" + collections.FirstOrDefault().ORNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newCollectionLinesDebitPayTypeJournal);
                        }
                    }

                    // =====================
                    // Credit: Lines Account
                    // =====================
                    var collectionLinesCreditAccounts = from d in db.TrnCollectionLines
                                                        where d.ORId == ORId && d.Amount > 0
                                                        group d by new
                                                        {
                                                            AccountId = d.AccountId,
                                                            ArticleId = d.ArticleId
                                                        } into g
                                                        select new
                                                        {
                                                            AccountId = g.Key.AccountId,
                                                            ArticleId = g.Key.ArticleId,
                                                            Amount = g.Sum(d => d.Amount)
                                                        };

                    if (collectionLinesCreditAccounts.Any())
                    {
                        foreach (var collectionLinesCreditAccount in collectionLinesCreditAccounts)
                        {
                            Data.TrnJournal newCollectionLinesCreditAccountJournal = new Data.TrnJournal
                            {
                                JournalDate = collections.FirstOrDefault().ORDate,
                                BranchId = collections.FirstOrDefault().MstBranch.Id,
                                AccountId = collectionLinesCreditAccount.AccountId,
                                ArticleId = collectionLinesCreditAccount.ArticleId,
                                Particulars = collections.FirstOrDefault().Particulars,
                                DebitAmount = 0,
                                CreditAmount = collectionLinesCreditAccount.Amount,
                                ORId = ORId,
                                DocumentReference = "OR-" + collections.FirstOrDefault().MstBranch.BranchCode + "-" + collections.FirstOrDefault().ORNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newCollectionLinesCreditAccountJournal);
                        }
                    }

                    // ======================================
                    // Debit: Lines Account (Negative Amount)
                    // ======================================
                    var collectionLinesDebitAccounts = from d in db.TrnCollectionLines
                                                       where d.ORId == ORId && d.Amount < 0
                                                       group d by new
                                                       {
                                                           AccountId = d.AccountId,
                                                           ArticleId = d.ArticleId
                                                       } into g
                                                       select new
                                                       {
                                                           AccountId = g.Key.AccountId,
                                                           ArticleId = g.Key.ArticleId,
                                                           Amount = g.Sum(d => d.Amount)
                                                       };

                    if (collectionLinesDebitAccounts.Any())
                    {
                        foreach (var collectionLinesDebitAccount in collectionLinesDebitAccounts)
                        {
                            Data.TrnJournal newCollectionLinesDebitAccounJournal = new Data.TrnJournal
                            {
                                JournalDate = collections.FirstOrDefault().ORDate,
                                BranchId = collections.FirstOrDefault().MstBranch.Id,
                                AccountId = collectionLinesDebitAccount.AccountId,
                                ArticleId = collectionLinesDebitAccount.ArticleId,
                                Particulars = collections.FirstOrDefault().Particulars,
                                DebitAmount = collectionLinesDebitAccount.Amount * -1,
                                CreditAmount = 0,
                                ORId = ORId,
                                DocumentReference = "OR-" + collections.FirstOrDefault().MstBranch.BranchCode + "-" + collections.FirstOrDefault().ORNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newCollectionLinesDebitAccounJournal);
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

        // ============================================
        // Delete Official Receipt (Collection) Journal
        // ============================================
        public void DeleteOfficialReceiptJournal(Int32 ORId)
        {
            try
            {
                var journals = from d in db.TrnJournals
                               where d.ORId == ORId
                               select d;

                if (journals.Any())
                {
                    db.TrnJournals.DeleteAllOnSubmit(journals);
                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ==========================================
        // Insert Cash Voucher (Disbursement) Journal
        // ==========================================
        public void InsertCashVoucherJournal(Int32 CVId)
        {
            try
            {
                var disbursements = from d in db.TrnDisbursements
                                    where d.Id == CVId
                                    select d;

                if (disbursements.Any())
                {
                    // ============================
                    // Debit - Line Positive Amount
                    // ============================
                    var disbursementPositiveLines = from d in db.TrnDisbursementLines
                                                    where d.CVId == CVId && d.Amount > 0
                                                    group d by new
                                                    {
                                                        BranchId = d.TrnDisbursement.BranchId,
                                                        AccountId = d.AccountId,
                                                        ArticleId = d.ArticleId
                                                    } into g
                                                    select new
                                                    {
                                                        BranchId = g.Key.BranchId,
                                                        AccountId = g.Key.AccountId,
                                                        ArticleId = g.Key.ArticleId,
                                                        Amount = g.Sum(d => d.Amount)
                                                    };

                    if (disbursementPositiveLines.Any())
                    {
                        foreach (var disbursementPositiveLine in disbursementPositiveLines)
                        {
                            Data.TrnJournal newDisbursementPositiveLineJournal = new Data.TrnJournal
                            {
                                JournalDate = disbursements.FirstOrDefault().CVDate,
                                BranchId = disbursementPositiveLine.BranchId,
                                AccountId = disbursementPositiveLine.AccountId,
                                ArticleId = disbursementPositiveLine.ArticleId,
                                Particulars = disbursements.FirstOrDefault().Particulars,
                                DebitAmount = disbursementPositiveLine.Amount,
                                CreditAmount = 0,
                                CVId = CVId,
                                DocumentReference = "CV-" + disbursements.FirstOrDefault().MstBranch.BranchCode + "-" + disbursements.FirstOrDefault().CVNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newDisbursementPositiveLineJournal);
                        }
                    }

                    // =============================
                    // Credit - Line Negative Amount
                    // =============================
                    var disbursementNegativeLines = from d in db.TrnDisbursementLines
                                                    where d.CVId == CVId && d.Amount < 0
                                                    group d by new
                                                    {
                                                        BranchId = d.TrnDisbursement.BranchId,
                                                        AccountId = d.AccountId,
                                                        ArticleId = d.ArticleId
                                                    } into g
                                                    select new
                                                    {
                                                        BranchId = g.Key.BranchId,
                                                        AccountId = g.Key.AccountId,
                                                        ArticleId = g.Key.ArticleId,
                                                        Amount = g.Sum(d => d.Amount)
                                                    };

                    if (disbursementNegativeLines.Any())
                    {
                        foreach (var disbursementNegativeLine in disbursementNegativeLines)
                        {
                            Data.TrnJournal newDisbursementNegativeLineJournal = new Data.TrnJournal
                            {
                                JournalDate = disbursements.FirstOrDefault().CVDate,
                                BranchId = disbursementNegativeLine.BranchId,
                                AccountId = disbursementNegativeLine.AccountId,
                                ArticleId = disbursementNegativeLine.ArticleId,
                                Particulars = disbursements.FirstOrDefault().Particulars,
                                DebitAmount = 0,
                                CreditAmount = disbursementNegativeLine.Amount * -1,
                                CVId = CVId,
                                DocumentReference = "CV-" + disbursements.FirstOrDefault().MstBranch.BranchCode + "-" + disbursements.FirstOrDefault().CVNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newDisbursementNegativeLineJournal);
                        }
                    }

                    if (disbursements.FirstOrDefault().Amount != 0)
                    {
                        // ======================
                        // Credit - Header (Bank)
                        // ======================
                        Data.TrnJournal newDisbursementJournal = new Data.TrnJournal
                        {
                            JournalDate = disbursements.FirstOrDefault().CVDate,
                            BranchId = disbursements.FirstOrDefault().BranchId,
                            AccountId = GetAccountId(disbursements.FirstOrDefault().MstArticle1.ArticleGroupId, disbursements.FirstOrDefault().BranchId, "Account"),
                            ArticleId = disbursements.FirstOrDefault().BankId,
                            Particulars = disbursements.FirstOrDefault().Particulars,
                            DebitAmount = 0,
                            CreditAmount = disbursements.FirstOrDefault().Amount,
                            CVId = CVId,
                            DocumentReference = "CV-" + disbursements.FirstOrDefault().MstBranch.BranchCode + "-" + disbursements.FirstOrDefault().CVNumber
                        };

                        db.TrnJournals.InsertOnSubmit(newDisbursementJournal);
                    }

                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ==========================================
        // Delete Cash Voucher (Disbursement) Journal
        // ==========================================
        public void DeleteCashVoucherJournal(Int32 CVId)
        {
            try
            {
                var journals = from d in db.TrnJournals
                               where d.CVId == CVId
                               select d;

                if (journals.Any())
                {
                    db.TrnJournals.DeleteAllOnSubmit(journals);
                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // =======================
        // Insert Stock In Journal
        // =======================
        public void InsertStockInJournal(Int32 INId)
        {
            try
            {
                var stockIns = from d in db.TrnStockIns
                               where d.Id == INId
                               select d;

                if (stockIns.Any())
                {
                    // ==============================
                    // Debit: Lines (Inventory Items)
                    // ==============================
                    var stockInDebitItems = from d in db.TrnStockInItems
                                            where d.INId == INId
                                            group d by new
                                            {
                                                ArticleGroup = d.MstArticle.MstArticleGroup
                                            } into g
                                            select new
                                            {
                                                ArticleGroup = g.Key.ArticleGroup,
                                                Amount = g.Sum(d => d.Amount)
                                            };

                    if (stockInDebitItems.Any())
                    {
                        foreach (var stockInDebitItem in stockInDebitItems)
                        {
                            Data.TrnJournal newStockInDebitItemJournal = new Data.TrnJournal
                            {
                                JournalDate = stockIns.FirstOrDefault().INDate,
                                BranchId = stockIns.FirstOrDefault().BranchId,
                                AccountId = GetAccountId(stockInDebitItem.ArticleGroup.Id, stockIns.FirstOrDefault().BranchId, "Account"),
                                ArticleId = stockIns.FirstOrDefault().ArticleId,
                                Particulars = stockIns.FirstOrDefault().Particulars,
                                DebitAmount = stockInDebitItem.Amount,
                                CreditAmount = 0,
                                INId = INId,
                                DocumentReference = "IN-" + stockIns.FirstOrDefault().MstBranch.BranchCode + "-" + stockIns.FirstOrDefault().INNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newStockInDebitItemJournal);
                        }
                    }

                    // ======================
                    // Component (Production)
                    // ======================
                    if (stockIns.FirstOrDefault().IsProduced)
                    {
                        var stockInArticleComponents = from d in db.TrnStockInItems
                                                       where d.INId == INId
                                                       select new
                                                       {
                                                           Quantity = d.Quantity,
                                                           ArticleComponent = d.MstArticle.MstArticleComponents
                                                       };

                        if (stockInArticleComponents.Any())
                        {
                            foreach (var stockInArticleComponent in stockInArticleComponents)
                            {
                                var articleComponents = from d in stockInArticleComponent.ArticleComponent.ToList()
                                                        group d by new
                                                        {
                                                            ArticleGroup = d.MstArticle1.MstArticleGroup
                                                        } into g
                                                        select new
                                                        {
                                                            ArticleGroup = g.Key.ArticleGroup,
                                                            Amount = g.Sum(d => d.Quantity * d.MstArticle1.MstArticleInventories.OrderByDescending(c => c.Cost).FirstOrDefault().Cost) * stockInArticleComponent.Quantity
                                                        };

                                if (articleComponents.Any())
                                {
                                    foreach (var articleComponent in articleComponents)
                                    {
                                        Data.TrnJournal newStockInCreditHeaderJournal = new Data.TrnJournal
                                        {
                                            JournalDate = stockIns.FirstOrDefault().INDate,
                                            BranchId = stockIns.FirstOrDefault().BranchId,
                                            AccountId = articleComponent.ArticleGroup.AccountId,
                                            ArticleId = stockIns.FirstOrDefault().ArticleId,
                                            Particulars = stockIns.FirstOrDefault().Particulars,
                                            DebitAmount = 0,
                                            CreditAmount = articleComponent.Amount,
                                            INId = INId,
                                            DocumentReference = "IN-" + stockIns.FirstOrDefault().MstBranch.BranchCode + "-" + stockIns.FirstOrDefault().INNumber
                                        };

                                        db.TrnJournals.InsertOnSubmit(newStockInCreditHeaderJournal);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // ==========================================
                        // Credit: Lines (Equity / Liability Account)
                        // ==========================================
                        Data.TrnJournal newStockInCreditHeaderJournal = new Data.TrnJournal
                        {
                            JournalDate = stockIns.FirstOrDefault().INDate,
                            BranchId = stockIns.FirstOrDefault().BranchId,
                            AccountId = stockIns.FirstOrDefault().AccountId,
                            ArticleId = stockIns.FirstOrDefault().ArticleId,
                            Particulars = stockIns.FirstOrDefault().Particulars,
                            DebitAmount = 0,
                            CreditAmount = stockIns.FirstOrDefault().TrnStockInItems.Sum(d => d.Amount),
                            INId = INId,
                            DocumentReference = "IN-" + stockIns.FirstOrDefault().MstBranch.BranchCode + "-" + stockIns.FirstOrDefault().INNumber
                        };

                        db.TrnJournals.InsertOnSubmit(newStockInCreditHeaderJournal);
                    }

                    db.SubmitChanges();

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // =======================
        // Delete Stock In Journal
        // =======================
        public void DeleteStockInJournal(Int32 INId)
        {
            try
            {
                var journals = from d in db.TrnJournals
                               where d.INId == INId
                               select d;

                if (journals.Any())
                {
                    db.TrnJournals.DeleteAllOnSubmit(journals);
                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ========================
        // Insert Stock Out Journal
        // ========================
        public void InsertStockOutJournal(Int32 OTId)
        {
            try
            {
                var stockOuts = from d in db.TrnStockOuts
                                where d.Id == OTId
                                select d;

                if (stockOuts.Any())
                {
                    // ==============================
                    // Debit: Lines (Expense Account)
                    // ==============================
                    Data.TrnJournal newStockOutDebitHeaderJournal = new Data.TrnJournal
                    {
                        JournalDate = stockOuts.FirstOrDefault().OTDate,
                        BranchId = stockOuts.FirstOrDefault().BranchId,
                        AccountId = stockOuts.FirstOrDefault().AccountId,
                        ArticleId = stockOuts.FirstOrDefault().ArticleId,
                        Particulars = stockOuts.FirstOrDefault().Particulars,
                        DebitAmount = stockOuts.FirstOrDefault().TrnStockOutItems.Sum(d => d.Amount),
                        CreditAmount = 0,
                        OTId = OTId,
                        DocumentReference = "OT-" + stockOuts.FirstOrDefault().MstBranch.BranchCode + "-" + stockOuts.FirstOrDefault().OTNumber
                    };

                    db.TrnJournals.InsertOnSubmit(newStockOutDebitHeaderJournal);

                    // ===============================
                    // Credit: Lines (Inventory Items)
                    // ===============================
                    var stockOutCreditItems = from d in db.TrnStockOutItems
                                              where d.OTId == OTId
                                              group d by new
                                              {
                                                  ArticleGroup = d.MstArticle.MstArticleGroup
                                              } into g
                                              select new
                                              {
                                                  ArticleGroup = g.Key.ArticleGroup,
                                                  Amount = g.Sum(d => d.Amount)
                                              };

                    if (stockOutCreditItems.Any())
                    {
                        foreach (var stockOutCreditItem in stockOutCreditItems)
                        {
                            Data.TrnJournal newStockOutCreditItemJournal = new Data.TrnJournal
                            {
                                JournalDate = stockOuts.FirstOrDefault().OTDate,
                                BranchId = stockOuts.FirstOrDefault().BranchId,
                                AccountId = GetAccountId(stockOutCreditItem.ArticleGroup.Id, stockOuts.FirstOrDefault().BranchId, "Account"),
                                ArticleId = stockOuts.FirstOrDefault().ArticleId,
                                Particulars = stockOuts.FirstOrDefault().Particulars,
                                DebitAmount = 0,
                                CreditAmount = stockOutCreditItem.Amount,
                                OTId = OTId,
                                DocumentReference = "OT-" + stockOuts.FirstOrDefault().MstBranch.BranchCode + "-" + stockOuts.FirstOrDefault().OTNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newStockOutCreditItemJournal);
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

        // ========================
        // Delete Stock Out Journal
        // ========================
        public void DeleteStockOutJournal(Int32 OTId)
        {
            try
            {
                var journals = from d in db.TrnJournals
                               where d.OTId == OTId
                               select d;

                if (journals.Any())
                {
                    db.TrnJournals.DeleteAllOnSubmit(journals);
                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // =============================
        // Insert Stock Transfer Journal
        // =============================
        public void InsertStockTransferJournal(Int32 STId)
        {
            try
            {
                var stockTransfers = from d in db.TrnStockTransfers
                                     where d.Id == STId
                                     select d;

                if (stockTransfers.Any())
                {
                    var stockTransferItems = from d in db.TrnStockTransferItems
                                             where d.STId == STId
                                             group d by new
                                             {
                                                 ArticleGroup = d.MstArticle.MstArticleGroup
                                             } into g
                                             select new
                                             {
                                                 ArticleGroup = g.Key.ArticleGroup,
                                                 Amount = g.Sum(d => d.Amount)
                                             };

                    if (stockTransferItems.Any())
                    {
                        foreach (var stockTransferItem in stockTransferItems)
                        {
                            // ==============================
                            // Debit: Lines (Inventory Items)
                            // ==============================
                            Data.TrnJournal newStockTransferItemDebitJournal = new Data.TrnJournal
                            {
                                JournalDate = stockTransfers.FirstOrDefault().STDate,
                                BranchId = stockTransfers.FirstOrDefault().ToBranchId,
                                AccountId = GetAccountId(stockTransferItem.ArticleGroup.Id, stockTransfers.FirstOrDefault().ToBranchId, "Account"),
                                ArticleId = stockTransfers.FirstOrDefault().ArticleId,
                                Particulars = stockTransfers.FirstOrDefault().Particulars,
                                DebitAmount = stockTransferItem.Amount,
                                CreditAmount = 0,
                                STId = STId,
                                DocumentReference = "ST-" + stockTransfers.FirstOrDefault().MstBranch.BranchCode + "-" + stockTransfers.FirstOrDefault().STNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newStockTransferItemDebitJournal);

                            // ===============================
                            // Credit: Lines (Inventory Items)
                            // ===============================
                            Data.TrnJournal newStockTransferItemCreditJournal = new Data.TrnJournal
                            {
                                JournalDate = stockTransfers.FirstOrDefault().STDate,
                                BranchId = stockTransfers.FirstOrDefault().BranchId,
                                AccountId = GetAccountId(stockTransferItem.ArticleGroup.Id, stockTransfers.FirstOrDefault().BranchId, "Account"),
                                ArticleId = stockTransfers.FirstOrDefault().ArticleId,
                                Particulars = stockTransfers.FirstOrDefault().Particulars,
                                DebitAmount = 0,
                                CreditAmount = stockTransferItem.Amount,
                                STId = STId,
                                DocumentReference = "ST-" + stockTransfers.FirstOrDefault().MstBranch.BranchCode + "-" + stockTransfers.FirstOrDefault().STNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newStockTransferItemCreditJournal);
                        }

                        db.SubmitChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // =============================
        // Delete Stock Transfer Journal
        // =============================
        public void DeleteStockTransferJournal(Int32 STId)
        {
            try
            {
                var journals = from d in db.TrnJournals
                               where d.STId == STId
                               select d;

                if (journals.Any())
                {
                    db.TrnJournals.DeleteAllOnSubmit(journals);
                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ===============================
        // Insert Stock Withdrawal Journal
        // ===============================
        public void InsertStockWithdrawalJournal(Int32 SWId)
        {
            try
            {
                var stockWithdrawals = from d in db.TrnStockWithdrawals
                                       where d.Id == SWId
                                       select d;

                if (stockWithdrawals.Any())
                {
                    var stockWithdrawalItems = from d in db.TrnStockWithdrawalItems
                                               where d.SWId == SWId
                                               group d by new
                                               {
                                                   ArticleGroup = d.MstArticle.MstArticleGroup
                                               } into g
                                               select new
                                               {
                                                   ArticleGroup = g.Key.ArticleGroup,
                                                   Amount = g.Sum(d => d.Amount)
                                               };

                    if (stockWithdrawalItems.Any())
                    {
                        foreach (var stockWithdrawalItem in stockWithdrawalItems)
                        {
                            // ==============================
                            // Debit: Lines (Inventory Items)
                            // ==============================
                            Data.TrnJournal newStockWithdrawalItemDebitJournal = new Data.TrnJournal
                            {
                                JournalDate = stockWithdrawals.FirstOrDefault().SWDate,
                                BranchId = stockWithdrawals.FirstOrDefault().SIBranchId,
                                AccountId = GetAccountId(stockWithdrawalItem.ArticleGroup.Id, stockWithdrawals.FirstOrDefault().SIBranchId, "Account"),
                                ArticleId = stockWithdrawals.FirstOrDefault().TrnSalesInvoice.CustomerId,
                                Particulars = stockWithdrawals.FirstOrDefault().Remarks,
                                DebitAmount = stockWithdrawalItem.Amount,
                                CreditAmount = 0,
                                SWId = SWId,
                                DocumentReference = "SW-" + stockWithdrawals.FirstOrDefault().MstBranch.BranchCode + "-" + stockWithdrawals.FirstOrDefault().SWNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newStockWithdrawalItemDebitJournal);

                            // ===============================
                            // Credit: Lines (Inventory Items)
                            // ===============================
                            Data.TrnJournal newStockWithdrawalItemCreditJournal = new Data.TrnJournal
                            {
                                JournalDate = stockWithdrawals.FirstOrDefault().SWDate,
                                BranchId = stockWithdrawals.FirstOrDefault().BranchId,
                                AccountId = GetAccountId(stockWithdrawalItem.ArticleGroup.Id, stockWithdrawals.FirstOrDefault().BranchId, "Account"),
                                ArticleId = stockWithdrawals.FirstOrDefault().TrnSalesInvoice.CustomerId,
                                Particulars = stockWithdrawals.FirstOrDefault().Remarks,
                                DebitAmount = 0,
                                CreditAmount = stockWithdrawalItem.Amount,
                                SWId = SWId,
                                DocumentReference = "SW-" + stockWithdrawals.FirstOrDefault().MstBranch.BranchCode + "-" + stockWithdrawals.FirstOrDefault().SWNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newStockWithdrawalItemCreditJournal);
                        }

                        db.SubmitChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ===============================
        // Delete Stock Withdrawal Journal
        // ===============================
        public void DeleteStockWithdrawalJournal(Int32 SWId)
        {
            try
            {
                var journals = from d in db.TrnJournals
                               where d.SWId == SWId
                               select d;

                if (journals.Any())
                {
                    db.TrnJournals.DeleteAllOnSubmit(journals);
                    db.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ==============================
        // Insert Journal Voucher Journal
        // ==============================
        public void InsertJournalVoucherJournal(Int32 JVId)
        {
            try
            {
                var journalVouchers = from d in db.TrnJournalVouchers
                                      where d.Id == JVId
                                      select d;

                if (journalVouchers.Any())
                {
                    var journalVoucherLines = from d in db.TrnJournalVoucherLines
                                              where d.JVId == JVId
                                              group d by new
                                              {
                                                  BranchId = d.BranchId,
                                                  AccountId = d.MstAccount.Id,
                                                  ArticleId = d.ArticleId,
                                              } into g
                                              select new
                                              {
                                                  BranchId = g.Key.BranchId,
                                                  AccountId = g.Key.AccountId,
                                                  ArticleId = g.Key.ArticleId,
                                                  DebitAmount = g.Sum(d => d.DebitAmount),
                                                  CreditAmount = g.Sum(d => d.CreditAmount),
                                              };

                    // ===============
                    // Journal Entries
                    // ===============
                    if (journalVoucherLines.Any())
                    {
                        foreach (var journalVoucherLine in journalVoucherLines)
                        {
                            Data.TrnJournal newJVDebitCreditJournal = new Data.TrnJournal
                            {
                                JournalDate = journalVouchers.FirstOrDefault().JVDate,
                                BranchId = journalVouchers.FirstOrDefault().BranchId,
                                AccountId = journalVoucherLine.AccountId,
                                ArticleId = journalVoucherLine.ArticleId,
                                Particulars = "JV",
                                DebitAmount = journalVoucherLine.DebitAmount,
                                CreditAmount = journalVoucherLine.CreditAmount,
                                JVId = JVId,
                                DocumentReference = "JV-" + journalVouchers.FirstOrDefault().MstBranch.BranchCode + "-" + journalVouchers.FirstOrDefault().JVNumber
                            };

                            db.TrnJournals.InsertOnSubmit(newJVDebitCreditJournal);
                        }

                        db.SubmitChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // ==============================
        // Delete Journal Voucher Journal
        // ==============================
        public void DeleteJournalVoucherJournal(Int32 JVId)
        {
            try
            {
                var journals = from d in db.TrnJournals
                               where d.JVId == JVId
                               select d;

                if (journals.Any())
                {
                    db.TrnJournals.DeleteAllOnSubmit(journals);
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