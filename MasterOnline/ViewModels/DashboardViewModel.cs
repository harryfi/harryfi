using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class DashboardViewModel
    {
        //remark by calvin 17 september 2019
        //public List<SOT01A> ListPesanan { get; set; } = new List<SOT01A>();
        //public List<SOT01B> ListPesananDetail { get; set; } = new List<SOT01B>();
        //public List<SIT01A> ListFaktur { get; set; } = new List<SIT01A>();
        //public List<SIT01B> ListFakturDetail { get; set; } = new List<SIT01B>();
        //public List<STF02> ListBarang { get; set; } = new List<STF02>();
        //end remark by calvin 17 september 2019

        public List<ARF01> ListAkunMarketplace { get; set; } = new List<ARF01>();
        public List<Marketplace> ListMarket { get; set; } = new List<Marketplace>();
        public List<STF08A> ListBarangUntukCekQty { get; set; } = new List<STF08A>();
        public List<STT01B> ListStok { get; set; } = new List<STT01B>();
        public List<PesananPerMarketplaceModel> ListPesananPerMarketplace { get; set; } = new List<PesananPerMarketplaceModel>();
        public List<PenjualanBarang> ListBarangLaku { get; set; } = new List<PenjualanBarang>();
        public List<PenjualanBarang> ListBarangTidakLaku { get; set; } = new List<PenjualanBarang>();
        public List<PenjualanBarang> ListBarangMiniStok = new List<PenjualanBarang>();
        //add by nurul 11/7/2019
        public List<DashboardMingguanModel> ListdashboardPesananMingguan { get; set; } = new List<DashboardMingguanModel>();
        public List<DashboardBulananModel> ListdashboardPesananBulanan { get; set; } = new List<DashboardBulananModel>();
        public List<DashboardTahunanModel> ListdashboardPesananTahunan { get; set; } = new List<DashboardTahunanModel>();

        public List<DashboardMingguanModel> ListdashboardFakturMingguan { get; set; } = new List<DashboardMingguanModel>();
        public List<DashboardBulananModel> ListdashboardFakturBulanan { get; set; } = new List<DashboardBulananModel>();
        public List<DashboardTahunanModel> ListdashboardFakturTahunan { get; set; } = new List<DashboardTahunanModel>();

        public List<DashboardMingguanModel> ListdashboardReturMingguan { get; set; } = new List<DashboardMingguanModel>();
        public List<DashboardBulananModel> ListdashboardReturBulanan { get; set; } = new List<DashboardBulananModel>();
        public List<DashboardTahunanModel> ListdashboardReturTahunan { get; set; } = new List<DashboardTahunanModel>();
        //end add by nurul 11/7/2019

        //add by nurul 5/7/2019
        public List<FakturPerMarketplaceModel> ListFakturPerMarketplace { get; set; } = new List<FakturPerMarketplaceModel>();
        //end add by nurul 5/7/2019

        public int? listBarangCount { get; set; }
        public int? JumlahPesananHariIni { get; set; }
        public double? NilaiPesananHariIni { get; set; }
        public int? JumlahPesananBulanIni { get; set; }
        public double? NilaiPesananBulanIni { get; set; }
        public int? JumlahFakturHariIni { get; set; }
        public double? NilaiFakturHariIni { get; set; }
        public int? JumlahFakturBulanIni { get; set; }
        public double? NilaiFakturBulanIni { get; set; }
        public int? JumlahReturHariIni { get; set; }
        public double? NilaiReturHariIni { get; set; }
        public int? JumlahReturBulanIni { get; set; }
        public double? NilaiReturBulanIni { get; set; }

        //add by nurul 9/9/2019
        public List<string> Errors { get; set; } = new List<string>();
        //end add by nurul 9/9/2019

        //add by nurul 19/9/2019
        public int? BarangTidakLakuCount { get; set; }
        public int? BarangDibawahMinStokCount { get; set; }
        //end add by nurul 19/9/2019

        //add by nurul 18/11/2019, arus kas 
        public double totalSI { get; set; }
        public double totalPB { get; set; }
        public double selisih { get; set; }
        //end add by nurul 18/11/2019, arus kas 

        //add by nurul 7/12/2020
        public DashboardPesanan pesananByStatus { get; set; }
        //end add by nurul 7/12/2020

        public PesananPerMarketplaceModelGroupByStatus ListPesananPerMarketplaceGroupByStatus { get; set; }
    }

    //add by nurul 7/12/2020
    public class DashboardPesanan
    {
        public int? JumlahPesananHariIni_Semua { get; set; }
        public double? NilaiPesananHariIni_Semua { get; set; }
        public int? JumlahPesananBulanIni_Semua { get; set; }
        public double? NilaiPesananBulanIni_Semua { get; set; }

        public int? JumlahPesananHariIni_Unpaid { get; set; }
        public double? NilaiPesananHariIni_Unpaid { get; set; }
        public int? JumlahPesananBulanIni_Unpaid { get; set; }
        public double? NilaiPesananBulanIni_Unpaid { get; set; }

        public int? JumlahPesananHariIni_Paid { get; set; }
        public double? NilaiPesananHariIni_Paid { get; set; }
        public int? JumlahPesananBulanIni_Paid { get; set; }
        public double? NilaiPesananBulanIni_Paid { get; set; }

        public int? JumlahPesananHariIni_Packing { get; set; }
        public double? NilaiPesananHariIni_Packing { get; set; }
        public int? JumlahPesananBulanIni_Packing { get; set; }
        public double? NilaiPesananBulanIni_Packing { get; set; }

        public int? JumlahPesananHariIni_Selesai { get; set; }
        public double? NilaiPesananHariIni_Selesai { get; set; }
        public int? JumlahPesananBulanIni_Selesai { get; set; }
        public double? NilaiPesananBulanIni_Selesai { get; set; }

        public int? JumlahPesananHariIni_Batal { get; set; }
        public double? NilaiPesananHariIni_Batal { get; set; }
        public int? JumlahPesananBulanIni_Batal { get; set; }
        public double? NilaiPesananBulanIni_Batal { get; set; }
    }
    public class PesananPerMarketplaceModelGroupByStatus
    {
        public List<PesananPerMarketplaceModel_Semua> listPesananMarket_Semua = new List<PesananPerMarketplaceModel_Semua>();
        public List<PesananPerMarketplaceModel_Unpaid> listPesananMarket_Unpaid = new List<PesananPerMarketplaceModel_Unpaid>();
        public List<PesananPerMarketplaceModel_Paid> listPesananMarket_Paid = new List<PesananPerMarketplaceModel_Paid>();
        public List<PesananPerMarketplaceModel_Packing> listPesananMarket_Packing = new List<PesananPerMarketplaceModel_Packing>();
        public List<PesananPerMarketplaceModel_Selesai> listPesananMarket_Selesai = new List<PesananPerMarketplaceModel_Selesai>();
        public List<PesananPerMarketplaceModel_Batal> listPesananMarket_Batal = new List<PesananPerMarketplaceModel_Batal>();
    }
    public class PesananPerMarketplaceModel_Semua
    {
        public string NamaMarket { get; set; }
        public string JumlahPesananHariIni { get; set; }
        public string NilaiPesananHariIni { get; set; }
        public string JumlahPesananBulanIni { get; set; }
        public string NilaiPesananBulanIni { get; set; }
    }
    public class PesananPerMarketplaceModel_Unpaid
    {
        public string NamaMarket { get; set; }
        public string JumlahPesananHariIni { get; set; }
        public string NilaiPesananHariIni { get; set; }
        public string JumlahPesananBulanIni { get; set; }
        public string NilaiPesananBulanIni { get; set; }
    }
    public class PesananPerMarketplaceModel_Paid
    {
        public string NamaMarket { get; set; }
        public string JumlahPesananHariIni { get; set; }
        public string NilaiPesananHariIni { get; set; }
        public string JumlahPesananBulanIni { get; set; }
        public string NilaiPesananBulanIni { get; set; }
    }
    public class PesananPerMarketplaceModel_Packing
    {
        public string NamaMarket { get; set; }
        public string JumlahPesananHariIni { get; set; }
        public string NilaiPesananHariIni { get; set; }
        public string JumlahPesananBulanIni { get; set; }
        public string NilaiPesananBulanIni { get; set; }
    }
    public class PesananPerMarketplaceModel_Selesai
    {
        public string NamaMarket { get; set; }
        public string JumlahPesananHariIni { get; set; }
        public string NilaiPesananHariIni { get; set; }
        public string JumlahPesananBulanIni { get; set; }
        public string NilaiPesananBulanIni { get; set; }
    }
    public class PesananPerMarketplaceModel_Batal
    {
        public string NamaMarket { get; set; }
        public string JumlahPesananHariIni { get; set; }
        public string NilaiPesananHariIni { get; set; }
        public string JumlahPesananBulanIni { get; set; }
        public string NilaiPesananBulanIni { get; set; }
    }
    //end add by nurul 7/12/2020
}