using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models.Api
{
    public class ResultDataFaktur
    {
        public SIT01A Faktur { get; set; }
        public SIT01A ReturFaktur { get; set; }
        public string MarketName { get; set; }
        public string BuyerName { get; set; }
        public DateTime TglJatuhTempo { get; set; }
    }
}