using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ModifiedApiControllers
{
    public class ApiRepItemListPerSupplierController : ApiController
    {
        // ============
        // Data Context 
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========================
        // List Item List Per Supplier
        // ===========================
        [Authorize, HttpGet, Route("api/itemListPerSupplier/list/{supplierId}")]
        public List<Entities.MstArticle> ListItemListPerSupplier(String supplierId)
        {
            var items = from d in db.MstArticles
                        where d.DefaultSupplierId == Convert.ToInt32(supplierId)
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

        // =================================
        // Dropdown List - Supplier (Filter)
        // =================================
        [Authorize, HttpGet, Route("api/itemListPerSupplier/dropdown/list/supplier")]
        public List<Entities.MstArticle> DropdownListItemListPerSupplierListSupplier()
        {
            var suppliers = from d in db.MstArticles.OrderBy(d => d.Article)
                            where d.ArticleTypeId == 3
                            select new Entities.MstArticle
                            {
                                Id = d.Id,
                                Article = d.Article
                            };

            return suppliers.ToList();
        }
    }
}
