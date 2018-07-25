namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF02B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string NO_GIRO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(11)]
        public string BUKTI_TERIMA { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGL_TERIMA { get; set; }

        [StringLength(11)]
        public string BUKTI_SETOR { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGL_SETOR { get; set; }

        [StringLength(11)]
        public string BUKTI_TOLAK { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGL_TOLAK { get; set; }

        [StringLength(11)]
        public string BUKTI_GANTI { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGL_GANTI { get; set; }

        [StringLength(20)]
        public string GIRO_PENGGANTI { get; set; }

        [StringLength(2)]
        public string ST_GIRO { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        public double? TUKAR { get; set; }

        public double? NILAI { get; set; }

        [Key]
        [Column(Order = 2)]
        public int RECNUM { get; set; }

        public int? DetailNoUrut { get; set; }
    }
}
