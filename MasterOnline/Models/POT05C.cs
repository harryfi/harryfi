namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT05C
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string NO_PENAWARAN { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string NO_PP { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(20)]
        public string KD_BRG { get; set; }

        public byte? STN { get; set; }

        public double? QTY { get; set; }

        public double? QTY_QR { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
