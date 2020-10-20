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

    //ADD BY NURUL 5/10/2020, BUNDLING
    public class mdlBundling
    {
        public int? recnum { get; set; }
        public string unit { get; set; }
        public string nama { get; set; }
        public string brg_bundling { get; set; }
        public DateTime? tgl_edit { get; set; }
    }

    public class mdlBundlingDetail
    {
        public string brg_komponen { get; set; }
        public string nama_komponen { get; set; }
        public double qty { get; set; }
        public string brg_bundling { get; set; }
        public string nama_bundling { get; set; }
    }

    public class mdlQtyBrgBundling
    {
        public string brg { get; set; }
        public double qty_sales { get; set; }
        public double qty_komp { get; set; }
    }

    public class mdlQtyBundling
    {
        public string Unit { get; set; }
        public double qty_bundling { get; set; }
    }
    //END ADD BY NURUL 5/10/2020, BUNDLING 
}
