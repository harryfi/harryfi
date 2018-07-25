namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF09C
    {
        [StringLength(4)]
        public string GD { get; set; }

        [StringLength(20)]
        public string BRG { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGL { get; set; }

        [Required]
        [StringLength(20)]
        public string BUKTI { get; set; }

        public double? HPOKOK { get; set; }

        public double? QTY { get; set; }

        public int? DETAILNOURUT { get; set; }

        [Key]
        public int RECNUM { get; set; }

        public double? DETAILNOURUT_BUKTI_MSK { get; set; }

        [StringLength(20)]
        public string BUKTI_MSK { get; set; }

        [StringLength(1)]
        public string MK { get; set; }

        [StringLength(10)]
        public string WO { get; set; }
    }
}
