using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.InnosoftPOS.ApiControllers
{
    public class ApiInnosoftPOSIntegrationMstTermController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ====================================
        // List Term (Innosoft POS Integration)
        // ====================================
        [HttpGet, Route("api/innosoftPOSIntegration/term/list")]
        public List<Entities.InnosoftPOSIntegrationMstTerm> ListInnosoftPOSIntegrationTerm()
        {
            var terms = from d in db.MstTerms.OrderBy(d => d.Term)
                        select new Entities.InnosoftPOSIntegrationMstTerm
                        {
                            Id = d.Id,
                            Term = d.Term
                        };

            return terms.ToList();
        }
    }
}
