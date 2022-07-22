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
    
    public class mdlUploadBarang
    {
        public int? RECNUM { get; set; }
        public string BRG_MP { get; set; }
        public string SELLER_SKU { get; set; }
        public string MEREK { get; set; }
        public string NAMA { get; set; }
        public string NAMA2 { get; set; }
        public string CATEGORY_NAME { get; set; }
        public double HJUAL { get; set; }

    }
    public class mdlSinkroBarang
    {
        public int? RECNUM { get; set; }
        public string BRG_MP { get; set; }
        public string SELLER_SKU { get; set; }
        public string GAMBAR { get; set; }
        public string NAMA { get; set; }
        public string NAMA2 { get; set; }
        public string BRG { get; set; }
        public string NAMABRG { get; set; }
        public string GAMBAR_MO { get; set; }
        public string TYPE { get; set; }

    }
}
