using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models.Api
{
    public class ResultDataPesanan
    {
        public SOT01A Pesanan { get; set; }
        public string MarketName { get; set; }
        public string BuyerName { get; set; }
    }
}