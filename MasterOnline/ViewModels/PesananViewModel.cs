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

        //add by nurul 2/12/2019, tambah dashboard pesanan
        public int? JumlahPesananBelumBayar { get; set; }
        public double? NilaiPesananBelumBayar { get; set; }
        public int? JumlahPesananSudahBayar { get; set; }
        public double? NilaiPesananSudahBayar { get; set; }
        public int? JumlahPesananSiapKirim { get; set; }
        public double? NilaiPesananSiapKirim { get; set; }
        public int? JumlahPesananBatal { get; set; }
        public double? NilaiPesananBatal { get; set; }
        //end add by nurul 2/12/2019, tambah dashboard pesanan    
        public int? JumlahBatalCOD { get; set; }//add by Tri 9 mar 2021
        //add by nurul 24/3/2020
        public PesananDetail_NotFound PesananDetail_NotFound { get; set; }
        public List<listBarang_NotFound> ListBarang_NotFound { get; set; } = new List<listBarang_NotFound>();
        public List<listBarangMarket_NotFound> ListBarangMarket_NotFound { get; set; } = new List<listBarangMarket_NotFound>();
        //end add by nurul 24/3/2020

        //add by nurul 5/5/2020
        public string namaMarket { get; set; }
        //end add by nurul 5/5/2020

        //add by nurul 1/12/2020
        public int? JumlahPesananPacking { get; set; }
        public double? NilaiPesananPacking { get; set; }
        public int? JumlahPesananFaktur { get; set; }
        public double? NilaiPesananFaktur { get; set; }
        //end add by nurul 1/12/2020

        //add by nurul 20/10/2020
        public SOT01G PesananBundling { get; set; }
        public List<SOT01G> ListPesananBundling { get; set; } = new List<SOT01G>();
        public List<listKomponenBundling> listKomponen { get; set; } = new List<listKomponenBundling>();
        public bool notfoundBundling { get; set; }
        //end add by nurul 20/10/2020

        //ADD BY NURUL 23/3/2021
        public bool prosesUpdateKurirShopee { get; set; }
        //END ADD BY NURUL 23/3/2021
    }

    //add by nurul 24/3/2020
    public class PesananDetail_NotFound
    {
        public string NO_BUKTI { get; set; }
        public string CATATAN { get; set; }
        public int? NO_URUT { get; set; }
    }
    public class listBarangMarket_NotFound
    {
        public string BRG { get; set; }
        public int? RecNum { get; set; }
        public int IDMARKET { get; set; }
    }
    public class listBarang_NotFound
    {
        public string BRG { get; set; }
        public string NAMA { get; set; }
        public string NAMA2 { get; set; }
    }
    //end add by nurul 24/3/2020

    public class listKomponenBundling
    {
        public string bundling { get; set; }
        public string komponen { get; set; }
    }
}