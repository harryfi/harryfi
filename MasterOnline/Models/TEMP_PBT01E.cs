namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_PBT01E
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

        [Key]
        [Column(Order = 5)]
        [StringLength(20)]
        public string BATCH_NO { get; set; }

        [Key]
        [Column(Order = 6)]
        public double QTY { get; set; }

        [Key]
        [Column(Order = 7)]
        [StringLength(4)]
        public string GUD { get; set; }

        [Key]
        [Column(Order = 8)]
        [StringLength(10)]
        public string STN { get; set; }

        [Key]
        [Column(Order = 9)]
        public DateTime EX_DATE { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(1)]
        public string STATUS { get; set; }

        [Key]
        [Column(Order = 11)]
        public double QTY_OUT { get; set; }

        [Key]
        [Column(Order = 12)]
        [StringLength(1000)]
        public string SPESIFIKASI { get; set; }

        [Key]
        [Column(Order = 13)]
        public int RecNum { get; set; }
    }
}
