using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;

namespace easyfis.ApiControllers
{
    public class ApiRepTopSellingItemsReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =============================
        // Top Selling Items Report List
        // =============================
        [Authorize, HttpGet, Route("api/topSellingItemsReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Models.TrnSalesInvoiceItem> ListTopSellingItemsReport(String startDate, String endDate, String companyId, String branchId)
        {
            var salesInvoiceItem = from d in db.TrnSalesInvoiceItems
                                   where d.TrnSalesInvoice.BranchId == Convert.ToInt32(branchId)
                                   && d.TrnSalesInvoice.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                   && d.TrnSalesInvoice.SIDate >= Convert.ToDateTime(startDate)
                                   && d.TrnSalesInvoice.SIDate <= Convert.ToDateTime(endDate)
                                   && d.TrnSalesInvoice.IsLocked == true
                                   group d by new
                                   {
                                       Branch = d.TrnSalesInvoice.MstBranch.Branch,
                                       ItemId = d.ItemId,
                                       Item = d.MstArticle.Article,
                                       Unit = d.MstUnit.Unit,
                                       Price = d.Price
                                   } into g
                                   select new Models.TrnSalesInvoiceItem
                                   {
                                       Branch = g.Key.Branch,
                                       ItemId = g.Key.ItemId,
                                       Item = g.Key.Item,
                                       Unit = g.Key.Unit,
                                       Quantity = g.Sum(d => d.Quantity),
                                       Price = g.Key.Price,
                                       Amount = g.Sum(d => d.Amount)
                                   };

            return salesInvoiceItem.OrderByDescending(q => q.Quantity).ToList();
        }
    }
}
