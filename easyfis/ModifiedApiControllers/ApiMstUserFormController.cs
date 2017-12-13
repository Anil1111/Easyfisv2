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
    public class ApiMstUserFormController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==============
        // List User Form
        // ==============
        [Authorize, HttpGet, Route("api/userForm/list/{userId}")]
        public List<Entities.MstUserForm> ListUserForm(String userId)
        {
            var userForms = from d in db.MstUserForms
                            where d.UserId == Convert.ToInt32(userId)
                            select new Entities.MstUserForm
                            {
                                Id = d.Id,
                                UserId = d.UserId,
                                FormId = d.FormId,
                                Form = d.SysForm.Particulars,
                                CanAdd = d.CanAdd,
                                CanEdit = d.CanEdit,
                                CanDelete = d.CanDelete,
                                CanLock = d.CanLock,
                                CanUnlock = d.CanUnlock,
                                CanPrint = d.CanPrint
                            };

            return userForms.ToList();
        }

        // =============================
        // Dropdown List - Forms (Field)
        // =============================
        [Authorize, HttpGet, Route("api/userForm/dropdown/list/forms")]
        public List<Entities.SysForm> DropdownListUserFormListForms()
        {
            var forms = from d in db.SysForms.OrderBy(d => d.Particulars)
                        select new Entities.SysForm
                        {
                            Id = d.Id,
                            FormName = d.FormName,
                            Particulars = d.Particulars
                        };

            return forms.ToList();
        }

        // =============
        // Add User Form
        // =============
        [Authorize, HttpPost, Route("api/userForm/add/{userId}")]
        public HttpResponseMessage AddUserForm(Entities.MstUserForm objUserForm, String userId)
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
                                    && d.SysForm.FormName.Equals("UserDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var user = from d in db.MstUsers
                                       where d.Id == Convert.ToInt32(userId)
                                       select d;

                            if (user.Any())
                            {
                                if (!user.FirstOrDefault().IsLocked)
                                {
                                    Data.MstUserForm newUserForm = new Data.MstUserForm
                                    {
                                        UserId = Convert.ToInt32(userId),
                                        FormId = objUserForm.FormId,
                                        CanAdd = objUserForm.CanAdd,
                                        CanEdit = objUserForm.CanEdit,
                                        CanDelete = objUserForm.CanDelete,
                                        CanLock = objUserForm.CanLock,
                                        CanUnlock = objUserForm.CanUnlock,
                                        CanPrint = objUserForm.CanPrint
                                    };

                                    db.MstUserForms.InsertOnSubmit(newUserForm);
                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add new user form if the current user detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current user details are not found in the server. Please add new user first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add user form.");
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

        // ================
        // Update User Form
        // ================
        [Authorize, HttpPut, Route("api/userForm/update/{id}/{userId}")]
        public HttpResponseMessage UpdateUserForm(Entities.MstUserForm objUserForm, String id, String userId)
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
                                    && d.SysForm.FormName.Equals("UserDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var user = from d in db.MstUsers
                                       where d.Id == Convert.ToInt32(userId)
                                       select d;

                            if (user.Any())
                            {
                                if (!user.FirstOrDefault().IsLocked)
                                {
                                    var userForm = from d in db.MstUserForms
                                                   where d.Id == Convert.ToInt32(id)
                                                   select d;

                                    if (userForm.Any())
                                    {
                                        var updateUserForm = userForm.FirstOrDefault();
                                        updateUserForm.FormId = objUserForm.FormId;
                                        updateUserForm.CanAdd = objUserForm.CanAdd;
                                        updateUserForm.CanEdit = objUserForm.CanEdit;
                                        updateUserForm.CanDelete = objUserForm.CanDelete;
                                        updateUserForm.CanLock = objUserForm.CanLock;
                                        updateUserForm.CanUnlock = objUserForm.CanUnlock;
                                        updateUserForm.CanPrint = objUserForm.CanPrint;
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These user form details are not found in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot edit and update user form if the current user detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current user details are not found in the server. Please add new user first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update user form.");
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

        // ================
        // Delete User Form
        // ================
        [Authorize, HttpDelete, Route("api/userForm/delete/{id}/{userId}")]
        public HttpResponseMessage DeleteUserForm(String id, String userId)
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
                                    && d.SysForm.FormName.Equals("UserDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var user = from d in db.MstUsers
                                       where d.Id == Convert.ToInt32(userId)
                                       select d;

                            if (user.Any())
                            {
                                if (!user.FirstOrDefault().IsLocked)
                                {
                                    var userForm = from d in db.MstUserForms
                                                   where d.Id == Convert.ToInt32(id)
                                                   select d;

                                    if (userForm.Any())
                                    {
                                        db.MstUserForms.DeleteOnSubmit(userForm.First());
                                        db.SubmitChanges();

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These user form details are not found in the server.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot delete user form if the current user detail is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "These current user details are not found in the server. Please add new user first before proceeding.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete user form.");
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
