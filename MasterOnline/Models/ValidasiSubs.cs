using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class ValidasiSubs
    {
        public int JumlahPesananBulanIni { get; set; }
        public int? JumlahPesananMax { get; set; }
        public int JumlahMarketplace { get; set; }
        public short? JumlahMarketplaceMax { get; set; }
        public bool SudahSampaiBatasTanggal { get; set; }
        public List<int> ListNamaMarketTerpakai { get; set; } = new List<int>();

        //add by nurul 1/3/2019
        public bool JumlahUserLebih { get; set; }
        //end add by nurul 1/3/2019
    }
}