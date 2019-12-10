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
    }
}
