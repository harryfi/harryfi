using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class PengirimanViewModel
    {
        public SIT04A Pengiriman { get; set; }
        //public List<Subscription> ListSubs { get; set; } = new List<Subscription>();
        public List<SIT04A> ListPengiriman { get; set; } = new List<SIT04A>();
        public SIT04B PengirimanDetail { get; set; }
        public List<SIT04B> ListPengirimanDetail { get; set; } = new List<SIT04B>();
        //public List<STF02> ListBarang { get; set; } = new List<STF02>();
        //public List<ARF01C> ListPembeli { get; set; } = new List<ARF01C>();
        //public List<ARF01> ListPelanggan { get; set; } = new List<ARF01>();
        //public List<Marketplace> ListMarketplace { get; set; } = new List<Marketplace>();
        //public List<APT03B> ListNInvoice { get; set; } = new List<APT03B>();
        public List<String> Errors { get; set; } = new List<string>();

        public string NamaToko { get; set; }
        public string NamaPerusahaan { get; set; }
        public string AlamatToko { get; set; }
        public string TlpToko { get; set; }
        public string NamaKurir { get; set; }

        //ADD FOR AUTOLOAD 
        public string DRTGL { get; set; }
        public string SDTGL { get; set; }
        public string DRCUST { get; set; }
        public string SDCUST { get; set; }  
        public string USERNAME { get; set; }
        
    }
}