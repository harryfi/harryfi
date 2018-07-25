namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tmp_STF08A
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string brg { get; set; }

        [Key]
        [Column(Order = 1)]
        public double qAwal { get; set; }

        [Key]
        [Column(Order = 2)]
        public double nAwal { get; set; }

        public double? QMasuk { get; set; }

        public double? QKeluar { get; set; }

        public double? NMasuk { get; set; }

        public double? NKeluar { get; set; }
    }
}
