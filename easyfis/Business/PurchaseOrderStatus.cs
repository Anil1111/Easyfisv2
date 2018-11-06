using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Business
{
    public class PurchaseOrderStatus
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ============================
        // Update Purchase Order Status
        // ============================
        public void UpdatePurchaseOrderStatus(Int32 RRId)
        {
            var receivedPOs = from d in db.TrnReceivingReceiptItems where d.RRId == RRId group d by d.POId into g select g;
            if (receivedPOs.Any())
            {
                foreach (var receivedPO in receivedPOs)
                {
                    Decimal balanceQuantity = 0;

                    var purchasedItems = from d in db.TrnPurchaseOrderItems where d.POId == receivedPO.Key group d by d.ItemId into g select g;
                    if (purchasedItems.Any())
                    {
                        Decimal purchasedItemsQuantity = 0;
                        Decimal receivedItemsQuantity = 0;

                        foreach (var purchasedItem in purchasedItems)
                        {
                            purchasedItemsQuantity = purchasedItem.Sum(d => d.Quantity);

                            var receivedItems = from d in db.TrnReceivingReceiptItems
                                                where d.POId == receivedPO.Key && d.ItemId == purchasedItem.Key && d.TrnReceivingReceipt.IsLocked == true
                                                group d by d.ItemId into g
                                                select g;

                            if (receivedItems.Any())
                            {
                                receivedItemsQuantity = receivedItems.FirstOrDefault().Sum(d => d.Quantity);
                            }

                            balanceQuantity += purchasedItemsQuantity - receivedItemsQuantity;
                        }
                    }

                    var currentPurchaseOrder = from d in db.TrnPurchaseOrders where d.Id == receivedPO.Key select d;
                    if (currentPurchaseOrder.Any())
                    {
                        Boolean isClose = false;
                        if (balanceQuantity <= 0)
                        {
                            isClose = true;
                        }

                        var updatePurchaseOrder = currentPurchaseOrder.FirstOrDefault();
                        updatePurchaseOrder.IsClose = isClose;
                        db.SubmitChanges();

                        balanceQuantity = 0;
                    }
                }
            }
        }
    }
}