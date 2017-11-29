using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ModifiedApiControllers
{
    public class ApiTrnInvevntoryController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==================================
        // List Inventory - Receiving Receipt
        // ==================================
        [Authorize, HttpGet, Route("api/inventory/receivingReceipt/list/{RRId}")]
        public List<Entities.TrnInventory> ListInventoryReceivingReceipt(String RRId)
        {
            var inventories = from d in db.TrnInventories
                              where d.RRId == Convert.ToUInt32(RRId)
                              select new Entities.TrnInventory
                              {
                                  InventoryDate = d.InventoryDate.ToShortDateString(),
                                  Branch = d.MstBranch.Branch,
                                  Article = d.MstArticle.Article,
                                  Particulars = d.Particulars,
                                  ArticleInventoryCode = d.MstArticleInventory.InventoryCode,
                                  Quantity = d.Quantity,
                                  ArticleUnit = d.MstArticle.MstUnit.Unit,
                                  Amount = d.Amount
                              };

            return inventories.ToList();
        }

        // ==============================
        // List Inventory - Sales Invoice
        // ==============================
        [Authorize, HttpGet, Route("api/inventory/salesInvoice/list/{SIId}")]
        public List<Entities.TrnInventory> ListInventorySalesInvoice(String SIId)
        {
            var inventories = from d in db.TrnInventories
                              where d.SIId == Convert.ToUInt32(SIId)
                              select new Entities.TrnInventory
                              {
                                  InventoryDate = d.InventoryDate.ToShortDateString(),
                                  Branch = d.MstBranch.Branch,
                                  Article = d.MstArticle.Article,
                                  Particulars = d.Particulars,
                                  ArticleInventoryCode = d.MstArticleInventory.InventoryCode,
                                  Quantity = d.Quantity,
                                  ArticleUnit = d.MstArticle.MstUnit.Unit,
                                  Amount = d.Amount
                              };

            return inventories.ToList();
        }

        // =========================
        // List Inventory - Stock In
        // =========================
        [Authorize, HttpGet, Route("api/inventory/stockIn/list/{INId}")]
        public List<Entities.TrnInventory> ListInventoryStockIn(String INId)
        {
            var inventories = from d in db.TrnInventories
                              where d.INId == Convert.ToUInt32(INId)
                              select new Entities.TrnInventory
                              {
                                  InventoryDate = d.InventoryDate.ToShortDateString(),
                                  Branch = d.MstBranch.Branch,
                                  Article = d.MstArticle.Article,
                                  Particulars = d.Particulars,
                                  ArticleInventoryCode = d.MstArticleInventory.InventoryCode,
                                  Quantity = d.Quantity,
                                  ArticleUnit = d.MstArticle.MstUnit.Unit,
                                  Amount = d.Amount
                              };

            return inventories.ToList();
        }

        // ==========================
        // List Inventory - Stock Out
        // ==========================
        [Authorize, HttpGet, Route("api/inventory/stockOut/list/{OTId}")]
        public List<Entities.TrnInventory> ListInventoryStockOut(String OTId)
        {
            var inventories = from d in db.TrnInventories
                              where d.OTId == Convert.ToUInt32(OTId)
                              select new Entities.TrnInventory
                              {
                                  InventoryDate = d.InventoryDate.ToShortDateString(),
                                  Branch = d.MstBranch.Branch,
                                  Article = d.MstArticle.Article,
                                  Particulars = d.Particulars,
                                  ArticleInventoryCode = d.MstArticleInventory.InventoryCode,
                                  Quantity = d.Quantity,
                                  ArticleUnit = d.MstArticle.MstUnit.Unit,
                                  Amount = d.Amount
                              };

            return inventories.ToList();
        }

        // ===============================
        // List Inventory - Stock Transfer
        // ===============================
        [Authorize, HttpGet, Route("api/inventory/stockTransfer/list/{STId}")]
        public List<Entities.TrnInventory> ListInventoryStockTransfer(String STId)
        {
            var inventories = from d in db.TrnInventories
                              where d.STId == Convert.ToUInt32(STId)
                              select new Entities.TrnInventory
                              {
                                  InventoryDate = d.InventoryDate.ToShortDateString(),
                                  Branch = d.MstBranch.Branch,
                                  Article = d.MstArticle.Article,
                                  Particulars = d.Particulars,
                                  ArticleInventoryCode = d.MstArticleInventory.InventoryCode,
                                  Quantity = d.Quantity,
                                  ArticleUnit = d.MstArticle.MstUnit.Unit,
                                  Amount = d.Amount
                              };

            return inventories.ToList();
        }
    }
}
