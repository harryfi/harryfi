namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT05B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string NOBUK { get; set; }

        [Key]
        [Column(Order = 1)]
        public int NO { get; set; }

        [StringLength(20)]
        public string KOBAR { get; set; }

        public short QTY { get; set; }

        [StringLength(10)]
        public string RAK { get; set; }

        [StringLength(4)]
        public string GD { get; set; }

        [StringLength(1)]
        public string BS { get; set; }

        [StringLength(1)]
        public string STN { get; set; }

        [StringLength(10)]
        public string WO { get; set; }
    }
}
