using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepAccountsPayable
    {
        public Int32 Id { get; set; }
        public String Branch { get; set; }
        public Int32 AccountId { get; set; }
        public String AccountCode { get; set; }
        public String Account { get; set; }
        public Int32 SupplierId { get; set; }
        public String Supplier { get; set; }
        public String RRNumber { get; set; }
        public String RRDate { get; set; }
        public String DocumentReference { get; set; }
        public Decimal BalanceAmount { get; set; }
        public String DueDate { get; set; }
        public Decimal NumberOfDaysFromDueDate { get; set; }
        public Decimal CurrentAmount { get; set; }
        public Decimal Age30Amount { get; set; }
        public Decimal Age60Amount { get; set; }
        public Decimal Age90Amount { get; set; }
        public Decimal Age120Amount { get; set; }
    }
}