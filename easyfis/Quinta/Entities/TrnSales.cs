using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis.Quinta.Entities
{

    // ===========
    // Model: Root
    // ===========
    public class RootObject
    {
        public List<TrnSales> TRN { get; set; }
    }

    public class TrnSales
    {
        public String FTN { get; set; }     // Folio Transaction #
        public String TDT { get; set; }     // Transaction Date
        public String SAI { get; set; }     // Item Code
        public String SAM { get; set; }     // Item Description
        public String ACI { get; set; }     // Customer Code
        public String ACC { get; set; }     // Customer Name
        public Decimal NAM { get; set; }    // Net Amount
    }
}