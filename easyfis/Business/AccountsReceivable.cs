using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Business
{
    public class AccountsReceivable
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==========================
        // Update Accounts Receivable
        // ==========================
        public void UpdateAccountsReceivable(Int32 SIId)
        {
            var salesInvoice = from d in db.TrnSalesInvoices where d.Id == SIId select d;
            if (salesInvoice.Any())
            {
                Decimal paidAmount = 0;
                Decimal adjustmentAmount = 0;

                var collectionLines = from d in db.TrnCollectionLines where d.SIId == SIId && d.TrnCollection.IsLocked == true && d.TrnCollection.IsCancelled == false select d;
                if (collectionLines.Any())
                {
                    paidAmount = collectionLines.Sum(d => d.Amount);
                }

                var journalVoucherLines = from d in db.TrnJournalVoucherLines where d.ARSIId == SIId && d.TrnJournalVoucher.IsLocked == true && d.TrnJournalVoucher.IsCancelled == false select d;
                if (journalVoucherLines.Any())
                {
                    Decimal debitAmount = journalVoucherLines.Sum(d => d.DebitAmount);
                    Decimal creditAmount = journalVoucherLines.Sum(d => d.CreditAmount);

                    adjustmentAmount = debitAmount - creditAmount;
                }

                Decimal salesInvoiceAmount = salesInvoice.FirstOrDefault().Amount;
                Decimal balanceAmount = (salesInvoiceAmount - paidAmount) + adjustmentAmount;

                var updateSalesInvoice = salesInvoice.FirstOrDefault();
                updateSalesInvoice.PaidAmount = paidAmount;
                updateSalesInvoice.AdjustmentAmount = adjustmentAmount;
                updateSalesInvoice.BalanceAmount = balanceAmount;

                db.SubmitChanges();
            }
        }
    }
}