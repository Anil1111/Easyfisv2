using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.InnosoftPOS.ApiControllers
{
    public class ApiInnosoftPOSIntegrationTrnStockTransferController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==============================================
        // List Stock Transfer (Innosoft POS Integration)
        // ==============================================
        [HttpGet, Route("api/innosoftPOSIntegration/stockTransfer/list/{toBranchCode}/{STDate}")]
        public List<Entities.InnosoftPOSIntegrationTrnStockTransfer> ListInnosoftPOSIntegrationStockTransfer(String toBranchCode, String STDate)
        {
            var stockTransfers = from d in db.TrnStockTransfers
                                 where d.MstBranch1.BranchCode.Equals(toBranchCode)
                                 && d.STDate == Convert.ToDateTime(STDate)
                                 && d.IsLocked == true
                                 select new Entities.InnosoftPOSIntegrationTrnStockTransfer
                                 {
                                     ToBranchCode = d.MstBranch1.BranchCode,
                                     ToBranch = d.MstBranch1.Branch,
                                     STNumber = d.STNumber,
                                     STDate = d.STDate.ToShortDateString(),
                                     ManualSTNumber = d.ManualSTNumber,
                                     ListStockTransferItem = db.TrnStockTransferItems.Select(i => new Entities.InnosoftPOSIntegrationTrnStockTransferItem
                                     {
                                         STId = i.STId,
                                         ManualItemCode = i.MstArticle.ManualArticleCode,
                                         ItemDescription = i.MstArticle.Article,
                                         Unit = i.MstUnit1.Unit,
                                         Quantity = i.Quantity,
                                         Cost = i.Cost,
                                         Amount = i.Amount
                                     }).Where(i => i.STId == d.Id).ToList(),
                                 };

            return stockTransfers.ToList();
        }
    }
}