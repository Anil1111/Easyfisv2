using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepCollectionDetailReportController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =============================
        // Collection Detail Report List
        // =============================
        [Authorize, HttpGet, Route("api/collectionDetailReport/list/{startDate}/{endDate}/{companyId}/{branchId}")]
        public List<Entities.RepCollectionDetailReport> ListCollectionDetailReport(String startDate, String endDate, String companyId, String branchId)
        {
            var collectionLines = from d in db.TrnCollectionLines
                                  where d.TrnCollection.BranchId == Convert.ToInt32(branchId)
                                  && d.TrnCollection.MstBranch.CompanyId == Convert.ToInt32(companyId)
                                  && d.TrnCollection.ORDate >= Convert.ToDateTime(startDate)
                                  && d.TrnCollection.ORDate <= Convert.ToDateTime(endDate)
                                  && d.TrnCollection.IsLocked == true
                                  select new Entities.RepCollectionDetailReport
                                  {
                                      ORId = d.ORId,
                                      ORBranch = d.TrnCollection.MstBranch.Branch,
                                      ORNumber = d.TrnCollection.ORNumber,
                                      ORDate = d.TrnCollection.ORDate.ToShortDateString(),
                                      Customer = d.TrnCollection.MstArticle.Article,
                                      Branch = d.MstBranch.Branch,
                                      Account = d.MstAccount.Account,
                                      Article = d.MstArticle.Article,
                                      PayType = d.MstPayType.PayType,
                                      SINumber = d.SIId != null ? d.TrnSalesInvoice.SINumber : "",
                                      DepositoryBank = d.MstArticle1.Article,
                                      CheckNumber = d.CheckNumber,
                                      CheckDate = d.CheckDate.ToShortDateString(),
                                      CheckBank = d.CheckBank,
                                      Amount = d.Amount,
                                  };

            return collectionLines.ToList();
        }

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/collectionDetailReport/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListCollectionDetailReportListCompany()
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
        [Authorize, HttpGet, Route("api/collectionDetailReport/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListCollectionDetailReportBranch(String companyId)
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

