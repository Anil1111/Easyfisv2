using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.POSIntegrationApiControllers
{
    public class ApiPOSIntegrationMstCustomerController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==============================
        // GET Customer - POS Integration
        // ==============================
        [HttpGet, Route("api/get/POSIntegration/customer/{updateDateTime}")]
        public List<POSIntegrationEntities.POSIntegrationMstCustomer> GetCustomerPOSIntegration(String updateDateTime)
        {
            var customers = from d in db.MstArticles
                            where d.ArticleTypeId == 2
                            && d.IsLocked == true
                            && d.UpdatedDateTime.Date == Convert.ToDateTime(updateDateTime)
                            select new POSIntegrationEntities.POSIntegrationMstCustomer
                            {
                                ManualArticleCode = d.ManualArticleCode,
                                Article = d.Article,
                                Address = d.Address,
                                ContactPerson = d.ContactPerson,
                                ContactNumber = d.ContactNumber,
                                Term = d.MstTerm.Term,
                                TaxNumber = d.TaxNumber,
                                CreditLimit = d.CreditLimit
                            };

            return customers.ToList();
        }
    }
}
