﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class RepInventoryReport
    {
        public Int32 Id { get; set; }
        public String Document { get; set; }
        public Int32 BranchId { get; set; }
        public String Branch { get; set; }
        public Int32 ArticleId { get; set; }
        public String Article { get; set; }
        public String ManualArticleCode { get; set; }
        public String ManualArticleOldCode { get; set; }
        public String InventoryCode { get; set; }
        public Decimal Quantity { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Amount { get; set; }
        public Int32 UnitId { get; set; }
        public String Unit { get; set; }
        public Decimal BegQuantity { get; set; }
        public Decimal InQuantity { get; set; }
        public Decimal OutQuantity { get; set; }
        public Decimal EndQuantity { get; set; }
        public String Category { get; set; }
        public Decimal Price { get; set; }
    }
}