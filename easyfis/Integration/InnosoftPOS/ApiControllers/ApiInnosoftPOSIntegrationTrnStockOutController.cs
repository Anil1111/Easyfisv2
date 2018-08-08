using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.InnosoftPOS.ApiControllers
{
    public class ApiInnosoftPOSIntegrationTrnStockOutController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =========================================
        // List Stock Out (Innosoft POS Integration)
        // =========================================
        [HttpGet, Route("api/innosoftPOSIntegration/stockOut/list/{branchCode}/{OTDate}")]
        public List<Entities.InnosoftPOSIntegrationTrnStockOut> ListInnosoftPOSIntegrationStockOut(String branchCode, String OTDate)
        {
            var stockOuts = from d in db.TrnStockOuts
                            where d.MstBranch.BranchCode.Equals(branchCode)
                            && d.OTDate == Convert.ToDateTime(OTDate)
                            && d.IsLocked == true
                            select new Entities.InnosoftPOSIntegrationTrnStockOut
                            {
                                BranchCode = d.MstBranch.BranchCode,
                                Branch = d.MstBranch.Branch,
                                OTNumber = d.OTNumber,
                                OTDate = d.OTDate.ToShortDateString(),
                                ManualOTNumber = d.ManualOTNumber,
                                ListStockOutItem = db.TrnStockOutItems.Select(i => new Entities.InnosoftPOSIntegrationTrnStockOutItem
                                {
                                    OTId = i.OTId,
                                    ManualItemCode = i.MstArticle.ManualArticleCode,
                                    ItemDescription = i.MstArticle.Article,
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