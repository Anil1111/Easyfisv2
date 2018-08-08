using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.InnosoftPOS.ApiControllers
{
    public class ApiInnosoftPOSIntegrationTrnStockInController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========================================
        // List Stock In (Innosoft POS Integration)
        // ========================================
        [HttpGet, Route("api/innosoftPOSIntegration/stockIn/list/{branchCode}/{INDate}")]
        public List<Entities.InnosoftPOSIntegrationTrnStockIn> ListInnosoftPOSIntegrationStockIn(String branchCode, String INDate)
        {
            var stockIns = from d in db.TrnStockIns
                           where d.MstBranch.BranchCode.Equals(branchCode)
                           && d.INDate == Convert.ToDateTime(INDate)
                           && d.IsLocked == true
                           select new Entities.InnosoftPOSIntegrationTrnStockIn
                           {
                               BranchCode = d.MstBranch.BranchCode,
                               Branch = d.MstBranch.Branch,
                               INNumber = d.INNumber,
                               INDate = d.INDate.ToShortDateString(),
                               ManualINNumber = d.ManualINNumber,
                               ListStockInItem = db.TrnStockInItems.Select(i => new Entities.InnosoftPOSIntegrationTrnStockInItem
                               {
                                   INId = i.INId,
                                   ManualItemCode = i.MstArticle.ManualArticleCode,
                                   ItemDescription = i.MstArticle.Article,
                                   Unit = i.MstUnit1.Unit,
                                   Quantity = i.Quantity,
                                   Cost = i.Cost,
                                   Amount = i.Amount
                               }).Where(i => i.INId == d.Id).ToList(),
                           };

            return stockIns.ToList();
        }
    }
}