using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models.Api
{
    public class ResultDataInvoice
    {
        public PBT01A Invoice { get; set; }
        public PBT01A ReturInvoice { get; set; }
        public string Supplier { get; set; }
    }
}