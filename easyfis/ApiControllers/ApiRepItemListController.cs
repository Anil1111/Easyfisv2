using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ModifiedApiControllers
{
    public class ApiRepItemListController : ApiController
    {
        // ============
        // Data Context 
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =========
        // Item List
        // =========
        [Authorize, HttpGet, Route("api/itemList/list/{itemGroupId}")]
        public List<Entities.MstArticle> ListItemList(String itemGroupId)
        {
            var items = from d in db.MstArticles
                        where d.ArticleGroupId == Convert.ToInt32(itemGroupId)
                        && d.ArticleTypeId == 1
                        select new Entities.MstArticle
                        {
                            Id = d.Id,
                            ArticleCode = d.ArticleCode,
                            ManualArticleCode = d.ManualArticleCode,
                            Article = d.Article,
                            Category = d.Category,
                            ArticleTypeId = d.ArticleTypeId,
                            ArticleGroupId = d.ArticleGroupId,
                            ArticleGroup = d.MstArticleGroup.ArticleGroup,
                            AccountId = d.AccountId,
                            SalesAccountId = d.SalesAccountId,
                            CostAccountId = d.CostAccountId,
                            AssetAccountId = d.AssetAccountId,
                            ExpenseAccountId = d.ExpenseAccountId,
                            UnitId = d.UnitId,
                            Unit = d.MstUnit.Unit,
                            InputTaxId = d.InputTaxId,
                            OutputTaxId = d.OutputTaxId,
                            WTaxTypeId = d.WTaxTypeId,
                            Price = d.Price,
                            Cost = d.Cost,
                            IsInventory = d.IsInventory,
                            Particulars = d.Particulars,
                            Address = d.Address,
                            TermId = d.TermId,
                            ContactNumber = d.ContactNumber,
                            ContactPerson = d.ContactPerson,
                            TaxNumber = d.TaxNumber,
                            CreditLimit = d.CreditLimit,
                            DateAcquired = d.DateAcquired.ToShortDateString(),
                            UsefulLife = d.UsefulLife,
                            SalvageValue = d.SalvageValue,
                            ManualArticleOldCode = d.ManualArticleOldCode
                        };

            return items.ToList();
        }

        // ===================================
        // Dropdown List - Item Group (Filter)
        // ===================================
        [Authorize, HttpGet, Route("api/itemList/dropdown/list/itemGroup")]
        public List<Entities.MstArticleGroup> DropdownListItemListItemGroup()
        {
            var itemGroups = from d in db.MstArticleGroups.OrderBy(d => d.ArticleGroup)
                             where d.ArticleTypeId == 1
                             select new Entities.MstArticleGroup
                             {
                                 Id = d.Id,
                                 ArticleGroup = d.ArticleGroup,
                             };

            return itemGroups.ToList();
        }
    }
}