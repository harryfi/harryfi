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

    public class mdlSaldoAwal
    {
        public int? RECNUM { get; set; }
        public string NO_BUKTI { get; set; }
        public DateTime? TGL { get; set; }
        public string KODE { get; set; }
        public string NAMA { get; set; }
        public DateTime? JTGL { get; set; }
        public string POSTING { get; set; }
        public double TOTAL { get; set; }
    }

    public class mdlMultiSKU
    {
        public int? recnum { get; set; }
        public string brg_acuan { get; set; }
        public string nama { get; set; }
        public DateTime? tgl_edit { get; set; }
    }

    public class mdlMultiSKUDetail
    {
        public string brg { get; set; }
        public string nama { get; set; }
        public string brg_acuan { get; set; }
        public string nama_acuan { get; set; }
    }
}
