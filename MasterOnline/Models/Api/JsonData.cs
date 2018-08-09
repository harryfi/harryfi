using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models.Api
{
    public class JsonData
    {
        public string UserId { get; set; }
        public string SelDate { get; set; }
        public string StatusTransaksi { get; set; }
        public int? RecNumPesanan { get; set; }
    }
}