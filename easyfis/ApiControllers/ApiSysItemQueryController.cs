using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;

namespace easyfis.ApiControllers
{
    public class ApiSysItemQueryController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ====================
        // List Inventory Items
        // ====================
        [Authorize, HttpGet, Route("api/sysItemQuery/list/inventory/items")]
        public List<Entities.SysItemQueryInventoryItems> ListItemQueryInventoryItems()
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var currentBranchId = currentUser.FirstOrDefault().BranchId;

            var itemInventories = from d in db.MstArticleInventories.OrderBy(d => d.MstArticle.Article)
                                  where d.BranchId == currentBranchId
                                  && d.MstArticle.IsLocked == true
                                  && d.MstArticle.IsInventory == true
                                  && d.Quantity > 0
                                  select new Entities.SysItemQueryInventoryItems
                                  {
                                      Article = d.MstArticle.Article,
                                      InventoryCode = d.InventoryCode,
                                      Quantity = d.Quantity,
                                      ManualArticleCode = d.MstArticle.ManualArticleCode,
                                      Unit = d.MstArticle.MstUnit.Unit,
                                      Price = d.MstArticle.Price
                                  };

            return itemInventories.ToList();
        }

        // ========================
        // List Non-Inventory Items
        // ========================
        [Authorize, HttpGet, Route("api/sysItemQuery/list/nonInventory/items")]
        public List<Entities.SysItemQueryNonInventoryItems> ListItemQueryNonInventoryItems()
        {
            var items = from d in db.MstArticles.OrderBy(d => d.Article)
                        where d.IsInventory == false
                        && d.IsLocked == true
                        && d.ArticleTypeId == 1
                        && d.Kitting != 2
                        select new Entities.SysItemQueryNonInventoryItems
                        {
                            ManualArticleCode = d.ManualArticleCode,
                            Article = d.Article,
                            Price = d.Price
                        };

            return items.ToList();
        }

        // ==================
        // List Package Items
        // ==================
        [Authorize, HttpGet, Route("api/sysItemQuery/list/package/items")]
        public List<Entities.SysItemQueryPackageItems> ListItemQueryPackageItems()
        {
            var items = from d in db.MstArticles.OrderBy(d => d.Article)
                        where d.ArticleTypeId == 1
                        && d.Kitting == 2
                        select new Entities.SysItemQueryPackageItems
                        {
                            ManualArticleCode = d.ManualArticleCode,
                            Article = d.Article,
                            Price = d.Price
                        };

            return items.ToList();
        }
    }
}
