using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class PesananViewModel
    {
        public SOT01A Pesanan { get; set; }
        public SOT01B PesananDetail { get; set; }
        public List<SOT01A> ListPesanan { get; set; } = new List<SOT01A>();
        public List<SOT01B> ListPesananDetail { get; set; } = new List<SOT01B>();
        public List<STF02> ListBarang { get; set; } = new List<STF02>();
        public List<ARF01C> ListPembeli { get; set; } = new List<ARF01C>();
        public List<ARF01> ListPelanggan { get; set; } = new List<ARF01>();
        public List<Marketplace> ListMarketplace { get; set; } = new List<Marketplace>();
        public List<SIT01A> ListFaktur { get; set; } = new List<SIT01A>();
        public List<Ekspedisi> ListEkspedisi { get; set; } = new List<Ekspedisi>();
        public List<Subscription> ListSubs { get; set; } = new List<Subscription>();
        public List<String> Errors { get; set; } = new List<String>();
        //add by nurul 26/9/2018
        public List<STF02H> ListBarangMarket { get; set; } = new List<STF02H>();
        public List<QOH_PER_GD> ListQOHPerGD { get; set; } = new List<QOH_PER_GD>();
        public List<QOO_PER_BRG> ListQOOPerBRG { get; set; } = new List<QOO_PER_BRG>();
        //add by nurul 11/3/2019
        public string setGd { get; set; }
        public string selectRec { get; set; }
        //end add by nurul 11/3/2019
        //add by nurul 10/4/2019
        public SIFSY DataUsaha { get; set; } = new SIFSY();
        public string alamatPenerima { get; set; }
        //end add by nurul 10/4/2019
        //add by Tri 19/9/19
        public int createPackinglist { get; set; }
        //end add by Tri 19/9/19
    }
}