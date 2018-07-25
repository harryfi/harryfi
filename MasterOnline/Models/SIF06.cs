namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF06
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(3)]
        public string WIL { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(1)]
        public string LEVEL_ITEM { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string KODE_LEVEL { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(3)]
        public string SLM { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(10)]
        public string CUST { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short THN { get; set; }

        public double QTY_1 { get; set; }

        public double QTY_2 { get; set; }

        public double QTY_3 { get; set; }

        public double QTY_4 { get; set; }

        public double QTY_5 { get; set; }

        public double QTY_6 { get; set; }

        public double QTY_7 { get; set; }

        public double QTY_8 { get; set; }

        public double QTY_9 { get; set; }

        public double QTY_10 { get; set; }

        public double QTY_11 { get; set; }

        public double QTY_12 { get; set; }

        public double NILAI_1 { get; set; }

        public double NILAI_2 { get; set; }

        public double NILAI_3 { get; set; }

        public double NILAI_4 { get; set; }

        public double NILAI_5 { get; set; }

        public double NILAI_6 { get; set; }

        public double NILAI_7 { get; set; }

        public double NILAI_8 { get; set; }

        public double NILAI_9 { get; set; }

        public double NILAI_10 { get; set; }

        public double NILAI_11 { get; set; }

        public double NILAI_12 { get; set; }

        public DateTime TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        public double? GM_1 { get; set; }

        public double? GM_2 { get; set; }

        public double? GM_3 { get; set; }

        public double? GM_4 { get; set; }

        public double? GM_5 { get; set; }

        public double? GM_6 { get; set; }

        public double? GM_7 { get; set; }

        public double? GM_8 { get; set; }

        public double? GM_9 { get; set; }

        public double? GM_10 { get; set; }

        public double? GM_11 { get; set; }

        public double? GM_12 { get; set; }
    }
}
