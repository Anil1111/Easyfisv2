using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ModifiedApiControllers
{
    public class ApiRepItemComponentListController : ApiController
    {
        // ============
        // Data Context 
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ================
        // Get Highest Cost
        // ================
        public Decimal GetHighestCost(Int32 articleId)
        {
            var articleInventories = from d in db.MstArticleInventories.OrderByDescending(d => d.Cost)
                                     where d.ArticleId == articleId
                                     select d;

            if (articleInventories.Any())
            {
                return articleInventories.FirstOrDefault().Cost;
            }
            else
            {
                return 0;
            }
        }

        // ===================
        // Item Component List
        // ===================
        [Authorize, HttpGet, Route("api/itemComponentList/list/{itemGroupId}")]
        public List<Entities.MstArticleComponent> ListItemComponentList(String itemGroupId)
        {
            var itemComponents = from d in db.MstArticleComponents
                                 where d.MstArticle.ArticleGroupId == Convert.ToInt32(itemGroupId)
                                 select new Entities.MstArticleComponent
                                 {
                                     Id = d.Id,
                                     ArticleId = d.ArticleId,
                                     ManualArticleCode = d.MstArticle1.ManualArticleCode,
                                     Article = d.MstArticle.Article,
                                     ComponentManualArticleOldCode = d.MstArticle1.ManualArticleOldCode,
                                     ComponentArticleId = d.ComponentArticleId,
                                     ComponentArticle = d.MstArticle1.Article,
                                     Quantity = d.Quantity,
                                     Unit = d.MstArticle1.MstUnit.Unit,
                                     Cost = GetHighestCost(d.ComponentArticleId),
                                     Particulars = d.MstArticle.Particulars,
                                 };

            return itemComponents.ToList();
        }

        // ===================================
        // Dropdown List - Item Group (Filter)
        // ===================================
        [Authorize, HttpGet, Route("api/itemComponentList/dropdown/list/itemGroup")]
        public List<Entities.MstArticleGroup> DropdownListItemComponentListItemGroup()
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
