namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF22
    {
        [Key]
        [StringLength(5)]
        public string Kd_Kasir { get; set; }

        [StringLength(20)]
        public string Nama { get; set; }

        [StringLength(30)]
        public string AL1 { get; set; }

        [StringLength(30)]
        public string AL2 { get; set; }

        [StringLength(15)]
        public string TLP { get; set; }

        public bool? LEVEL_USER { get; set; }

        public double? KEMBALI { get; set; }
    }
}
