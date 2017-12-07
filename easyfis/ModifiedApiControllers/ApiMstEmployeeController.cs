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
    public class ApiMstEmployeeController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // =============
        // List Employee
        // =============
        [Authorize, HttpGet, Route("api/employee/list")]
        public List<Entities.MstArticle> ListEmployee()
        {
            var employees = from d in db.MstArticles.OrderByDescending(d => d.ArticleCode)
                            where d.ArticleTypeId == 4
                            select new Entities.MstArticle
                            {
                                Id = d.Id,
                                ArticleCode = d.ArticleCode,
                                Article = d.Article,
                                Address = d.Address,
                                ContactNumber = d.ContactNumber,
                                ContactPerson = d.ContactPerson,
                                ArticleGroupId = d.ArticleGroupId,
                                ArticleGroup = d.MstArticleGroup.ArticleGroup,
                                AccountId = d.AccountId,
                                IsLocked = d.IsLocked,
                                CreatedBy = d.MstUser.FullName,
                                CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                UpdatedBy = d.MstUser1.FullName,
                                UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                            };

            return employees.ToList();
        }

        // ======================================
        // Dropdown List - Employee Group (Field)
        // ======================================
        [Authorize, HttpGet, Route("api/employee/dropdown/list/employeeGroup")]
        public List<Entities.MstArticleGroup> DropdownListEmployeeGroup()
        {
            var employeeGroups = from d in db.MstArticleGroups.OrderBy(d => d.ArticleGroup)
                                 where d.ArticleTypeId == 4
                                 && d.IsLocked == true
                                 select new Entities.MstArticleGroup
                                 {
                                     Id = d.Id,
                                     ArticleGroup = d.ArticleGroup,
                                     AccountId = d.AccountId,
                                     SalesAccountId = d.SalesAccountId,
                                     CostAccountId = d.CostAccountId,
                                     AssetAccountId = d.AssetAccountId,
                                     ExpenseAccountId = d.ExpenseAccountId
                                 };

            return employeeGroups.ToList();
        }

        // ==============================================
        // Dropdown List - Employee Group Account (Field)
        // ==============================================
        [Authorize, HttpGet, Route("api/employee/dropdown/list/employeeGroup/account")]
        public List<Entities.MstAccount> DropdownListEmployeeGroupAccount()
        {
            var employeeGroupAccounts = from d in db.MstAccounts.OrderBy(d => d.Account)
                                        where d.IsLocked == true
                                        select new Entities.MstAccount
                                        {
                                            Id = d.Id,
                                            AccountCode = d.AccountCode,
                                            Account = d.Account
                                        };

            return employeeGroupAccounts.ToList();
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

        // ============
        // Add Employee
        // ============
        [Authorize, HttpPost, Route("api/employee/add")]
        public HttpResponseMessage AddEmployee(Entities.MstArticle objEmployee)
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
                            var defaultEmployeeCode = "0000000001";
                            var lastEmployee = from d in db.MstArticles.OrderByDescending(d => d.Id)
                                               where d.ArticleTypeId == 4
                                               select d;

                            if (lastEmployee.Any())
                            {
                                var employeeCode = Convert.ToInt32(lastEmployee.FirstOrDefault().ArticleCode) + 0000000001;
                                defaultEmployeeCode = FillLeadingZeroes(employeeCode, 10);
                            }

                            var articleGroups = from d in db.MstArticleGroups
                                                where d.Id == objEmployee.ArticleGroupId
                                                && d.ArticleTypeId == 4
                                                select d;

                            if (articleGroups.Any())
                            {
                                var units = from d in db.MstUnits
                                            select d;

                                if (units.Any())
                                {
                                    var taxTypes = from d in db.MstTaxTypes
                                                   select d;

                                    if (taxTypes.Any())
                                    {
                                        var terms = from d in db.MstTerms
                                                    select d;

                                        if (terms.Any())
                                        {
                                            Data.MstArticle newEmployee = new Data.MstArticle
                                            {
                                                ArticleCode = defaultEmployeeCode,
                                                ManualArticleCode = "NA",
                                                Article = objEmployee.Article,
                                                Category = "NA",
                                                ArticleTypeId = 4,
                                                ArticleGroupId = articleGroups.FirstOrDefault().Id,
                                                AccountId = articleGroups.FirstOrDefault().AccountId,
                                                SalesAccountId = articleGroups.FirstOrDefault().SalesAccountId,
                                                CostAccountId = articleGroups.FirstOrDefault().CostAccountId,
                                                AssetAccountId = articleGroups.FirstOrDefault().AssetAccountId,
                                                ExpenseAccountId = articleGroups.FirstOrDefault().ExpenseAccountId,
                                                UnitId = units.FirstOrDefault().Id,
                                                OutputTaxId = db.MstTaxTypes.FirstOrDefault().Id,
                                                InputTaxId = db.MstTaxTypes.FirstOrDefault().Id,
                                                WTaxTypeId = db.MstTaxTypes.FirstOrDefault().Id,
                                                Price = 0,
                                                Cost = 0,
                                                IsInventory = false,
                                                Particulars = "NA",
                                                Address = objEmployee.Address,
                                                TermId = terms.FirstOrDefault().Id,
                                                ContactNumber = objEmployee.ContactNumber,
                                                ContactPerson = objEmployee.ContactPerson,
                                                EmailAddress = "NA",
                                                TaxNumber = "NA",
                                                CreditLimit = 0,
                                                DateAcquired = DateTime.Now,
                                                UsefulLife = 0,
                                                SalvageValue = 0,
                                                ManualArticleOldCode = "NA",
                                                Kitting = 0,
                                                IsLocked = true,
                                                CreatedById = currentUserId,
                                                CreatedDateTime = DateTime.Now,
                                                UpdatedById = currentUserId,
                                                UpdatedDateTime = DateTime.Now
                                            };

                                            db.MstArticles.InsertOnSubmit(newEmployee);
                                            db.SubmitChanges();

                                            return Request.CreateResponse(HttpStatusCode.OK, newEmployee.Id);
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.NotFound, "No term found. Please setup more terms for all master tables.");
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.NotFound, "No tax type found. Please setup more tax types for all master tables.");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.NotFound, "No unit found. Please setup more units for all master tables.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No article group found. Please setup at least one article group for employees.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add employee.");
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

        // ===============
        // Update Employee
        // ===============
        [Authorize, HttpPut, Route("api/employee/update/{id}")]
        public HttpResponseMessage UpdateEmployee(Entities.MstArticle objEmployee, String id)
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
                            var employee = from d in db.MstArticles
                                           where d.Id == Convert.ToInt32(id)
                                           && d.ArticleTypeId == 4
                                           select d;

                            if (employee.Any())
                            {
                                var lockEmployee = employee.FirstOrDefault();
                                lockEmployee.Article = objEmployee.Article;
                                lockEmployee.ArticleGroupId = objEmployee.ArticleGroupId;
                                lockEmployee.AccountId = objEmployee.AccountId;
                                lockEmployee.SalesAccountId = objEmployee.SalesAccountId;
                                lockEmployee.CostAccountId = objEmployee.CostAccountId;
                                lockEmployee.AssetAccountId = objEmployee.AssetAccountId;
                                lockEmployee.ExpenseAccountId = objEmployee.ExpenseAccountId;
                                lockEmployee.Address = objEmployee.Address;
                                lockEmployee.ContactNumber = objEmployee.ContactNumber;
                                lockEmployee.ContactPerson = objEmployee.ContactPerson;
                                lockEmployee.IsLocked = true;
                                lockEmployee.UpdatedById = currentUserId;
                                lockEmployee.UpdatedDateTime = DateTime.Now;

                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These employee details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to edit and update employee.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this system tables page.");
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

        // ===============
        // Delete Employee
        // ===============
        [Authorize, HttpDelete, Route("api/employee/delete/{id}")]
        public HttpResponseMessage DeleteEmployee(String id)
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
                            var employee = from d in db.MstArticles
                                           where d.Id == Convert.ToInt32(id)
                                           && d.ArticleTypeId == 4
                                           select d;

                            if (employee.Any())
                            {
                                db.MstArticles.DeleteOnSubmit(employee.First());
                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected employee is not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete employee.");
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
