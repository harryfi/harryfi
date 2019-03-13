using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class FakturViewModel
    {
        public string NamaToko { get; set; }
        public string LogoMarket { get; set; }
        public string NamaPerusahaan { get; set; }
        public SIT01A Faktur { get; set; }
        public List<SIT01A> ListFaktur { get; set; } = new List<SIT01A>();
        public SIT01B FakturDetail { get; set; }
        public List<SIT01B> ListFakturDetail { get; set; } = new List<SIT01B>();
        public List<STF02> ListBarang { get; set; } = new List<STF02>();
        public List<ARF01C> ListPembeli { get; set; } = new List<ARF01C>();
        public List<ARF01> ListPelanggan { get; set; } = new List<ARF01>();
        public List<Marketplace> ListMarketplace { get; set; } = new List<Marketplace>();
        public List<ART03B> ListNFaktur { get; set; } = new List<ART03B>();
        public List<SOT01A> ListPesanan { get; set; } = new List<SOT01A>();
        public List<String> Errors { get; set; } = new List<string>();
        public List<Subscription> ListSubs { get; set; } = new List<Subscription>();
        public List<LOG_IMPORT_FAKTUR> ListImportFaktur { get; set; } = new List<LOG_IMPORT_FAKTUR>();
        //add by nurul 29/11/2018 (modiv cetak faktur)
        public string AlamatToko { get; set; }
        public string TlpToko { get; set; }
        public string noRef { get; set; }
        //add by nurul 28/1/2019
        public string Kurir { get; set; }
        public string Marketplace { get; set; }
        public string LogoKurir { get; set; }
        public string NoResi { get; set; }
        //add by nurul 8/3/2019
        public List<QOH_PER_GD> ListQOHPerGD { get; set; } = new List<QOH_PER_GD>();
        public string setGd { get; set; }
        //end add by nurul 8/3/2019
    }
}