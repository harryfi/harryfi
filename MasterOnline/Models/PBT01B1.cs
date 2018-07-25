namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBT01B1
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENISFORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string INV { get; set; }

        [StringLength(10)]
        public string PO { get; set; }

        [Key]
        [Column(Order = 2)]
        public int NO { get; set; }

        [Required]
        [StringLength(1)]
        public string JENIS_SIZE { get; set; }

        [StringLength(4)]
        public string GD { get; set; }

        [StringLength(1)]
        public string BK { get; set; }

        public double QTY1 { get; set; }

        public double QTY2 { get; set; }

        public double QTY3 { get; set; }

        public double QTY4 { get; set; }

        public double QTY5 { get; set; }

        public double QTY6 { get; set; }

        public double QTY7 { get; set; }

        public double QTY8 { get; set; }

        public double QTY9 { get; set; }

        public double QTY10 { get; set; }

        public double QTY11 { get; set; }

        public double QTY12 { get; set; }

        public double QTY13 { get; set; }

        public double QTY14 { get; set; }

        public double QTY15 { get; set; }

        public double DISC2 { get; set; }

        public double NDISC2 { get; set; }

        public double HBELI { get; set; }

        public double THARGA { get; set; }

        [StringLength(10)]
        public string NOBUK { get; set; }

        [StringLength(2)]
        public string AUTO_LOAD { get; set; }

        public double? QTY_RETUR { get; set; }

        public double? BIAYA { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public DateTime TGLINPUT { get; set; }

        public double? TOTAL_LOT { get; set; }

        public double? TOTAL_QTY { get; set; }

        public double DISCOUNT_1 { get; set; }

        public double DISCOUNT_2 { get; set; }

        public double DISCOUNT_3 { get; set; }

        public double NILAI_DISC_1 { get; set; }

        public double NILAI_DISC_2 { get; set; }

        public double NILAI_DISC_3 { get; set; }

        [StringLength(1000)]
        public string KET { get; set; }

        public double? NO_URUT_PO { get; set; }

        public double? PPNBM { get; set; }

        public double? NILAI_PPNBM { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT1 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT2 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT3 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT4 { get; set; }

        public virtual PBT01A PBT01A { get; set; }
    }
}
