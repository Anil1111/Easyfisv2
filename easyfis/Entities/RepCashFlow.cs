using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepCashFlow
    {
        public String AccountCashFlowCode { get; set; }
        public String AccountCashFlow { get; set; }
        public String SubCategoryDescription { get; set; }
        public String AccountTypeCode { get; set; }
        public String AccountType { get; set; }
        public Int32 AccountId { get; set; }
        public String AccountCode { get; set; }
        public String Account { get; set; }
        public Decimal DebitAmount { get; set; }
        public Decimal CreditAmount { get; set; }
        public Decimal Balance { get; set; }
    }
}