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
    
    public class mdlPromosi
    {
        public int? RECNUM { get; set; }
        public string NAMA { get; set; }
        public DateTime TGL_MULAI { get; set; }
        public DateTime TGL_AKHIR { get; set; }
        public string NAMAMARKET { get; set; }
        
    }

    //add 5/9/2019 by Tri, model untuk master pembeli
    public class mdlPembeli
    {
        public int RecNum { get; set; }
        public string BUYER_CODE { get; set; }
        public string NAMA { get; set; }
        public string KODEPROV { get; set; }
        public string EMAIL { get; set; }
        public string KODEKABKOT { get; set; }
        public string TLP { get; set; }
        public int FREKUENSI { get; set; }
        public double NILAI { get; set; }
    }
    //end add 5/9/2019 by Tri, model untuk master pembeli

    //add by Tri, 24/9/19
    public class mdlPromosiBarng
    {
        public int? RECNUM { get; set; }
        public string BRG { get; set; }
        public string NAMA_BARANG { get; set; }
        public string NAMA_PROMO { get; set; }
        public DateTime TGL_MULAI { get; set; }
        public DateTime TGL_AKHIR { get; set; }
        public string NAMAMARKET { get; set; }
        public double HARGA_NORMAL { get; set; }
        public double HARGA_PROMO { get; set; }
        public double PERSEN_PROMO { get; set; }

    }
    //add by Tri, 24/9/19


    public class mdlPackinglist
    {
        public int? RECNUM { get; set; }
        public string NO_BUKTI { get; set; }
        public DateTime? TGL { get; set; }

    }
}
