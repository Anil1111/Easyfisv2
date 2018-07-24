using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.POSIntegration.ApiControllers
{
    public class ApiPOSIntegrationTrnStockOutController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===============================
        // Get Stock Out - POS Integration
        // ===============================
        [HttpGet, Route("api/get/POSIntegration/stockOut/{stockOutDate}/{branchCode}")]
        public List<Entities.POSIntegrationTrnStockOut> GetStockInPOSIntegration(String stockOutDate, String branchCode)
        {
            var stockOuts = from d in db.TrnStockOuts
                            where d.OTDate == Convert.ToDateTime(stockOutDate)
                            && d.MstBranch.BranchCode.Equals(branchCode)
                            && d.IsLocked == true
                            select new Entities.POSIntegrationTrnStockOut
                            {
                                BranchCode = d.MstBranch.BranchCode,
                                Branch = d.MstBranch.Branch,
                                OTNumber = d.OTNumber,
                                OTDate = d.OTDate.ToShortDateString(),
                                ListPOSIntegrationTrnStockOutItem = db.TrnStockOutItems.Select(i => new Entities.POSIntegrationTrnStockOutItem
                                {
                                    OTId = i.OTId,
                                    ItemCode = i.MstArticle.ManualArticleCode,
                                    Item = i.MstArticle.Article,
                                    Unit = i.MstUnit1.Unit,
                                    Quantity = i.Quantity,
                                    Cost = i.Cost,
                                    Amount = i.Amount
                                }).Where(i => i.OTId == d.Id).ToList(),
                            };

            return stockOuts.ToList();
        }
    }
}
