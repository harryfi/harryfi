using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class BayarPiutangViewModel
    {
        public ART03A Piutang { get; set; }
        public List<ART03A> ListPiutang { get; set; } = new List<ART03A>();
        public ART03B PiutangDetail { get; set; }
        public List<ART03B> ListPiutangDetail { get; set; } = new List<ART03B>();
        public List<SIT01A> ListFaktur { get; set; } = new List<SIT01A>();
        public List<ART01D> ListSisa { get; set; } = new List<ART01D>();
        public List<string> Errors { get; set; } = new List<string>();
        //add by Tri 4 Jan 2019, autoload faktur
        public double bayarPiutang { get; set; }

        //add by nurul 6/4/2020
        public BindUploadExcel ret { get; set; }
        //end add by nurul 6/4/2020
        public bool adaError { get; set; }
    }

    public class BindUploadExcel
    {
        public List<string> Errors { get; set; }
        public List<int> lastRow { get; set; }
        public bool success { get; set; }
        public List<string> cust { get; set; }
        public List<string> namaCust { get; set; }
        public List<string> namaGudang { get; set; }
        public bool nextFile { get; set; }
        public byte[] byteData { get; set; }
        public bool statusLoop { get; set; }
        public bool statusSuccess { get; set; }
        public int progress { get; set; }
        public int percent { get; set; }
        public int countAll { get; set; }
        public string nobuk { get; set; }

        //add by nurul 6/4/2020\
        public double TBAYAR { get; set; }
        public double TPOT { get; set; }
        public double TLEBIHBAYAR { get; set; }
        public string TipeData { get; set; }
        public string buktiLog { get; set; }
        public List<ShopeeExcelBayarPiutang> records { get; set; } = new List<ShopeeExcelBayarPiutang>();
        //public List<LazadaExcelBayarPiutang> recordsLazada { get; set; } = new List<LazadaExcelBayarPiutang>();
        public List<DetailLazada> detailLazada { get; set; } = new List<DetailLazada>();        
        //end add by nurul 6/4/2020
    }

    public class DetailLazada
    {
        public DateTime Tanggal { get; set; }
        public string Ref { get; set; }
        public double? NItemPrice { get; set; }
        public double? NPaymentFee { get; set; }
        public double? NPromotion { get; set; }
        public double? NShipping { get; set; }
        public double? Potongan { get; set; }
        public double? NLostClaim { get; set; }
        public double? NShippingVoucher { get; set; }
        public double? Bayar { get; set; }
    }

    public class errorBayarPiutang
    {
        public LOG_IMPORT_FAKTUR header { get; set; }
        public List<TABLE_LOG_DETAIL> detail { get; set; } = new List<TABLE_LOG_DETAIL>();
    }
}