using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepAccountsReceivable
    {
        public Int32 SIId { get; set; }
        public String Branch { get; set; }
        public String Account { get; set; }
        public String SINumber { get; set; }
        public String SIDate { get; set; }
        public String Customer { get; set; }
        public String DocumentReference { get; set; }
        public Decimal BalanceAmount { get; set; }
        public String DueDate { get; set; }
        public Decimal CurrentAmount { get; set; }
        public Decimal Age30Amount { get; set; }
        public Decimal Age60Amount { get; set; }
        public Decimal Age90Amount { get; set; }
        public Decimal Age120Amount { get; set; }
    }
}