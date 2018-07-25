namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT03B
    {
        [Key]
        public int NO { get; set; }

        [StringLength(10)]
        public string NOBUK { get; set; }

        [StringLength(20)]
        public string KOBAR { get; set; }

        public double QTY { get; set; }

        [StringLength(4)]
        public string GD { get; set; }

        [StringLength(20)]
        public string Nm_BRG { get; set; }

        [StringLength(1)]
        public string Satuan { get; set; }

        public virtual STT03A STT03A { get; set; }
    }
}
