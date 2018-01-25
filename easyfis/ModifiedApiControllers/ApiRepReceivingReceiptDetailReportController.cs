using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepReceivingReceiptDetailReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ====================================
        // Receiving Receipt Detail Report List
        // ====================================
        [Authorize, HttpGet, Route("api/receivingReceiptDetailReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepReceivingReceiptDetailReport> ListReceivingReceiptDetailReport(String startDate, String endDate, String companyId, String branchId)
        {
            var receivingReceiptItems = from d in db.TrnReceivingReceiptItems
                                        where d.TrnReceivingReceipt.RRDate >= Convert.ToDateTime(startDate)
                                        && d.TrnReceivingReceipt.RRDate <= Convert.ToDateTime(endDate)
                                        && d.TrnReceivingReceipt.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                        && d.TrnReceivingReceipt.BranchId == Convert.ToInt32(branchId)
                                        && d.TrnReceivingReceipt.IsLocked == true
                                        select new Entities.RepReceivingReceiptDetailReport
                                        {
                                            RRId = d.RRId,
                                            Branch = d.MstBranch.Branch,
                                            RRDate = d.TrnReceivingReceipt.RRDate.ToShortDateString(),
                                            RRNumber = d.TrnReceivingReceipt.RRNumber,
                                            Supplier = d.TrnReceivingReceipt.MstArticle.Article,
                                            Remarks = d.TrnReceivingReceipt.Remarks,
                                            PONumber = d.TrnPurchaseOrder.PONumber,
                                            ItemCode = d.MstArticle.ManualArticleCode,
                                            ItemDescription = d.MstArticle.Article,
                                            Unit = d.MstUnit.Unit,
                                            Quantity = d.Quantity,
                                            Cost = d.Cost,
                                            Amount = d.Amount,
                                            VAT = d.VATAmount,
                                            WTAX = d.WTAXAmount
                                        };

            return receivingReceiptItems.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/receivingReceiptDetailReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListReceivingReceiptDetailReportListCompany()
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
        [Authorize, HttpGet, Route("api/receivingReceiptDetailReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListReceivingReceiptDetailReportBranch(String companyId)
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

