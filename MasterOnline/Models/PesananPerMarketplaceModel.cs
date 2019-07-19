using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class PesananPerMarketplaceModel
    {
        public string NamaMarket { get; set; }
        public string JumlahPesananHariIni { get; set; }
        public string NilaiPesananHariIni { get; set; }
        public string JumlahPesananBulanIni { get; set; }
        public string NilaiPesananBulanIni { get; set; }
    }

    public class FakturPerMarketplaceModel
    {
        public string NamaMarket { get; set; }
        public string JumlahFaktur { get; set; }
        public string NilaiFaktur { get; set; }
    }
}