using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class FakturViewModel
    {
        public string NamaToko { get; set; }
        public string LogoMarket { get; set; }
        public string NamaPerusahaan { get; set; }
        public SIT01A Faktur { get; set; }
        public List<SIT01A> ListFaktur { get; set; } = new List<SIT01A>();
        public SIT01B FakturDetail { get; set; }
        public List<SIT01B> ListFakturDetail { get; set; } = new List<SIT01B>();
        public List<STF02> ListBarang { get; set; } = new List<STF02>();
        public List<STF02H> ListBarangMarket { get; set; } = new List<STF02H>();
        public List<ARF01C> ListPembeli { get; set; } = new List<ARF01C>();
        public List<ARF01> ListPelanggan { get; set; } = new List<ARF01>();
        public List<Marketplace> ListMarketplace { get; set; } = new List<Marketplace>();
        public List<ART03B> ListNFaktur { get; set; } = new List<ART03B>();
        public List<SOT01A> ListPesanan { get; set; } = new List<SOT01A>();
        public List<String> Errors { get; set; } = new List<string>();
        public List<Subscription> ListSubs { get; set; } = new List<Subscription>();
        public List<LOG_IMPORT_FAKTUR> ListImportFaktur { get; set; } = new List<LOG_IMPORT_FAKTUR>();
        //add by nurul 29/11/2018 (modiv cetak faktur)
        public string AlamatToko { get; set; }
        public string TlpToko { get; set; }
        public string noRef { get; set; }
        //add by nurul 28/1/2019
        public string Kurir { get; set; }
        public string Marketplace { get; set; }
        public string LogoKurir { get; set; }
        public string NoResi { get; set; }
        //add by nurul 8/3/2019
        public List<QOH_PER_GD> ListQOHPerGD { get; set; } = new List<QOH_PER_GD>();
        public string setGd { get; set; }
        public string alamatPenerima { get; set; }
        //end add by nurul 8/3/2019

        //add by nurul 11/12/2019, for cetak label mo
        public List<CetakLabelViewModel> ListCetakLabel { get; set; } = new List<CetakLabelViewModel>();
        public string urlAl { get; set; }
        public string urlTlp { get; set; }
        public string urlMp { get; set; }
        public string urlNobuk { get; set; }
        public string urlTotal { get; set; }
        public string urlNama { get; set; }
        public string urlFaktur { get; set; }
        public string urlLabel { get; set; }
        public string urlKet { get; set; }
        //end add by nurul 11/12/2019, for cetak label mo

        //add by nurul 3/6/2020
        public List<tempLabelTokopedia> listLabelTokped { get; set; } = new List<tempLabelTokopedia>();
        //end add by nurul 3/6/2020

        //add by nurul 8/6/2020
        public string noCust { get; set; }
        public string noFaktur { get; set; }
        public List<STF18> listGudang { get; set; }
        public List<brgForReturPrompt> listBarangRetur { get; set; } = new List<brgForReturPrompt>();
        //end add by nurul 8/6/2020

        //add by nurul 20/10/2020
        public SIT01H FakturBundling { get; set; }
        public List<SIT01H> ListFakturBundling { get; set; } = new List<SIT01H>();
        public List<listKomponenBundling> listKomponen { get; set; } = new List<listKomponenBundling>();
        //end add by nurul 20/10/2020
    }

    public class CetakLabelViewModel
    {
        public string NamaToko { get; set; }
        public string LogoMarket { get; set; }
        public string NamaPerusahaan { get; set; }
        public string AlamatToko { get; set; }
        public string TlpToko { get; set; }
        public string noRef { get; set; }
        public string Kurir { get; set; }
        public string Marketplace { get; set; }
        public string LogoKurir { get; set; }
        public string NoResi { get; set; }
        public string alamatPenerima { get; set; }
        public SIT01A Faktur { get; set; }
        public List<ARF01C> ListPembeli { get; set; } = new List<ARF01C>();
        public List<SIT01B> ListFakturDetail { get; set; } = new List<SIT01B>();
        public List<STF02> ListBarang { get; set; } = new List<STF02>();
        
        public string linktotal { get; set; }
        public string linktoko { get; set; }
        public string linktlptoko { get; set; }
        public string linkport { get; set; }
        public string linkref { get; set; }
        public string isiPort { get; set; }
        public string isiRef { get; set; }
        public string namaPembeli { get; set; }
        public string tlpPembeli { get; set; }
        public string tglKirim { get; set; }        
        public string logoKurirApi { get; set; }
        
        //add by nurul 2/4/2020
        public string KdBooking { get; set; }
        //end add by nurul 2/4/2020

        //add by nurul 15/5/2020
        public string Ket { get; set; }
        public List<tempKetTokped> listKetTokped = new List<tempKetTokped>();
        //end add by nurul 15/5/2020
    }

    public class tempKetTokped
    {
        public string Nobuk { get; set; }
        public string Brg { get; set; }
        public string KetDetail { get; set; }
        public string brg_multisku { get; set; }
    }

    //add by nurul 8/6/2020
    public class FakturForReturPrompt
    {
        public string no_bukti { get; set; }
        public DateTime tgl { get; set; }
        public string namapemesan { get; set; }
        public string no_ref { get; set; }
        public double netto { get; set; }
    }
    public class brgForReturPrompt
    {
        public int recnum { get; set; }
        public string brg { get; set; }
        public string nama { get; set; }
        public double qty { get; set; }
        public string gudang { get; set; }
    }
    //end add by nurul 8/6/2020
}