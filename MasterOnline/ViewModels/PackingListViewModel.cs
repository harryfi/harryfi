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

        //add by fauzi 22/01/2021, tambah dashboard packing list
        public int? JumlahPackingList { get; set; }
        public int? JumlahPesanan { get; set; }
        public int? JumlahRekapBarang { get; set; }
        public int? JumlahRekapQtyBarang { get; set; }
        //end add by fauzi 22/01/2021, tambah dashboard packing list
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

        //add by nurul 22/7/2020
        public string NO_REFERENSI { get; set; }
        public string STATUS_PRINT { get; set; }
        //public double? BARCODE { get; set; }
        //end add by nurul 22/7/2020
        public bool? BARCODE { get; set; }

    }

    public class RekapBarang
    {
        public string NO_PESANAN { get; set; }
        public string BRG { get; set; }
        public string NAMA_BARANG { get; set; }
        public string PEMBELI { get; set; }
        public string MARKETPLACE { get; set; }
        public int QTY { get; set; }

        //add by nurul 22/7/2020
        public string BARCODE { get; set; }
        public string NO_REFERENSI { get; set; }
        //end add by nurul 22/7/2020

        //add by nurul 17/9/2020
        public string BRG_MULTISKU { get; set; }
        public string NAMA_BRG_MULTISKU { get; set; }
        //end add by nurul 17/9/2020

        //ADD BY TRI 6 OKT 2020
        public string RAK { get; set; }
        //END ADD BY TRI 6 OKT 2020
    }

    public class templistDetailPacking
    {
        public string nobuk { get; set; }
        public List<SOT03BDetailPacking> listDetail = new List<SOT03BDetailPacking>();
    }
}