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
    public class ApiMstUserBranchController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ================
        // List User Branch
        // ================
        [Authorize, HttpGet, Route("api/userBranch/list/{userId}")]
        public List<Entities.MstUserBranch> ListUserBranch(String userId)
        {
            var userBranches = from d in db.MstUserBranches
                               where d.UserId == Convert.ToInt32(userId)
                               select new Entities.MstUserBranch
                               {
                                   Id = d.Id,
                                   UserId = d.UserId,
                                   CompanyId = d.MstBranch.CompanyId,
                                   Company = d.MstBranch.MstCompany.Company,
                                   BranchId = d.BranchId,
                                   Branch = d.MstBranch.Branch
                               };

            return userBranches.ToList();
        }

        // ===============================
        // Dropdown List - Company (Field)
        // ===============================
        [Authorize, HttpGet, Route("api/userBranch/dropdown/list/company")]
        public List<Entities.MstCompany> DropdownListUserBranchListCompany()
        {
            var companies = from d in db.MstCompanies.OrderBy(d => d.Company)
                            select new Entities.MstCompany
                            {
                                Id = d.Id,
                                Company = d.Company
                            };

            return companies.ToList();
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/userBranch/dropdown/list/branch/{companyId}")]
        public List<Entities.MstBranch> DropdownListUserBranchBranch(String companyId)
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // ===============
        // Add User Branch
        // ===============
        [Authorize, HttpPost, Route("api/userBranch/add/{userId}")]
        public HttpResponseMessage AddUserBranch(Entities.MstUserBranch objUserBranch, String userId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userBranches = from d in db.MstUserForms
                                       where d.UserId == currentUserId
                                       && d.SysForm.FormName.Equals("UsersDetail")
                                       select d;

                    if (userBranches.Any())
                    {
                        if (userBranches.FirstOrDefault().CanAdd)
                        {
                            var user = from d in db.MstUsers
                                       where d.Id == Convert.ToInt32(userId)
                                       select d;

                            if (user.Any())
                            {
                                if (!user.FirstOrDefault().IsLocked)
                                {
                                    Data.MstUserBranch newUserBranch = new Data.MstUserBranch
                                    {
                                        UserId = Convert.ToInt32(userId),
                                        BranchId = objUserBranch.BranchId
                                    };

                                    db.MstUserBranches.InsertOnSubmit(newUserBranch);
                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new user branch if the current user detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current user details are not found in the server. Please add new user first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add user branch.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this user page.");
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

        // ==================
        // Update User Branch
        // ==================
        [Authorize, HttpPut, Route("api/userBranch/update/{id}/{userId}")]
        public HttpResponseMessage UpdateUserBranch(Entities.MstUserBranch objUserBranch, String id, String userId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userBranches = from d in db.MstUserForms
                                       where d.UserId == currentUserId
                                       && d.SysForm.FormName.Equals("UsersDetail")
                                       select d;

                    if (userBranches.Any())
                    {
                        if (userBranches.FirstOrDefault().CanEdit)
                        {
                            var user = from d in db.MstUsers
                                       where d.Id == Convert.ToInt32(userId)
                                       select d;

                            if (user.Any())
                            {
                                if (!user.FirstOrDefault().IsLocked)
                                {
                                    var userBranch = from d in db.MstUserBranches
                                                     where d.Id == Convert.ToInt32(id)
                                                     select d;

                                    if (userBranch.Any())
                                    {
                                        var updateUserBranch = userBranch.FirstOrDefault();
                                        updateUserBranch.BranchId = objUserBranch.BranchId;
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These user branch details are not found in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot edit and update user branch if the current user detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current user details are not found in the server. Please add new user first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update user branch.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this user page.");
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

        // ==================
        // Delete User Branch
        // ==================
        [Authorize, HttpDelete, Route("api/userBranch/delete/{id}/{userId}")]
        public HttpResponseMessage DeleteUserBranch(String id, String userId)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userBranches = from d in db.MstUserForms
                                       where d.UserId == currentUserId
                                       && d.SysForm.FormName.Equals("UsersDetail")
                                       select d;

                    if (userBranches.Any())
                    {
                        if (userBranches.FirstOrDefault().CanDelete)
                        {
                            var user = from d in db.MstUsers
                                       where d.Id == Convert.ToInt32(userId)
                                       select d;

                            if (user.Any())
                            {
                                if (!user.FirstOrDefault().IsLocked)
                                {
                                    var userBranch = from d in db.MstUserBranches
                                                     where d.Id == Convert.ToInt32(id)
                                                     select d;

                                    if (userBranch.Any())
                                    {
                                        db.MstUserBranches.DeleteOnSubmit(userBranch.First());
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These user branch details are not found in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot delete user branch if the current user detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current user details are not found in the server. Please add new user first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete user branch.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this user page.");
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
