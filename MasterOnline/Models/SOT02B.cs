namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SOT02B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(15)]
        public string NO_BUKTI { get; set; }

        [Key]
        [Column(Order = 1)]
        public int NO_URUT { get; set; }

        [StringLength(20)]
        public string BRG { get; set; }

        [Required]
        [StringLength(1)]
        public string STN { get; set; }

        public double H_SATUAN { get; set; }

        public double? QTY { get; set; }

        public double? DISCOUNT { get; set; }

        public double? NILAI_DISC { get; set; }

        public double? DISCOUNT_2 { get; set; }

        public double? DISCOUNT_3 { get; set; }

        public double? DISCOUNT_4 { get; set; }

        public double? DISCOUNT_5 { get; set; }

        public double? NILAI_DISC_1 { get; set; }

        public double? NILAI_DISC_2 { get; set; }

        public double? NILAI_DISC_3 { get; set; }

        public double? NILAI_DISC_4 { get; set; }

        public double? NILAI_DISC_5 { get; set; }

        public double? HARGA { get; set; }

        public bool WRITE_KONFIG { get; set; }

        [Required]
        [StringLength(20)]
        public string USER_NAME { get; set; }

        public DateTime? TGL_INPUT { get; set; }

        [Required]
        [StringLength(10)]
        public string LOKASI { get; set; }

        public DateTime? TGL_KIRIM { get; set; }

        [Required]
        [StringLength(200)]
        public string CATATAN { get; set; }

        [StringLength(20)]
        public string BRG_CUST { get; set; }

        public double? NTITIPAN { get; set; }

        public double? DISC_TITIPAN { get; set; }
    }
}
