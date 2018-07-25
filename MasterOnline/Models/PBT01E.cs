namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBT01E
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENISFORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string BUKTI { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NO_URUT { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(20)]
        public string LOT_NO { get; set; }

        [Required]
        [StringLength(20)]
        public string BATCH_NO { get; set; }

        public double QTY { get; set; }

        [Required]
        [StringLength(4)]
        public string GUD { get; set; }

        [Required]
        [StringLength(10)]
        public string STN { get; set; }

        public DateTime EX_DATE { get; set; }

        [Required]
        [StringLength(1)]
        public string STATUS { get; set; }

        public double QTY_OUT { get; set; }

        [Required]
        [StringLength(1000)]
        public string SPESIFIKASI { get; set; }

        [Key]
        [Column(Order = 5)]
        public int RecNum { get; set; }

        public bool? AutoLoad_Serial { get; set; }

        public bool? AutoLoad_Batch { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public virtual PBT01B PBT01B { get; set; }
    }
}
