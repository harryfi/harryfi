using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models.Api
{
    public class ResultDataHargaJualBarang
    {
        public STF02H Barang { get; set; }
        public string NamaBarang { get; set; }
        public string AkunMarket { get; set; }
        public double? HargaTerakhir { get; set; }
    }
}