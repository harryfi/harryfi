namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT04D
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(4)]
        public string GUD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime TGL_PROSES { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RECNUM { get; set; }

        public double? QTY { get; set; }

        public double? TGL { get; set; }

        public double? HPOKOK { get; set; }

        [StringLength(1)]
        public string BK { get; set; }

        [StringLength(5)]
        public string STN { get; set; }

        [StringLength(10)]
        public string WO { get; set; }

        [StringLength(30)]
        public string NABRG { get; set; }

        public double? QTY_BERAT { get; set; }

        public double? QTY_KECIL { get; set; }

        public double? QTY_BESAR { get; set; }

        public double? QTY_3 { get; set; }

        public double? QTY_4 { get; set; }

        public int? ID { get; set; }
    }
}
