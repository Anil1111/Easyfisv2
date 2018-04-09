using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;

namespace easyfis.CSVIntegratorApiControllers
{
    public class CSVIntegratorTrnSalesInvoiceController : ApiController
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

        // ==================================
        // Add Sales Invoice (CSV Integrator)
        // ==================================
        [HttpPost, Route("api/add/CSVIntegrator/salesInvoice")]
        public HttpResponseMessage AddSalesInvoiceCSVIntegrator(CSVIntegratorEntities.CSVIntegratorTrnSalesInvoice objSalesInvoice)
        {
            try
            {
                Boolean currentBranchExist = false;
                var currentBranch = from d in db.MstBranches where d.BranchCode.Equals(objSalesInvoice.BranchCode) select d;
                if (currentBranch.Any())
                {
                    currentBranchExist = true;
                }

                Boolean customersExist = false;
                var customers = from d in db.MstArticles.OrderBy(d => d.Article) where d.ManualArticleCode.Equals(objSalesInvoice.CustomerManualArticleCode) && d.ArticleTypeId == 2 && d.IsLocked == true select d;
                if (customers.Any())
                {
                    customersExist = true;
                }

                Boolean termsExist = false;
                var terms = from d in db.MstTerms.OrderBy(d => d.Term) where d.Term.Equals(objSalesInvoice.Term) && d.IsLocked == true select d;
                if (terms.Any())
                {
                    termsExist = true;
                }

                Boolean usersExist = false;
                var users = from d in db.MstUsers.OrderBy(d => d.FullName) where d.IsLocked == true select d;
                if (users.Any())
                {
                    usersExist = true;
                }

                var defaultSINumber = "0000000001";
                var lastSalesInvoice = from d in db.TrnSalesInvoices.OrderByDescending(d => d.Id)
                                       where d.MstBranch.BranchCode.Equals(objSalesInvoice.BranchCode)
                                       select d;

                if (lastSalesInvoice.Any())
                {
                    var SINumber = Convert.ToInt32(lastSalesInvoice.FirstOrDefault().SINumber) + 0000000001;
                    defaultSINumber = FillLeadingZeroes(SINumber, 10);
                }

                String errorMessage = "";
                Boolean isValid = false;

                if (currentBranchExist)
                {
                    if (customersExist)
                    {
                        if (termsExist)
                        {
                            if (usersExist)
                            {
                                isValid = true;
                            }
                            else
                            {
                                errorMessage = "No User!";
                            }
                        }
                        else
                        {
                            errorMessage = "No Term!";
                        }
                    }
                    else
                    {
                        errorMessage = "No Customer!";
                    }
                }
                else
                {
                    errorMessage = "No Branch!";
                }

                if (isValid)
                {
                    Data.TrnSalesInvoice newSalesInvoice = new Data.TrnSalesInvoice
                    {
                        BranchId = currentBranch.FirstOrDefault().Id,
                        SINumber = defaultSINumber,
                        SIDate = Convert.ToDateTime(objSalesInvoice.SIDate),
                        DocumentReference = "NA",
                        CustomerId = customers.FirstOrDefault().Id,
                        TermId = terms.FirstOrDefault().Id,
                        Remarks = objSalesInvoice.Remarks,
                        ManualSINumber = objSalesInvoice.ManualSINumber,
                        Amount = 0,
                        PaidAmount = 0,
                        AdjustmentAmount = 0,
                        BalanceAmount = 0,
                        SoldById = users.FirstOrDefault().Id,
                        PreparedById = users.FirstOrDefault().Id,
                        CheckedById = users.FirstOrDefault().Id,
                        ApprovedById = users.FirstOrDefault().Id,
                        IsLocked = false,
                        CreatedById = users.FirstOrDefault().Id,
                        CreatedDateTime = DateTime.Now,
                        UpdatedById = users.FirstOrDefault().Id,
                        UpdatedDateTime = DateTime.Now
                    };

                    db.TrnSalesInvoices.InsertOnSubmit(newSalesInvoice);
                    db.SubmitChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, newSalesInvoice.SINumber);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorMessage);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }
    }
}
