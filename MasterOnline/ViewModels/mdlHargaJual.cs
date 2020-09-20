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
    
    public class mdlHargaJual
    {
        public int? RECNUM { get; set; }
        public string BRG { get; set; }
        public string IDMARKET { get; set; }
        public string NAMA { get; set; }
        public string NAMA2 { get; set; }
        public string NAMAMARKET { get; set; }
        public string AKUNMARKET { get; set; }
        public double HJUAL { get; set; }
        public double HPOKOK { get; set; }
        public double STF02_HJUAL { get; set; }
    }
    public class mdlHargaJualMassal
    {
        public DateTime TGL { get; set; }
        public string IDMARKET { get; set; }
        public string USERNAME { get; set; }
        public string FILE { get; set; }
        public string STATUS { get; set; }
        public string NAMAMARKET { get; set; }
        public string AKUNMARKET { get; set; }
    }
}
