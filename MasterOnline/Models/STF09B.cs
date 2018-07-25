namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF09B
    {
        [StringLength(4)]
        public string GD { get; set; }

        [StringLength(20)]
        public string BRG { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGL { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string BUKTI_MSK { get; set; }

        public double? HPOKOK { get; set; }

        public double? QM { get; set; }

        public double? QK { get; set; }

        [Key]
        [Column(Order = 1)]
        public float DETAILNOURUT { get; set; }

        [Key]
        [Column(Order = 2)]
        public int RECNUM { get; set; }
    }
}
