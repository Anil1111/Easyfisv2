using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.InnosoftPOS.ApiControllers
{
    public class ApiInnosoftPOSIntegrationMstTaxController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===================================
        // List Tax (Innosoft POS Integration)
        // ===================================
        [HttpGet, Route("api/innosoftPOSIntegration/tax/list")]
        public List<Entities.InnosoftPOSIntegrationMstTax> ListInnosoftPOSIntegrationTax()
        {
            var taxes = from d in db.MstTaxTypes.OrderBy(d => d.TaxType)
                        select new Entities.InnosoftPOSIntegrationMstTax
                        {
                            Id = d.Id,
                            Tax = d.TaxType
                        };

            return taxes.ToList();
        }
    }
}
