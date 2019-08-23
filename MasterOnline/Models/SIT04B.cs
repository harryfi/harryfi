using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIT04B
    {
        [StringLength(10)]
        public string NO_BUKTI { get; set; }

        [Key]
        public int? NO_URUT { get; set; }

        [StringLength(10)]
        public string PESANAN { get; set; }

        [StringLength(10)]
        public string MARKETPLACE { get; set; }

        [StringLength(250)]
        public string NAMA_MARKET { get; set; }

        [StringLength(10)]
        public string PEMBELI { get; set; }

        [StringLength(250)]
        public string NAMA_PEMBELI { get; set; }

        [StringLength(250)]
        public string ALAMAT_KIRIM { get; set; }

        [StringLength(50)]
        public string KOTA { get; set; }

        [StringLength(50)]
        public string PROPINSI { get; set; }

        [StringLength(50)]
        public string KODE_POS { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public DateTime? TGL_INPUT { get; set; }
    }
}
