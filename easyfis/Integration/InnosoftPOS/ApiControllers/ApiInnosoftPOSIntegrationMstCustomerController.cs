using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis.Integration.InnosoftPOS.ApiControllers
{
    public class ApiInnosoftPOSIntegrationMstCustomerController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ========================================
        // List Customer (Innosoft POS Integration)
        // ========================================
        [HttpGet, Route("api/innosoftPOSIntegration/customer/list/{updatedDateTime}")]
        public List<Entities.InnosoftPOSIntegrationMstCustomer> ListInnosoftPOSIntegrationCustomer(String updatedDateTime)
        {
            var customers = from d in db.MstArticles
                            where d.ArticleTypeId == 2
                            && d.IsLocked == true
                            && d.UpdatedDateTime.Date == Convert.ToDateTime(updatedDateTime)
                            select new Entities.InnosoftPOSIntegrationMstCustomer
                            {
                                ManualCustomerCode = d.ManualArticleCode,
                                Customer = d.Article,
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