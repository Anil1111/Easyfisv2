using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;
using System.Reflection;

namespace easyfis.ModifiedApiControllers
{
    public class ApiMstTermController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===========
        // Audit Trail
        // ===========
        private Business.AuditTrail at = new Business.AuditTrail();

        // =========
        // List Term
        // =========
        [Authorize, HttpGet, Route("api/term/list")]
        public List<Entities.MstTerm> ListTerm()
        {
            var terms = from d in db.MstTerms.OrderBy(d => d.Term)
                        select new Entities.MstTerm
                        {
                            Id = d.Id,
                            Term = d.Term,
                            NumberOfDays = d.NumberOfDays,
                            IsLocked = d.IsLocked,
                            CreatedById = d.CreatedById,
                            CreatedBy = d.MstUser.FullName,
                            CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                            UpdatedById = d.UpdatedById,
                            UpdatedBy = d.MstUser1.FullName,
                            UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                        };

            return terms.ToList();
        }

        // ========
        // Add Term
        // ========
        [Authorize, HttpPost, Route("api/term/add")]
        public HttpResponseMessage AddTerm(Entities.MstTerm objTerm)
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
                                    && d.SysForm.FormName.Equals("SystemTables")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            Data.MstTerm newTerm = new Data.MstTerm
                            {
                                Term = objTerm.Term,
                                NumberOfDays = objTerm.NumberOfDays,
                                IsLocked = true,
                                CreatedById = currentUserId,
                                CreatedDateTime = DateTime.Now,
                                UpdatedById = currentUserId,
                                UpdatedDateTime = DateTime.Now
                            };

                            db.MstTerms.InsertOnSubmit(newTerm);
                            db.SubmitChanges();

                            String newObject = at.GetObjectString(newTerm);
                            at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, "NA", newObject);

                            return Request.CreateResponse(HttpStatusCode.OK, newTerm.Id);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add term.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this system table page.");
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

        // ===========
        // Update Term
        // ===========
        [Authorize, HttpPut, Route("api/term/update/{id}")]
        public HttpResponseMessage UpdateTerm(Entities.MstTerm objTerm, String id)
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
                                    && d.SysForm.FormName.Equals("SystemTables")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanEdit)
                        {
                            var term = from d in db.MstTerms
                                       where d.Id == Convert.ToInt32(id)
                                       select d;

                            if (term.Any())
                            {
                                String oldObject = at.GetObjectString(term.FirstOrDefault());

                                var updateTerm = term.FirstOrDefault();
                                updateTerm.Term = objTerm.Term;
                                updateTerm.NumberOfDays = objTerm.NumberOfDays;
                                updateTerm.IsLocked = true;
                                updateTerm.UpdatedById = currentUserId;
                                updateTerm.UpdatedDateTime = DateTime.Now;

                                db.SubmitChanges();

                                String newObject = at.GetObjectString(term.FirstOrDefault());
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, newObject);

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These term details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update term.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this system table page.");
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

        // ===========
        // Delete Term
        // ===========
        [Authorize, HttpDelete, Route("api/term/delete/{id}")]
        public HttpResponseMessage DeleteTerm(String id)
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
                                    && d.SysForm.FormName.Equals("SystemTables")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var term = from d in db.MstTerms
                                       where d.Id == Convert.ToInt32(id)
                                       select d;

                            if (term.Any())
                            {
                                db.MstTerms.DeleteOnSubmit(term.First());

                                String oldObject = at.GetObjectString(term.FirstOrDefault());
                                at.InsertAuditTrail(currentUser.FirstOrDefault().Id, GetType().Name, MethodBase.GetCurrentMethod().Name, oldObject, "NA");

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected term is not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete term.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this system table page.");
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
