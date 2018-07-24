using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Diagnostics;

namespace easyfis.Integration.JMPoultry.ApiControllers
{
    public class JMPoultryIntegrationTrnStockInController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ============================
        // Zero Fill - Document Numbers
        // ============================
        public String FillLeadingZeroes(Int32 number, Int32 length)
        {
            var result = number.ToString();
            var pad = length - result.Length;
            while (pad > 0)
            {
                result = '0' + result;
                pad--;
            }

            return result;
        }

        // =============================
        // Add Stock In (CSV Integrator)
        // =============================
        [HttpPost, Route("api/add/CSVIntegrator/stockIn")]
        public HttpResponseMessage AddStockInCSVIntegrator(List<Entities.JMPoultryIntegrationTrnStockIn> objStockIns)
        {
            try
            {
                if (objStockIns.Any())
                {
                    foreach (var objStockIn in objStockIns)
                    {
                        Boolean currentBranchExist = false;
                        var currentBranch = from d in db.MstBranches where d.BranchCode.Equals(objStockIn.BranchCode) select d;
                        if (currentBranch.Any())
                        {
                            currentBranchExist = true;
                        }

                        Boolean accountsExist = false;
                        var accounts = from d in db.MstAccounts where d.IsLocked == true select d;

                        List<Int32> listArticleIds = new List<Int32>();
                        if (accounts.Any())
                        {
                            accountsExist = true;
                            var accountArticleTypes = from d in db.MstAccountArticleTypes where d.AccountId == accounts.FirstOrDefault().Id && d.MstAccount.IsLocked == true select d;
                            if (accountArticleTypes.Any())
                            {
                                foreach (var accountArticleType in accountArticleTypes)
                                {
                                    var articles = from d in db.MstArticles where d.ArticleTypeId == accountArticleType.ArticleTypeId && d.IsLocked == true select d;
                                    if (articles.Any())
                                    {
                                        foreach (var article in articles)
                                        {
                                            listArticleIds.Add(article.Id);
                                        }
                                    }
                                }
                            }
                        }

                        Boolean articleListsExist = false;
                        var articleLists = from d in listArticleIds.ToList() select d;
                        if (articleLists.Any())
                        {
                            articleListsExist = true;
                        }

                        Boolean usersExist = false;
                        var users = from d in db.MstUsers.OrderBy(d => d.FullName) where d.IsLocked == true select d;
                        if (users.Any())
                        {
                            usersExist = true;
                        }

                        Boolean isValid = false;
                        if (currentBranchExist)
                        {
                            if (accountsExist)
                            {
                                if (articleListsExist)
                                {
                                    if (usersExist)
                                    {
                                        isValid = true;
                                    }
                                }
                            }
                        }

                        if (isValid)
                        {
                            var defaultINNumber = "0000000001";
                            var lastStockIn = from d in db.TrnStockIns.OrderByDescending(d => d.Id)
                                              where d.MstBranch.BranchCode.Equals(objStockIn.BranchCode)
                                              select d;

                            if (lastStockIn.Any())
                            {
                                var INNumber = Convert.ToInt32(lastStockIn.FirstOrDefault().INNumber) + 0000000001;
                                defaultINNumber = FillLeadingZeroes(INNumber, 10);
                            }

                            Data.TrnStockIn newStockIn = new Data.TrnStockIn
                            {
                                BranchId = currentBranch.FirstOrDefault().Id,
                                INNumber = defaultINNumber,
                                INDate = Convert.ToDateTime(objStockIn.INDate),
                                AccountId = accounts.FirstOrDefault().Id,
                                ArticleId = listArticleIds.FirstOrDefault(),
                                Particulars = objStockIn.Particulars,
                                ManualINNumber = objStockIn.ManualINNumber,
                                IsProduced = objStockIn.IsProduced,
                                PreparedById = users.FirstOrDefault().Id,
                                CheckedById = users.FirstOrDefault().Id,
                                ApprovedById = users.FirstOrDefault().Id,
                                IsLocked = false,
                                CreatedById = users.FirstOrDefault().Id,
                                CreatedDateTime = DateTime.Now,
                                UpdatedById = users.FirstOrDefault().Id,
                                UpdatedDateTime = DateTime.Now
                            };

                            db.TrnStockIns.InsertOnSubmit(newStockIn);
                            db.SubmitChanges();
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, "Sent Successful!");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Empty!");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server! " + e.Message);
            }
        }
    }
}