using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Linq;
    using System.Web;
    using MasterOnline.Models;
    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Web;
    //using MasterOnline.Models;

    //namespace MasterOnline.ViewModels
    //{
    public class mdlJurnal
    {
        public int? RECNUM { get; set; }
        public string BUKTI { get; set; }
        public DateTime TGL { get; set; }
        public string POSTING { get; set; }
        public double DEBET { get; set; }
        public double KREDIT { get; set; }
        public short LKS { get; set; }

    }
    public class PackingPerMP
    {
        public string CUST { get; set; }
        public string NAMA_CUST { get; set; }
        public string no_bukti { get; set; }
        public string no_referensi { get; set; }
        public string nama_pemesan { get; set; }
        public string kurir { get; set; }
        public int jumlah_item { get; set; }
        public string status_kirim { get; set; }
        public string tracking_no { get; set; }
        public int so_recnum { get; set; }

        //add by nurul 20/3/2020, job
        public string no_job { get; set; }
        //end add by nurul 20/3/2020
    }

    public class tempBarcodeLazada
    {
        public string ResiApi { get; set; }
        public string referensiApi { get; set; }
        public string PortCodeApi { get; set; }
        public string HargaApi { get; set; }
        public string urlLogoKurirApi { get; set; }
        public string tglApi { get; set; }        
    }

    public class tempLabel
    {
        public string CUST { get; set; }
        //public string NAMA_CUST { get; set; }
        public string so_bukti { get; set; }
        public string so_referensi { get; set; }
        public string no_resi { get; set; }
        public string kurir { get; set; }
        public double so_netto { get; set; }
        public double si_netto { get; set; }
        public double so_ongkir { get; set; }
        public string so_kota { get; set; }
        public string so_propinsi { get; set; }
        public string so_pos { get; set; }
        public string so_alamat { get; set; }
        public string nama_pemesan { get; set; }
        public int jumlah_item { get; set; }
        public string si_bukti { get; set; }
        public DateTime? si_tgl { get; set; }
        public string perso { get; set; }
        public string namamarket { get; set; }
        public string logo { get; set; }
        public string namapembeli { get; set; }
        public string tlppembeli { get; set; }

        //add by nurul 26/3/2020
        public string no_job { get; set; }
        //end add by nurul 26/3/2020

        //add by nurul 15/5/2020
        public string ket { get; set; }
        //end add by nurul 15/5/2020
    }

    //add by nurul 2/6/2020
    public class tempLabelTokopedia
    {
        public string Header { get; set; }
        public string Resi { get; set; }
        public string Invoice { get; set; }
        public string Insurance { get; set; }
        public string Address { get; set; }
        public string Product { get; set; }
    }
    //end add by nurul 2/6/2020
}
