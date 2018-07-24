﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class MstArticleComponent
    {
        public Int32 Id { get; set; }
        public Int32 ArticleId { get; set; }
        public String ManualArticleCode { get; set; }
        public String Article { get; set; }
        public Int32 ComponentArticleId { get; set; }
        public String ComponentArticleManualCode { get; set; }
        public String ComponentArticle { get; set; }
        public String ComponentManualArticleOldCode { get; set; }
        public Decimal Quantity { get; set; }
        public String Unit { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Amount { get; set; }
        public String Particulars { get; set; }
    }
}