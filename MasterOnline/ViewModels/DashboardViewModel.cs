using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class DashboardViewModel
    {
        public List<SOT01A> ListPesanan { get; set; } = new List<SOT01A>();
        public List<SOT01B> ListPesananDetail { get; set; } = new List<SOT01B>();
        public List<SIT01A> ListFaktur { get; set; } = new List<SIT01A>();
        public List<SIT01B> ListFakturDetail { get; set; } = new List<SIT01B>();
        public List<STF02> ListBarang { get; set; } = new List<STF02>();
        public List<ARF01> ListAkunMarketplace { get; set; } = new List<ARF01>();
        public List<Marketplace> ListMarket { get; set; } = new List<Marketplace>();
        public List<STF08A> ListBarangUntukCekQty { get; set; } = new List<STF08A>();
        public List<STT01B> ListStok { get; set; } = new List<STT01B>();
        public List<PesananPerMarketplaceModel> ListPesananPerMarketplace { get; set; } = new List<PesananPerMarketplaceModel>();
        public List<PenjualanBarang> ListBarangLaku { get; set; } = new List<PenjualanBarang>();
        public List<PenjualanBarang> ListBarangTidakLaku { get; set; } = new List<PenjualanBarang>();
        public List<PenjualanBarang> ListBarangMiniStok = new List<PenjualanBarang>();

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
    }
}