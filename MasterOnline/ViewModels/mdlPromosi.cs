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
}
