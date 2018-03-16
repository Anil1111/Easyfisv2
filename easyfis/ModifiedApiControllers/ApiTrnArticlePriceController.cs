using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;

namespace easyfis.ModifiedApiControllers
{
    public class ApiTrnArticlePriceController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===============
        // List Item Price
        // ===============
        [Authorize, HttpGet, Route("api/itemPrice/list/{startDate}/{endDate}")]
        public List<Entities.TrnArticlePrice> ListItemPrice(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var itemPrices = from d in db.TrnArticlePrices.OrderByDescending(d => d.Id)
                             where d.BranchId == branchId
                             && d.IPDate >= Convert.ToDateTime(startDate)
                             && d.IPDate <= Convert.ToDateTime(endDate)
                             select new Entities.TrnArticlePrice
                             {
                                 Id = d.Id,
                                 IPNumber = d.IPNumber,
                                 IPDate = d.IPDate.ToShortDateString(),
                                 ManualIPNumber = d.ManualIPNumber,
                                 Particulars = d.Particulars,
                                 PreparedById = d.PreparedById,
                                 CheckedById = d.CheckedById,
                                 ApprovedById = d.ApprovedById,
                                 IsLocked = d.IsLocked,
                                 CreatedById = d.CreatedById,
                                 CreatedBy = d.MstUser3.FullName,
                                 CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                 UpdatedById = d.UpdatedById,
                                 UpdatedBy = d.MstUser4.FullName,
                                 UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                             };

            return itemPrices.ToList();
        }

    }
}
