namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_SIT01B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENIS_FORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string NO_BUKTI { get; set; }

        [Key]
        [Column(Order = 2)]
        public int NO_URUT { get; set; }

        [StringLength(20)]
        public string BRG { get; set; }

        public double? H_SATUAN { get; set; }

        [StringLength(1)]
        public string SATUAN { get; set; }

        [Key]
        [Column(Order = 3)]
        public double QTY { get; set; }

        [StringLength(4)]
        public string GUDANG { get; set; }

        public double? DISCOUNT { get; set; }

        public double? NILAI_DISC { get; set; }

        public double? HARGA { get; set; }

        public double? QTY_KIRIM { get; set; }

        [StringLength(1)]
        public string AUTO_LOAD { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        public DateTime? TGLINPUT { get; set; }

        public double? QTY_RETUR { get; set; }

        [Key]
        [Column(Order = 4)]
        public bool WRITE_KONFIG { get; set; }

        public double? DISCOUNT_2 { get; set; }

        public double? DISCOUNT_3 { get; set; }

        public double? DISCOUNT_4 { get; set; }

        public double? DISCOUNT_5 { get; set; }

        public double? NILAI_DISC_1 { get; set; }

        public double? NILAI_DISC_2 { get; set; }

        public double? NILAI_DISC_3 { get; set; }

        public double? NILAI_DISC_4 { get; set; }

        public double? NILAI_DISC_5 { get; set; }

        public double? TOTAL_LOT { get; set; }

        public double? TOTAL_QTY { get; set; }

        public DateTime? TGL_KIRIM { get; set; }

        [Key]
        [Column(Order = 5)]
        public double NO_URUT_SO { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(255)]
        public string CATATAN { get; set; }
    }
}
