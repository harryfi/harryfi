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

    public class mdlCustomer
    {
        public int? RECNUM { get; set; }
        public string KODE { get; set; }
        public string NAMA { get; set; }
        public string EMAIL { get; set; }
        public string STATUS_API { get; set; }
        public string PERSO { get; set; }
        
    }
}
