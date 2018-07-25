namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIT01B1
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

        public double? H_SATUAN { get; set; }

        [StringLength(1)]
        public string SATUAN { get; set; }

        [Required]
        [StringLength(1)]
        public string JENIS_SIZE { get; set; }

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

        [StringLength(4)]
        public string GUDANG { get; set; }

        public double? DISCOUNT { get; set; }

        public double? NILAI_DISC { get; set; }

        public double? HARGA { get; set; }

        [StringLength(1)]
        public string AUTO_LOAD { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        public DateTime? TGLINPUT { get; set; }

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

        public double NO_URUT_SO { get; set; }

        [Required]
        [StringLength(255)]
        public string CATATAN { get; set; }

        [StringLength(10)]
        public string SORT1 { get; set; }

        [StringLength(10)]
        public string SORT2 { get; set; }

        [StringLength(10)]
        public string SORT3 { get; set; }

        [StringLength(10)]
        public string SORT4 { get; set; }

        public double? QTY_BESAR { get; set; }

        public double? QTY_KECIL { get; set; }

        public double QTY_KIRIM1 { get; set; }

        public double QTY_KIRIM2 { get; set; }

        public double QTY_KIRIM3 { get; set; }

        public double QTY_KIRIM4 { get; set; }

        public double QTY_KIRIM5 { get; set; }

        public double QTY_KIRIM6 { get; set; }

        public double QTY_KIRIM7 { get; set; }

        public double QTY_KIRIM8 { get; set; }

        public double QTY_KIRIM9 { get; set; }

        public double QTY_KIRIM10 { get; set; }

        public double QTY_KIRIM11 { get; set; }

        public double QTY_KIRIM12 { get; set; }

        public double QTY_KIRIM13 { get; set; }

        public double QTY_KIRIM14 { get; set; }

        public double QTY_KIRIM15 { get; set; }

        public double QTY_RETUR1 { get; set; }

        public double QTY_RETUR2 { get; set; }

        public double QTY_RETUR3 { get; set; }

        public double QTY_RETUR4 { get; set; }

        public double QTY_RETUR5 { get; set; }

        public double QTY_RETUR6 { get; set; }

        public double QTY_RETUR7 { get; set; }

        public double QTY_RETUR8 { get; set; }

        public double QTY_RETUR9 { get; set; }

        public double QTY_RETUR10 { get; set; }

        public double QTY_RETUR11 { get; set; }

        public double QTY_RETUR12 { get; set; }

        public double QTY_RETUR13 { get; set; }

        public double QTY_RETUR14 { get; set; }

        public double QTY_RETUR15 { get; set; }

        public virtual SIT01A SIT01A { get; set; }
    }
}
