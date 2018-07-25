namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF02D
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string UNIT { get; set; }

        [Key]
        [Column(Order = 1)]
        public int NO_URUT { get; set; }

        [StringLength(20)]
        public string BRG { get; set; }

        public double QTY { get; set; }

        [StringLength(1)]
        public string SATUAN { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public virtual STF02 STF02 { get; set; }
    }
}
