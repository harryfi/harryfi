namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SOT01B2
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(15)]
        public string NO_BUKTI { get; set; }

        [Key]
        [Column(Order = 1)]
        public int NO_URUT { get; set; }

        [StringLength(1)]
        public string SATUAN { get; set; }

        [StringLength(1)]
        public string JENIS_SIZE { get; set; }

        public double? H_SATUAN { get; set; }

        public double? QTY1 { get; set; }

        public double? QTY2 { get; set; }

        public double? QTY3 { get; set; }

        public double? QTY4 { get; set; }

        public double? QTY5 { get; set; }

        public double? QTY6 { get; set; }

        public double? QTY7 { get; set; }

        public double? QTY8 { get; set; }

        public double? QTY9 { get; set; }

        public double? QTY10 { get; set; }

        public double? QTY11 { get; set; }

        public double? QTY12 { get; set; }

        public double? QTY13 { get; set; }

        public double? QTY14 { get; set; }

        public double? QTY15 { get; set; }

        public double? DISCOUNT { get; set; }

        public double? NILAI_DISC { get; set; }

        public bool WRITE_KONFIG { get; set; }

        [StringLength(20)]
        public string USER_NAME { get; set; }

        public DateTime TGL_INPUT { get; set; }

        public DateTime? TGL_KIRIM { get; set; }

        [StringLength(10)]
        public string LOKASI { get; set; }

        public double? DISCOUNT_2 { get; set; }

        public double? DISCOUNT_3 { get; set; }

        public double? DISCOUNT_4 { get; set; }

        public double? DISCOUNT_5 { get; set; }

        public double? NILAI_DISC_1 { get; set; }

        public double? NILAI_DISC_2 { get; set; }

        public double? NILAI_DISC_3 { get; set; }

        public double? NILAI_DISC_4 { get; set; }

        public double? NILAI_DISC_5 { get; set; }

        [StringLength(10)]
        public string SORT1 { get; set; }

        [StringLength(10)]
        public string SORT2 { get; set; }

        [StringLength(10)]
        public string SORT3 { get; set; }

        [StringLength(10)]
        public string SORT4 { get; set; }
    }
}
