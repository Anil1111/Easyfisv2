using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.InnosoftPOS.ApiControllers
{
    public class ApiInnosoftPOSIntegrationMstDiscountController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========================================
        // List Discount (Innosoft POS Integration)
        // ========================================
        [HttpGet, Route("api/innosoftPOSIntegration/discount/list")]
        public List<Entities.InnosoftPOSIntegrationMstDiscount> ListInnosoftPOSIntegrationDiscount()
        {
            var discounts = from d in db.MstDiscounts.OrderBy(d => d.Discount)
                            select new Entities.InnosoftPOSIntegrationMstDiscount
                            {
                                Id = d.Id,
                                Discount = d.Discount
                            };

            return discounts.ToList();
        }
    }
}
