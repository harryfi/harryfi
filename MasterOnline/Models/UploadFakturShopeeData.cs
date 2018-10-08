using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{

    public class UploadFakturShopeeData
    {
        public UploadFakturShopeeDatas[] data { get; set; }
        public string cust { get; set; }
        public string perso { get; set; }
    }

    public class UploadFakturShopeeDatas
    {
        public string NoPesanan { get; set; }
        public string StatusPesanan { get; set; }
        public string StatusPembatalanPengembalian { get; set; }
        public string NoResi { get; set; }
        public string OpsiPengiriman { get; set; }
        public string Antarkecounterpickup { get; set; }
        public string PesananHarusDikirimkanSebelumMenghindariketerlambatan { get; set; }
        public string WaktuPengirimanDiatur { get; set; }
        public string WaktuPesananDibuat { get; set; }
        public string WaktuPembayaranDilakukan { get; set; }
        public string SKUInduk { get; set; }
        public string NamaProduk { get; set; }
        public string NomorReferensiSKU { get; set; }
        public string NamaVariasi { get; set; }
        public string HargaSebelumDiskon { get; set; }
        public string HargaSetelahDiskon { get; set; }
        public int Jumlah { get; set; }
        public string TotalHargaProduk { get; set; }
        public string TotalDiskon { get; set; }
        public string DiskonDariPenjual { get; set; }
        public string DiskonDariShopee { get; set; }
        public string BeratProduk { get; set; }
        public int JumlahProdukdiPesan { get; set; }
        public string TotalBerat { get; set; }
        public string VoucherDitanggungPenjual { get; set; }
        public string VoucherDitanggungShopee { get; set; }
        public string PaketDiskon { get; set; }
        public string PaketDiskonDiskondariShopee { get; set; }
        public string PaketDiskonDiskondariPenjual { get; set; }
        public int PotonganKoinShopee { get; set; }
        public string DiskonKartuKredit { get; set; }
        public string OngkosKirimDibayarolehPembeli { get; set; }
        public string TotalPembayarandibayarpembeli { get; set; }
        public string PerkiraanOngkosKirim { get; set; }
        public string CatatandariPembeli { get; set; }
        public string Catatan { get; set; }
        public string UsernamePembeli { get; set; }
        public string NamaPenerima { get; set; }
        public string NoTelepon { get; set; }
        public string AlamatPengiriman { get; set; }
        public string KotaKabupaten { get; set; }
        public string Provinsi { get; set; }
        public string WaktuPesananSelesai { get; set; }
    }

}