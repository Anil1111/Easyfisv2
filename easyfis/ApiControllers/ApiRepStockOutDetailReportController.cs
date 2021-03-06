﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepStockOutDetailReportController : ApiController
    {
        // ============
        // Data Context 
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ============================
        // Stock Out Detail Report List
        // ============================
        [Authorize, HttpGet, Route("api/stockOutDetailReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepStockOutDetailReport> ListStockOutDetailReport(String startDate, String endDate, String companyId, String branchId)
        {
            var stockOutItems = from d in db.TrnStockOutItems
                                where d.TrnStockOut.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                && d.TrnStockOut.BranchId == Convert.ToInt32(branchId)
                                && d.TrnStockOut.OTDate >= Convert.ToDateTime(startDate)
                                && d.TrnStockOut.OTDate <= Convert.ToDateTime(endDate)
                                && d.TrnStockOut.IsLocked == true
                                select new Entities.RepStockOutDetailReport
                                {
                                    Id = d.Id,
                                    OTId = d.OTId,
                                    OT = d.TrnStockOut.OTNumber,
                                    OTDate = d.TrnStockOut.OTDate.ToShortDateString(),
                                    ExpenseAccountId = d.ExpenseAccountId,
                                    ExpenseAccount = d.MstAccount.Account,
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
                                    BaseCost = d.BaseCost
                                };

            return stockOutItems.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/stockOutDetailReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListStockOutDetailReportListCompany()
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
        [Authorize, HttpGet, Route("api/stockOutDetailReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListStockOutDetailReportListBranch(String companyId)
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
