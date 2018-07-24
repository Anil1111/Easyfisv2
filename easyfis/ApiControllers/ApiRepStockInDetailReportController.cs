using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepStockInDetailReportController : ApiController
    {
        // ============
        // Data Context 
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========================
        // Stock In Detail Report List
        // ===========================
        [Authorize, HttpGet, Route("api/stockInDetailReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepStockInDetailReport> ListStockInDetailReport(String startDate, String endDate, String companyId, String branchId)
        {
            var stockInItems = from d in db.TrnStockInItems
                               where d.TrnStockIn.MstBranch.CompanyId == Convert.ToInt32(companyId)
                               && d.TrnStockIn.BranchId == Convert.ToInt32(branchId)
                               && d.TrnStockIn.INDate >= Convert.ToDateTime(startDate)
                               && d.TrnStockIn.INDate <= Convert.ToDateTime(endDate)
                               && d.TrnStockIn.IsLocked == true
                               select new Entities.RepStockInDetailReport
                               {
                                   Id = d.Id,
                                   INId = d.INId,
                                   IN = d.TrnStockIn.INNumber,
                                   INDate = d.TrnStockIn.INDate.ToShortDateString(),
                                   ItemId = d.ItemId,
                                   ItemCode = d.MstArticle.ManualArticleCode,
                                   ItemManualArticleOldCode = d.MstArticle.ManualArticleOldCode,
                                   Item = d.MstArticle.Article,
                                   Particulars = d.Particulars,
                                   UnitId = d.UnitId,
                                   Unit = d.MstUnit.Unit,
                                   Quantity = d.Quantity,
                                   Cost = d.Cost,
                                   Amount = d.Amount,
                                   BaseUnitId = d.BaseUnitId,
                                   BaseUnit = d.MstUnit1.Unit,
                                   BaseQuantity = d.BaseQuantity,
                                   BaseCost = d.BaseCost
                               };

            return stockInItems.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/stockInDetailReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListStockInDetailReportListCompany()
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
        [Authorize, HttpGet, Route("api/stockInDetailReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListStockInDetailReportListBranch(String companyId)
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
    }
}
