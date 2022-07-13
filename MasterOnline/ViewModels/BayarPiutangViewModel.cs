﻿using System;
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
        public BindUploadExcelBayar ret { get; set; }
        //end add by nurul 6/4/2020
        public bool adaError { get; set; }
        public List<FakturJson> listFakturBelumLunas { get; set; } = new List<FakturJson>();
        public string noCust { get; set; }
        public List<tempOngkirFaktur> ListOngkir = new List<tempOngkirFaktur>();

        public List<TEMP_UPLOAD_EXCEL_BAYAR> listDetailBayar = new List<TEMP_UPLOAD_EXCEL_BAYAR>();
        //ADD BY NURUL 2/10/2020
        public tempHitungHeader tempHitungHeader { get; set; }
        //END ADD BY NURUL 2/10/2020
    }

    public class BindUploadExcelBayar
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
        //public List<ShopeeExcelBayarPiutang> records { get; set; } = new List<ShopeeExcelBayarPiutang>();
        ////public List<LazadaExcelBayarPiutang> recordsLazada { get; set; } = new List<LazadaExcelBayarPiutang>();
        //public List<DetailLazada> detailLazada { get; set; } = new List<DetailLazada>();
        //public List<int> recnum_record { get; set; } = new List<int>();
        public bool sudahSimpanTemp { get; set; }
        //public string cust_id { get; set; }
        //public string cust_perso { get; set; }
        //end add by nurul 6/4/2020
        public bool adaError { get; set; }
        public bool TidakLanjutProses { get; set; }

        public bool statusLoopDownload { get; set; }
        public bool statusSuccessDownload { get; set; }
        public int progressDownload { get; set; }
        public int percentDownload { get; set; }
        public int countAllDownload { get; set; }
        public bool selesaiProsesDownload { get; set; }

        public bool statusLoopTemp { get; set; }
        public bool statusSuccessTemp { get; set; }
        public int progressTemp { get; set; }
        public int percentTemp { get; set; }
        public int countAllTemp { get; set; }
        //public bool selesaiProsesTemp { get; set; }
        public string fileCsvPath { get; set; }
        public List<TEMP_UPLOAD_EXCEL_BAYAR> list_Detail_ret = new List<TEMP_UPLOAD_EXCEL_BAYAR>();
        public int successUpdateDetail { get; set; }
        public int successUpdateHeader { get; set; }

        public int id { get; set; }

        public string noSTokOM { get; set; }
        public string noSTokOK { get; set; }
        public string countAllSukses { get; set; }

        public bool adaErrorQty { get; set; }
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
        public double totalData { get; set; }
        public int totalSuccess { get; set; }
    }

    public class FakturBelumLunasPrompt
    {
        public string nobuk { get; set; }
        public string norefSI { get; set; }
        public DateTime? tglSI { get; set; }
        public string norefSO { get; set; }
        public DateTime? tglSO { get; set; }
        public double sisa { get; set; }
        public double ongkir { get; set; }
        //add by nurul 30/6/2021
        public string pembeli { get; set; }
        //end add by nurul 30/6/2021
    }

    public class tempOngkirFaktur
    {
        public string NOBUK_FAKTUR { get; set; }
        public double ONGKIR { get; set; }
    }

    //ADD BY NURUL 2/10/2020
    public class tempHitungHeader
    {
        public string BUKTI { get; set; }
        public double TotalFaktur { get; set; }
        public double TotalBayar { get; set; }
        public double TotalPotongan { get; set; }
        public double TotalPelunasan { get; set; }
        public double Selisih { get; set; }
        public double TotalLebihBayar { get; set; }
    }
    //END ADD BY NURUL 2/10/2020

    public class DetailBayarPerFaktur
    {
        public List<detail__> bayar = new List<detail__>();
    }
    public class detail__
    {
        public string bukti { get; set; }
        public DateTime? tgl { get; set; }
    }
}