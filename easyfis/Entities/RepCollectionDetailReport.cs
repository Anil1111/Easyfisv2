using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepCollectionDetailReport
    {
        public Int32 ORId { get; set; }
        public String ORBranch { get; set; }
        public String ORNumber { get; set; }
        public String ORDate { get; set; }
        public String Customer { get; set; }
        public String Branch { get; set; }
        public String Account { get; set; }
        public String Article { get; set; }
        public String PayType { get; set; }
        public String SINumber { get; set; }
        public String DepositoryBank { get; set; }
        public String CheckNumber { get; set; }
        public String CheckDate { get; set; }
        public String CheckBank { get; set; }
        public Decimal Amount { get; set; }
    }
}