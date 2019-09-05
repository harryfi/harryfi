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

    }
    //end add 5/9/2019 by Tri, model untuk master pembeli
}
