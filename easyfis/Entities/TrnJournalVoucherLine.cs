using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class TrnJournalVoucherLine
    {
        public Int32 Id { get; set; }
        public Int32 JVId { get; set; }
        public String JVNumber { get; set; }
        public String JVDate { get; set; }
        public Int32 BranchId { get; set; }
        public String Branch { get; set; }
        public Int32 AccountId { get; set; }
        public String Account { get; set; }
        public Int32 ArticleId { get; set; }
        public String Article { get; set; }
        public String Particulars { get; set; }
        public Decimal DebitAmount { get; set; }
        public Decimal CreditAmount { get; set; }
        public Decimal BalanceAmount { get; set; }
        public Int32? APRRId { get; set; }
        public String APRRNumber { get; set; }
        public Int32? ARSIId { get; set; }
        public String ARSINumber { get; set; }
        public Boolean IsClear { get; set; }
    }
}