using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.ModifiedApiControllers
{
    public class ApiRepDisbursementCheckInformationController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==============================
        // Disbursement Check Information
        // ==============================
        [HttpGet, Route("api/disbursementCheckInformation/list/{CVNumber}/{BranchCode}")]
        public Entities.RepDisbursementSummaryReport ListDisbursementDetailReport(String CVNumber, String BranchCode)
        {
            var disbursements = from d in db.TrnDisbursements
                                where d.CVNumber.Equals(CVNumber)
                                && d.MstBranch.BranchCode.Equals(BranchCode)
                                && d.IsLocked == true
                                select new Entities.RepDisbursementSummaryReport
                                {
                                    CVNumber = d.CVNumber,
                                    CVDate = d.CVDate.ToShortDateString(),
                                    ManualCVNumber = d.ManualCVNumber,
                                    Payee = d.Payee,
                                    Particulars = d.Particulars,
                                    Bank = d.MstArticle1.Article,
                                    CheckNumber = d.CheckNumber,
                                    CheckDate = d.CheckDate.ToShortDateString(),
                                    Amount = d.Amount
                                };

            return disbursements.FirstOrDefault();
        }
    }
}
