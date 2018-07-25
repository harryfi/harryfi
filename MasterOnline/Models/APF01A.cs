namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class APF01A
    {
        [Key]
        [StringLength(10)]
        public string SUPP { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_1 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_2 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_3 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_4 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_5 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_6 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_7 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_8 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_9 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_10 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_11 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_12 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_13 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_14 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_15 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_16 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_17 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_18 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_19 { get; set; }

        [StringLength(40)]
        public string SPESIFIKASI_20 { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
