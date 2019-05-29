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
        public int? LKS { get; set; }
        public double NILAI { get; set; }
        
    }
}
