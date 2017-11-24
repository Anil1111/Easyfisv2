﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepSalesDetailReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========================
        // Sales Detail Report List
        // ========================
        [Authorize, HttpGet, Route("api/salesDetailReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Models.TrnSalesInvoiceItem> ListSalesDetailReport(String startDate, String endDate, String companyId, String branchId)
        {
            var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                    where d.TrnSalesInvoice.BranchId == Convert.ToInt32(branchId)
                                    && d.TrnSalesInvoice.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                    && d.TrnSalesInvoice.SIDate >= Convert.ToDateTime(startDate)
                                    && d.TrnSalesInvoice.SIDate <= Convert.ToDateTime(endDate)
                                    && d.TrnSalesInvoice.IsLocked == true
                                    select new Models.TrnSalesInvoiceItem
                                    {
                                        Id = d.Id,
                                        SIId = d.SIId,
                                        Branch = d.TrnSalesInvoice.MstBranch.Branch,
                                        SI = d.TrnSalesInvoice.SINumber,
                                        SIDate = d.TrnSalesInvoice.SIDate.ToShortDateString(),
                                        Item = d.MstArticle.Article,
                                        ItemInventory = d.MstArticleInventory.InventoryCode,
                                        Unit = d.MstUnit.Unit,
                                        Quantity = d.Quantity,
                                        Amount = d.Amount,
                                        Price = d.Price,
                                        Customer = d.TrnSalesInvoice.MstArticle.Article,
                                        ItemCategory = d.MstArticle.Category
                                    };

            return salesInvoiceItems.ToList();
        }
    }
}
