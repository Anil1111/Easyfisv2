using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.POSIntegration.ApiControllers
{
    public class ApiPOSIntegrationTrnStockInController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==============================
        // Get Stock In - POS Integration
        // ==============================
        [HttpGet, Route("api/get/POSIntegration/stockIn/{stockInDate}/{branchCode}")]
        public List<Entities.POSIntegrationTrnStockIn> GetStockInPOSIntegration(String stockInDate, String branchCode)
        {
            var stockIns = from d in db.TrnStockIns
                           where d.INDate == Convert.ToDateTime(stockInDate)
                           && d.MstBranch.BranchCode.Equals(branchCode)
                           && d.IsLocked == true
                           select new Entities.POSIntegrationTrnStockIn
                           {
                               BranchCode = d.MstBranch.BranchCode,
                               Branch = d.MstBranch.Branch,
                               INNumber = d.INNumber,
                               INDate = d.INDate.ToShortDateString(),
                               ListPOSIntegrationTrnStockInItem = db.TrnStockInItems.Select(i => new Entities.POSIntegrationTrnStockInItem
                               {
                                   INId = i.INId,
                                   ItemCode = i.MstArticle.ManualArticleCode,
                                   Item = i.MstArticle.Article,
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
