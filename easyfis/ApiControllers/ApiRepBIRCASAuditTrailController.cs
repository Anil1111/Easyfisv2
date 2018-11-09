using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ApiControllers
{
    public class ApiRepBIRCASAuditTrailController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ================================
        // Dropdown List - Company (Filter)
        // ================================
        [Authorize, HttpGet, Route("api/BIRCASAuditTrail/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListBIRCASAuditTrailListCompany()
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
        [Authorize, HttpGet, Route("api/BIRCASAuditTrail/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListBIRCASAuditTrailListBranch(String companyId)
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

        // =====================
        // List Audit Trail Data
        // =====================
        [Authorize, HttpGet, Route("api/BIRCASAuditTrail/list/{startDate}/{endDate}")]
        public List<Entities.RepBIRCASAuditTrail> ListBIRCASAuditTrail(String startDate, String endDate)
        {
            var auditTrails = from d in db.SysAuditTrails
                              where d.AuditDate >= Convert.ToDateTime(startDate)
                              && d.AuditDate <= Convert.ToDateTime(endDate).AddHours(24)
                              select new Entities.RepBIRCASAuditTrail
                              {
                                  TimeStamp = d.AuditDate.ToString("MM-dd-yyyy hh:mm:ss tt", CultureInfo.InvariantCulture),
                                  User = d.MstUser.FullName,
                                  Entity = d.Entity,
                                  Activity = d.Activity,
                                  OldObject = d.OldObject,
                                  NewObject = d.NewObject
                              };

            return auditTrails.ToList();
        }
    }
}
