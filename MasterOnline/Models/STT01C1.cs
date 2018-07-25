namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT01C1
    {
        public byte JENISFORM { get; set; }

        [Required]
        [StringLength(10)]
        public string BUKTI { get; set; }

        public int NO_URUT { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG { get; set; }

        [Required]
        [StringLength(20)]
        public string LOT_NO { get; set; }

        [StringLength(20)]
        public string BATCH_NO { get; set; }

        public double QTY { get; set; }

        [StringLength(4)]
        public string GUD { get; set; }

        [StringLength(10)]
        public string STN { get; set; }

        public DateTime EX_DATE { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        public double QTY_OUT { get; set; }

        [StringLength(1000)]
        public string SPESIFIKASI { get; set; }

        [Key]
        public int RECNUM { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
