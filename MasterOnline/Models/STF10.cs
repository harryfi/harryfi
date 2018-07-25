namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF10
    {
        [StringLength(20)]
        public string BRG { get; set; }

        public DateTime TGL { get; set; }

        public double QTY { get; set; }

        public double HPOKOK { get; set; }

        [StringLength(4)]
        public string GD { get; set; }

        [Key]
        public int RECNUM { get; set; }

        [Column("ref")]
        [StringLength(10)]
        public string _ref { get; set; }

        [StringLength(20)]
        public string LOT_NO { get; set; }

        [StringLength(20)]
        public string BATCH_NO { get; set; }
    }
}
