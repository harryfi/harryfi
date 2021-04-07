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

        //add by nurul 16/9/2019, tambah cek shipment dr list pesanan sit04b
        public List<ShipmentOfKirim> Shipment { get; set; } = new List<ShipmentOfKirim>();
        //end add by nurul 16/9/2019

        public List<cetakSerahTerima> CetakSerahTerima = new List<cetakSerahTerima>();
    }

    public class ShipmentOfKirim
    {
        public string SHIPMENT { get; set; }
        public string NO_BUKTI { get; set; }
    }

    //add by nurul 29/3/2021
    public class SerahTerimaViewModel
    {
        public SIT04A Pengiriman { get; set; }
        public List<SIT04A> ListPengiriman { get; set; } = new List<SIT04A>();
        public SIT04B PengirimanDetail { get; set; }
        public List<SIT04B> ListPengirimanDetail { get; set; } = new List<SIT04B>();
        public List<String> Errors { get; set; } = new List<string>();

        //public string NamaToko { get; set; }
        //public string NamaPerusahaan { get; set; }
        //public string AlamatToko { get; set; }
        //public string TlpToko { get; set; }
        //public string NamaKurir { get; set; }

        //ADD FOR AUTOLOAD 
        //public string DRTGL { get; set; }
        //public string SDTGL { get; set; }
        //public string DRCUST { get; set; }
        //public string SDCUST { get; set; }
        //public string USERNAME { get; set; }

        //add by nurul 16/9/2019, tambah cek shipment dr list pesanan sit04b
        //public List<ShipmentOfKirim> Shipment { get; set; } = new List<ShipmentOfKirim>();
        //end add by nurul 16/9/2019

        public DateTime? cutoffPengiriman { get; set; }
        public List<SerahTerima> ListSerahTerima { get; set; } = new List<SerahTerima>();
        public List<DashboardSerahTerima> ListDashboard { get; set; } = new List<DashboardSerahTerima>();

        public int? kirimId { get; set; }
        public string modeEditDO { get; set; }
    }
    public class SerahTerima
    {
        public string NoPesanan { get; set; }
        public string NoRef { get; set; }
        public string NoFaktur { get; set; }
        public DateTime? TglPesanan { get; set; }
        public string NoDO { get; set; }
        public DateTime? TglDO { get; set; }
        public string Marketplace { get; set; }
        public string Perso { get; set; }
        public string Pembeli { get; set; }
        public double? TipePesanan { get; set; }
        public string Kurir { get; set; }
        public string NoResi { get; set; }
        public string N_UCAPAN { get; set; }
        public string RecnumSO { get; set; }
        public string RecnumDO { get; set; }
    }
    public class DashboardSerahTerima
    {
        public string Kurir { get; set; }
        public int Jumlah { get; set; }
    }

    public class ScanBarcodeSerahTerimaViewModel
    {
        public string KURIR_TEMP { get; set; }
        public List<SerahTerima> listDataScan { get; set; }
    }
    
    public class cetakSerahTerima
    {
        public int? NO_URUT { get; set; }
        public string NO_BUKTI { get; set; }
        public string PESANAN { get; set; }
        public string MARKETPLACE { get; set; }
        public string PEMBELI { get; set; }
        public string NOREF { get; set; }
        public string KURIR { get; set; }
        public string RESI { get; set; }
    }
    //end add by nurul 29/3/2021
}