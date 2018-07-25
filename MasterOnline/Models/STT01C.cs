namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT01C
    {
        public byte JENISFORM { get; set; }

        [Required]
        [StringLength(10)]
        public string BUKTI { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
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

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }

        public bool Autoload_Serial { get; set; }

        public double Autoload_Batch { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
