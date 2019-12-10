using MasterOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class PackingListViewModel
    {
        public List<SOT03A> listParent { get; set; }
        public SOT03A packingList { get; set; }
        public SOT03B detailPackingList { get; set; }
        public List<SOT03BDetailPacking> listDetailPacking { get; set; }
        public List<SOT03B> listPesanan { get; set; }
        public List<RekapBarang> listRekapBarang { get; set; }
        public List<string> Errors { get; set; }
        public string printMode { get; set; }
    }

    public class SOT03BDetailPacking
    {
        public string NO_BUKTI { get; set; }
        
        public string NO_PESANAN { get; set; }
        
        public string SO_TRACKING_NUMBER { get; set; }

        public string SO_STATUS_KIRIM { get; set; }

        public DateTime? TGL_PESANAN { get; set; }

        public string PEMBELI { get; set; }

        public string MARKETPLACE { get; set; }

        public string USERNAME { get; set; }

        public DateTime? TGL_INPUT { get; set; }

        public int? RecNum { get; set; }
    }

    public class RekapBarang
    {
        public string NO_PESANAN { get; set; }
        public string BRG { get; set; }
        public string NAMA_BARANG { get; set; }
        public string PEMBELI { get; set; }
        public string MARKETPLACE { get; set; }
        public int QTY { get; set; }
    }
}