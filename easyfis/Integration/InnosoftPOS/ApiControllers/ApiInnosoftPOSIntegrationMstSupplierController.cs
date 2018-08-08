using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.InnosoftPOS.ApiControllers
{
    public class ApiInnosoftPOSIntegrationMstSupplierController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========================================
        // List Supplier (Innosoft POS Integration)
        // ========================================
        [HttpGet, Route("api/innosoftPOSIntegration/supplier/list/{updatedDateTime}")]
        public List<Entities.InnosoftPOSIntegrationMstSupplier> ListInnosoftPOSIntegrationSupplier(String updatedDateTime)
        {
            var suppliers = from d in db.MstArticles
                            where d.ArticleTypeId == 3
                            && d.IsLocked == true
                            && d.UpdatedDateTime.Date == Convert.ToDateTime(updatedDateTime)
                            select new Entities.InnosoftPOSIntegrationMstSupplier
                            {
                                Supplier = d.Article,
                                Address = d.Address,
                                ContactNumber = d.ContactNumber,
                                Term = d.MstTerm.Term,
                                TaxNumber = d.TaxNumber
                            };

            return suppliers.ToList();
        }
    }
}