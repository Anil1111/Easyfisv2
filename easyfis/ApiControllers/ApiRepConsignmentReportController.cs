using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ModifiedApiControllers
{
    public class ApiRepConsignmentReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =======================
        // Consignment Report List
        // =======================
        [Authorize, HttpGet, Route("api/consignmentReport/list/{startDate}/{endDate}/{companyId}/{branchId}/{supplierId}")]
        public List<Entities.RepConsignmentReport> ListConsignmentReport(String startDate, String endDate, String companyId, String branchId, String supplierId)
        {
            var salesInvoiceItems = from d in db.TrnSalesInvoiceItems
                                    where d.MstArticle.DefaultSupplierId == Convert.ToInt32(supplierId)
                                    && d.TrnSalesInvoice.SIDate >= Convert.ToDateTime(startDate)
                                    && d.TrnSalesInvoice.SIDate <= Convert.ToDateTime(endDate)
                                    && d.TrnSalesInvoice.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                    && d.TrnSalesInvoice.BranchId == Convert.ToInt32(branchId)
                                    select new
                                    {
                                        ItemManualArticleOldCode = d.MstArticle.ManualArticleOldCode,
                                        ItemCode = d.MstArticle.ManualArticleCode,
                                        ItemDescription = d.MstArticle.Article,
                                        Unit = d.MstArticle.MstUnit.Unit,
                                        Quantity = d.Quantity,
                                        Amount = ((d.NetPrice * (d.MstArticle.ConsignmentCostPercentage / 100)) + d.MstArticle.ConsignmentCostValue) * d.Quantity
                                    };

            if (salesInvoiceItems.Any())
            {
                var consignmentItems = from d in salesInvoiceItems
                                       group d by new
                                       {
                                           ItemManualArticleOldCode = d.ItemManualArticleOldCode,
                                           ItemCode = d.ItemCode,
                                           ItemDescription = d.ItemDescription,
                                           Unit = d.Unit
                                       } into g
                                       select new Entities.RepConsignmentReport
                                       {
                                           ItemManualArticleOldCode = g.Key.ItemManualArticleOldCode,
                                           ItemCode = g.Key.ItemCode,
                                           ItemDescription = g.Key.ItemDescription,
                                           Unit = g.Key.Unit,
                                           Quantity = g.Sum(d => d.Quantity),
                                           Cost = g.Sum(d => d.Amount) / g.Sum(d => d.Quantity),
                                           Amount = g.Sum(d => d.Amount)
                                       };

                return consignmentItems.ToList();
            }
            else
            {
                return new List<Entities.RepConsignmentReport>();
            }
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/consignmentReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListConsignmentReportListCompany()
        {
            var companies = from d in db.MstCompanies.OrderBy(d => d.Company)
                            select new Entities.MstCompany
                            {
                                Id = d.Id,
                                Company = d.Company
                            };

            return companies.ToList();
        }

        // ===============================
        // Dropdown List - Branch (Filter)
        // ===============================
        [Authorize, HttpGet, Route("api/consignmentReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListConsignmentReportListBranch(String companyId)
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           where d.CompanyId == Convert.ToInt32(companyId)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // =================================
        // Dropdown List - Supplier (Filter)
        // =================================
        [Authorize, HttpGet, Route("api/consignmentReport/dropdown/list/supplier")]
        public List<Entities.MstArticle> DropdownListConsignmentReportListSupplier()
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
