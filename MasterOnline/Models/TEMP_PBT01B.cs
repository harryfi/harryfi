namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_PBT01B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENISFORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string INV { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string PO { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(40)]
        public string NAMA_BRG { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(4)]
        public string GD { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(1)]
        public string BK { get; set; }

        [Key]
        [Column(Order = 7)]
        public double QTY { get; set; }

        [Key]
        [Column(Order = 8)]
        public double DISC2 { get; set; }

        [Key]
        [Column(Order = 9)]
        public double NDISC2 { get; set; }

        [Key]
        [Column(Order = 10)]
        public double HBELI { get; set; }

        [Key]
        [Column(Order = 11)]
        public double THARGA { get; set; }

        [Key]
        [Column(Order = 12)]
        [StringLength(10)]
        public string NOBUK { get; set; }

        [Key]
        [Column(Order = 13)]
        [StringLength(2)]
        public string AUTO_LOAD { get; set; }

        [Key]
        [Column(Order = 14)]
        public double QTY_RETUR { get; set; }

        [Key]
        [Column(Order = 15)]
        public double BIAYA { get; set; }

        [Key]
        [Column(Order = 16)]
        [StringLength(20)]
        public string USERNAME { get; set; }

        [Key]
        [Column(Order = 17)]
        public DateTime TGLINPUT { get; set; }

        [Key]
        [Column(Order = 18)]
        public double TOTAL_LOT { get; set; }

        [Key]
        [Column(Order = 19)]
        public double TOTAL_QTY { get; set; }

        [Key]
        [Column(Order = 20)]
        public double DISCOUNT_1 { get; set; }

        [Key]
        [Column(Order = 21)]
        public double DISCOUNT_2 { get; set; }

        [Key]
        [Column(Order = 22)]
        public double DISCOUNT_3 { get; set; }

        [Key]
        [Column(Order = 23)]
        public double NILAI_DISC_1 { get; set; }

        [Key]
        [Column(Order = 24)]
        public double NILAI_DISC_2 { get; set; }

        [Key]
        [Column(Order = 25)]
        public double NILAI_DISC_3 { get; set; }

        [Key]
        [Column(Order = 26)]
        [StringLength(1000)]
        public string KET { get; set; }

        [Key]
        [Column(Order = 27)]
        public double NO_URUT_PO { get; set; }

        [Key]
        [Column(Order = 28)]
        public double PPNBM { get; set; }

        [Key]
        [Column(Order = 29)]
        public double NILAI_PPNBM { get; set; }

        [Key]
        [Column(Order = 30)]
        public double NO { get; set; }
    }
}
