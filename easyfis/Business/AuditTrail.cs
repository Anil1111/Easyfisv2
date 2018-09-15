using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;
using System.Reflection;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Core.EntityClient;

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

        // ==============
        // Get Old Object
        // ==============
        public String GetOldObjectString<T>(T obj)
        {
            var properties = obj.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType == false && p.PropertyType.BaseType == typeof(ValueType));
            if (properties.Any())
            {
                foreach (PropertyInfo property in properties)
                {
                    Debug.WriteLine(property.PropertyType.BaseType);
                    Debug.WriteLine(property.Name + " : " + property.GetValue(obj));
                }
            }

            return "";
        }
    }
}