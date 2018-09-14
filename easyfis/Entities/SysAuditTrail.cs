using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class SysAuditTrail
    {
        public Int32 Id { get; set; }
        public DateTime AuditDate { get; set; }
        public Int32 UserId { get; set; }
        public String Entity { get; set; }
        public String Activity { get; set; }
        public String OldObject { get; set; }
        public String NewObject { get; set; }
    }
}