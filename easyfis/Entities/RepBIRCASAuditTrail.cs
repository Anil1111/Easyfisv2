using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepBIRCASAuditTrail
    {
        public String TimeStamp { get; set; }
        public String User { get; set; }
        public String Entity { get; set; }
        public String Activity { get; set; }
        public String OldObject { get; set; }
        public String NewObject { get; set; }
    }
}