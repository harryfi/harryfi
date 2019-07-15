using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class BukaLapakExcelFaktur
    {
        public DateTime Tanggal { get; set; }
        public string IDTransaksi { get; set; }
        public string TransaksiDropshipper { get; set; }
        public string NamaDropshipper { get; set; }
        public string DetailDropshipper { get; set; }
        public string Penjual { get; set; }
        public string Pembeli { get; set; }
        public string UsernamePembeli { get; set; }
        public string HPPembeli { get; set; }
        public string AlamatPembeli { get; set; }
        public string KecamatanPembeli { get; set; }
        public string KotaPembeli { get; set; }
        public string PropinsiPembeli { get; set; }
        public string KodePosPembeli { get; set; }
        public string NamaProduk { get; set; }
        public string HargaProduk { get; set; }
        public string BiayaPengiriman { get; set; }
        public string BiayaAsuransi { get; set; }
        public string TotalTerbayar { get; set; }
        public string JumlahProduk { get; set; }
        public string SKU { get; set; }
        public string Varian { get; set; }
        public string Kurir { get; set; }
        public string KodeTracking { get; set; }
        public string Status { get; set; }

    }
}