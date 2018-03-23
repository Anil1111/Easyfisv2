﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class TrnStockCount
    {
        public Int32 Id { get; set; }
        public Int32 BranchId { get; set; }
        public String SCNumber { get; set; }
        public String SCDate { get; set; }
        public String Particulars { get; set; }
        public Int32 PreparedById { get; set; }
        public Int32 CheckedById { get; set; }
        public Int32 ApprovedById { get; set; }
        public Boolean IsLocked { get; set; }
        public Int32 CreatedById { get; set; }
        public String CreatedBy { get; set; }
        public String CreatedDateTime { get; set; }
        public Int32 UpdatedById { get; set; }
        public String UpdatedBy { get; set; }
        public String UpdatedDateTime { get; set; }
        public List<TrnStockCountItem> StockCountItem { get; set; }
    }
}