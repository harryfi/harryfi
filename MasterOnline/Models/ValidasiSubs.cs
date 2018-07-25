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
    }
}