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
using System.Dynamic;
using Newtonsoft.Json;

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

        // =================
        // Get Object String
        // =================
        public String GetObjectString<T>(T obj)
        {
            String json = "";

            var properties = obj.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType == false && p.PropertyType.BaseType == typeof(ValueType));
            if (properties.Any())
            {
                dynamic flexible = new ExpandoObject();
                var dictionary = (IDictionary<string, object>)flexible;

                foreach (PropertyInfo property in properties)
                {
                    dictionary.Add(property.Name, property.GetValue(obj));
                }

                var serialized = JsonConvert.SerializeObject(dictionary);

                json = serialized;
            }

            return json;
        }
    }
}