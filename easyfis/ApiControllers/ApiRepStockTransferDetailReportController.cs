using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepStockTransferDetailReportController : ApiController
    {
        // ============
        // Data Context 
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =================================
        // Stock Transfer Detail Report List
        // =================================
        [Authorize, HttpGet, Route("api/stockTransferDetailReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepStockTransferDetailReport> ListStockTransferDetailReport(String startDate, String endDate, String companyId, String branchId)
        {
            var stockTransferItems = from d in db.TrnStockTransferItems
                                     where d.TrnStockTransfer.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                     && d.TrnStockTransfer.BranchId == Convert.ToInt32(branchId)
                                     && d.TrnStockTransfer.STDate >= Convert.ToDateTime(startDate)
                                     && d.TrnStockTransfer.STDate <= Convert.ToDateTime(endDate)
                                     && d.TrnStockTransfer.IsLocked == true
                                     select new Entities.RepStockTransferDetailReport
                                     {
                                         Id = d.Id,
                                         STId = d.STId,
                                         ST = d.TrnStockTransfer.STNumber,
                                         STDate = d.TrnStockTransfer.STDate.ToShortDateString(),
                                         ToBranch = d.TrnStockTransfer.MstBranch1.Branch,
                                         ItemId = d.ItemId,
                                         ItemCode = d.MstArticle.ManualArticleCode,
                                         ItemManualArticleOldCode = d.MstArticle.ManualArticleOldCode,
                                         Item = d.MstArticle.Article,
                                         ItemInventoryId = d.ItemInventoryId,
                                         ItemInventory = d.MstArticleInventory.InventoryCode,
                                         Particulars = d.Particulars,
                                         UnitId = d.UnitId,
                                         Unit = d.MstUnit.Unit,
                                         Quantity = d.Quantity,
                                         Cost = d.Cost,
                                         Amount = d.Amount,
                                         BaseUnitId = d.BaseUnitId,
                                         BaseUnit = d.MstUnit1.Unit,
                                         BaseQuantity = d.BaseQuantity,
                                         BaseCost = d.BaseCost,
                                     };

            return stockTransferItems.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/stockTransferDetailReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListStockTransferDetailReportListCompany()
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
        [Authorize, HttpGet, Route("api/stockTransferDetailReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListStockTransferDetailReportListBranch(String companyId)
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
