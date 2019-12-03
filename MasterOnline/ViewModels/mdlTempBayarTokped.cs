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
    
    public class mdlTempBayarTokped
    {
        public string REF { get; set; }
        public DateTime TGL { get; set; }
        public string KETERANGAN { get; set; }
        public double NILAI { get; set; }
    }
}
