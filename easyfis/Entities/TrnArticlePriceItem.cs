using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Entities
{
    public class TrnArticlePriceItem
    {
        public Int32 Id { get; set; }
        public Int32 ArticlePriceId { get; set; }
        public Int32 ItemId { get; set; }
        public String ItemDescription { get; set; }
        public Decimal Price { get; set; }
        public Decimal TriggerQuantity { get; set; }
    }
}