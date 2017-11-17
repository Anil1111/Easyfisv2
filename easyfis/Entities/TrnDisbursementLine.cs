﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class TrnDisbursementLine
    {
        public Int32 Id { get; set; }
        public Int32 CVId { get; set; }
        public Int32 BranchId { get; set; }
        public String Branch { get; set; }
        public Int32 AccountId { get; set; }
        public String Account { get; set; }
        public Int32 ArticleId { get; set; }
        public String Article { get; set; }
        public Int32? RRId { get; set; }
        public String RRNumber { get; set; }
        public String Particulars { get; set; }
        public Decimal Amount { get; set; }
    }
}