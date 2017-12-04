using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class TrnCollectionLine
    {
        public Int32 Id { get; set; }
        public Int32 ORId { get; set; }
        public String ORNumber { get; set; }
        public String ORDate { get; set; }
        public String Customer { get; set; }
        public Int32 BranchId { get; set; }
        public String Branch { get; set; }
        public Int32 AccountId { get; set; }
        public String Account { get; set; }
        public Int32 ArticleId { get; set; }
        public String Article { get; set; }
        public Int32? SIId { get; set; }
        public String SINumber { get; set; }
        public String Particulars { get; set; }
        public Decimal Amount { get; set; }
        public Int32 PayTypeId { get; set; }
        public String PayType { get; set; }
        public String CheckNumber { get; set; }
        public String CheckDate { get; set; }
        public String CheckBank { get; set; }
        public Int32? DepositoryBankId { get; set; }
        public String DepositoryBank { get; set; }
        public Boolean IsClear { get; set; }
    }
}