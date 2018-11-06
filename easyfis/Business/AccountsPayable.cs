using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Business
{
    public class AccountsPayable
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =======================
        // Update Accounts Payable
        // =======================
        public void UpdateAccountsPayable(Int32 RRId)
        {
            var receivingReceipt = from d in db.TrnReceivingReceipts where d.Id == RRId select d;
            if (receivingReceipt.Any())
            {
                Decimal paidAmount = 0;
                Decimal adjustmentAmount = 0;

                var disbursementLines = from d in db.TrnDisbursementLines where d.RRId == RRId && d.TrnDisbursement.IsLocked == true && d.TrnDisbursement.IsCancelled == false select d;
                if (disbursementLines.Any())
                {
                    paidAmount = disbursementLines.Sum(d => d.Amount);
                }

                var journalVoucherLines = from d in db.TrnJournalVoucherLines where d.APRRId == RRId && d.TrnJournalVoucher.IsLocked == true && d.TrnJournalVoucher.IsCancelled == false select d;
                if (journalVoucherLines.Any())
                {
                    Decimal debitAmount = journalVoucherLines.Sum(d => d.DebitAmount);
                    Decimal creditAmount = journalVoucherLines.Sum(d => d.CreditAmount);

                    adjustmentAmount = creditAmount - debitAmount;
                }

                Decimal receivingReceiptAmount = receivingReceipt.FirstOrDefault().Amount;
                Decimal WTAXAmount = receivingReceipt.FirstOrDefault().WTaxAmount;
                Decimal balanceAmount = (receivingReceiptAmount - WTAXAmount - paidAmount) + adjustmentAmount;

                var updateReceivingReceipt = receivingReceipt.FirstOrDefault();
                updateReceivingReceipt.PaidAmount = paidAmount;
                updateReceivingReceipt.AdjustmentAmount = adjustmentAmount;
                updateReceivingReceipt.BalanceAmount = balanceAmount;

                db.SubmitChanges();
            }
        }
    }
}