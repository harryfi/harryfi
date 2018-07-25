namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tmp_STF08
    {
        [StringLength(20)]
        public string brg { get; set; }

        [Key]
        [Column(Order = 0)]
        public double isi { get; set; }

        [Key]
        [Column(Order = 1)]
        public double isi3 { get; set; }

        [Key]
        [Column(Order = 2)]
        public double isi4 { get; set; }

        [Key]
        [Column(Order = 3)]
        public double qAwal { get; set; }

        [Key]
        [Column(Order = 4)]
        public double nAwal { get; set; }

        [Key]
        [Column(Order = 5)]
        public double QMasuk { get; set; }

        [Key]
        [Column(Order = 6)]
        public double QKeluar { get; set; }

        [Key]
        [Column(Order = 7)]
        public double NMasuk { get; set; }

        [Key]
        [Column(Order = 8)]
        public double NKeluar { get; set; }
    }
}
