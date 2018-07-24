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
    public class ApiMstBranchController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // List Branch
        // ===========
        [Authorize, HttpGet, Route("api/branch/list/{companyId}")]
        public List<Entities.MstBranch> ListBranch(String companyId)
        {
            var branchs = from d in db.MstBranches.OrderBy(d => d.BranchCode)
                          where d.CompanyId == Convert.ToInt32(companyId)
                          select new Entities.MstBranch
                          {
                              Id = d.Id,
                              CompanyId = d.CompanyId,
                              BranchCode = d.BranchCode,
                              Branch = d.Branch,
                              Address = d.Address,
                              ContactNumber = d.ContactNumber,
                              TaxNumber = d.TaxNumber
                          };

            return branchs.ToList();
        }

        // ===================
        // Fill Leading Zeroes
        // ===================
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

        // ==========
        // Add Branch
        // ==========
        [Authorize, HttpPost, Route("api/branch/add/{companyId}")]
        public HttpResponseMessage AddBranch(Entities.MstBranch objBranch, String companyId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("CompanyDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var company = from d in db.MstCompanies
                                          where d.Id == Convert.ToInt32(companyId)
                                          select d;

                            if (company.Any())
                            {
                                if (!company.FirstOrDefault().IsLocked)
                                {
                                    var defaultBranchCode = "101";
                                    var lastBranch = from d in db.MstBranches.OrderByDescending(d => d.Id)
                                                     select d;

                                    if (lastBranch.Any())
                                    {
                                        defaultBranchCode = lastBranch.FirstOrDefault().BranchCode;

                                        var lastBranchByCompany = from d in db.MstBranches.OrderByDescending(d => d.Id)
                                                                  where d.CompanyId == Convert.ToInt32(companyId)
                                                                  select d;

                                        if (lastBranchByCompany.Any())
                                        {
                                            defaultBranchCode = lastBranchByCompany.FirstOrDefault().BranchCode;

                                            var CVNumber = Convert.ToInt32(lastBranchByCompany.FirstOrDefault().BranchCode) + 001;
                                            defaultBranchCode = FillLeadingZeroes(CVNumber, 3);
                                        }
                                        else
                                        {
                                            var branchCodeIncrement = Convert.ToInt32(lastBranch.FirstOrDefault().BranchCode) + 100;

                                            var branchCode = Math.Round(Convert.ToDouble(branchCodeIncrement) / 100, 0) * 100;
                                            var CVNumber = branchCode + 001;
                                            defaultBranchCode = FillLeadingZeroes(Convert.ToInt32(CVNumber), 3);
                                        }
                                    }

                                    Data.MstBranch newBranch = new Data.MstBranch
                                    {
                                        CompanyId = Convert.ToInt32(companyId),
                                        BranchCode = defaultBranchCode,
                                        Branch = objBranch.Branch,
                                        Address = objBranch.Address,
                                        ContactNumber = objBranch.ContactNumber,
                                        TaxNumber = objBranch.TaxNumber,
                                        IsLocked = true,
                                        CreatedById = currentUserId,
                                        CreatedDateTime = DateTime.Now,
                                        UpdatedById = currentUserId,
                                        UpdatedDateTime = DateTime.Now
                                    };

                                    db.MstBranches.InsertOnSubmit(newBranch);
                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new branch if the current company detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current company details are not found in the server. Please add new company first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add branch.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this company page.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // =============
        // Update Branch
        // =============
        [Authorize, HttpPut, Route("api/branch/update/{id}/{companyId}")]
        public HttpResponseMessage UpdateBranch(Entities.MstBranch objBranch, String id, String companyId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("CompanyDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var company = from d in db.MstCompanies
                                          where d.Id == Convert.ToInt32(companyId)
                                          select d;

                            if (company.Any())
                            {
                                if (!company.FirstOrDefault().IsLocked)
                                {
                                    var branch = from d in db.MstBranches
                                                 where d.Id == Convert.ToInt32(id)
                                                 select d;

                                    if (branch.Any())
                                    {
                                        var updateBranch = branch.FirstOrDefault();
                                        updateBranch.Branch = objBranch.Branch;
                                        updateBranch.Address = objBranch.Address;
                                        updateBranch.ContactNumber = objBranch.ContactNumber;
                                        updateBranch.TaxNumber = objBranch.TaxNumber;
                                        updateBranch.IsLocked = true;
                                        updateBranch.CreatedById = currentUserId;
                                        updateBranch.CreatedDateTime = DateTime.Now;
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These branch details are not found in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot edit and update branch if the current company detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current company details are not found in the server. Please add new company first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update branch.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this company page.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // =============
        // Delete Branch
        // =============
        [Authorize, HttpDelete, Route("api/branch/delete/{id}/{companyId}")]
        public HttpResponseMessage DeleteBranch(String id, String companyId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("CompanyDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var company = from d in db.MstCompanies
                                          where d.Id == Convert.ToInt32(companyId)
                                          select d;

                            if (company.Any())
                            {
                                if (!company.FirstOrDefault().IsLocked)
                                {
                                    var branch = from d in db.MstBranches
                                                 where d.Id == Convert.ToInt32(id)
                                                 select d;

                                    if (branch.Any())
                                    {
                                        db.MstBranches.DeleteOnSubmit(branch.First());
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These branch details are not found in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot delete branch if the current company detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current company details are not found in the server. Please add new company first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete branch.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this company page.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }
    }
}
