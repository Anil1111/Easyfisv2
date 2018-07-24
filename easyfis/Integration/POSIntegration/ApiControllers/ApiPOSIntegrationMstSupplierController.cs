using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.POSIntegration.ApiControllers
{
    public class ApiPOSIntegrationMstSupplierController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==============================
        // GET Supplier - POS Integration
        // ==============================
        [HttpGet, Route("api/get/POSIntegration/supplier/{updateDateTime}")]
        public List<Entities.POSIntegrationMstSupplier> GetSupplierPOSIntegration(String updateDateTime)
        {
            var suppliers = from d in db.MstArticles
                            where d.ArticleTypeId == 3
                            && d.IsLocked == true
                            && d.UpdatedDateTime.Date == Convert.ToDateTime(updateDateTime)
                            select new Entities.POSIntegrationMstSupplier
                            {
                                Article = d.Article,
                                Address = d.Address,
                                ContactNumber = d.ContactNumber,
                                Term = d.MstTerm.Term,
                                TaxNumber = d.TaxNumber
                            };

            return suppliers.ToList();
        }
    }
}
