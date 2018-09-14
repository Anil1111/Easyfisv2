using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;

namespace easyfis.Business
{
    public class AuditTrail
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==================
        // Insert Audit Trail
        // ==================
        public void InsertAuditTrail(Entities.SysAuditTrail objAuditTrail)
        {
            try
            {
                Data.SysAuditTrail newAuditTrail = new Data.SysAuditTrail
                {
                    AuditDate = DateTime.Now,
                    UserId = objAuditTrail.UserId,
                    Entity = objAuditTrail.Entity,
                    Activity = objAuditTrail.Activity,
                    OldObject = objAuditTrail.OldObject,
                    NewObject = objAuditTrail.NewObject
                };

                db.SysAuditTrails.InsertOnSubmit(newAuditTrail);
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}