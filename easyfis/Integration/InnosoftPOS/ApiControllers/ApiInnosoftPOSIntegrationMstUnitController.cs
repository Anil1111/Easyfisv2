using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.InnosoftPOS.ApiControllers
{
    public class ApiInnosoftPOSIntegrationMstUnitController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ====================================
        // List Unit (Innosoft POS Integration)
        // ====================================
        [HttpGet, Route("api/innosoftPOSIntegration/unit/list")]
        public List<Entities.InnosoftPOSIntegrationMstUnit> ListInnosoftPOSIntegrationUnit()
        {
            var units = from d in db.MstUnits.OrderBy(d => d.Unit)
                        select new Entities.InnosoftPOSIntegrationMstUnit
                        {
                            Id = d.Id,
                            Unit = d.Unit
                        };

            return units.ToList();
        }
    }
}
