namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT03
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string NO_MATRIK { get; set; }

        [Key]
        [Column(Order = 1)]
        public int NO_URUT { get; set; }

        [StringLength(10)]
        public string POP_NO { get; set; }

        [StringLength(20)]
        public string ITEM_NO { get; set; }

        [StringLength(50)]
        public string ITEM_DESCRIPTION { get; set; }

        [StringLength(3)]
        public string VALUTA { get; set; }

        public byte? STN { get; set; }

        public double UNIT { get; set; }

        [StringLength(10)]
        public string KODE_SUPPLIER1 { get; set; }

        [StringLength(10)]
        public string KODE_SUPPLIER2 { get; set; }

        [StringLength(10)]
        public string KODE_SUPPLIER3 { get; set; }

        [StringLength(10)]
        public string KODE_SUPPLIER4 { get; set; }

        [StringLength(50)]
        public string NAMA_SUPPLIER1 { get; set; }

        [StringLength(50)]
        public string NAMA_SUPPLIER2 { get; set; }

        [StringLength(50)]
        public string NAMA_SUPPLIER3 { get; set; }

        [StringLength(50)]
        public string NAMA_SUPPLIER4 { get; set; }

        public double HARGA_SUPPLIER1 { get; set; }

        public double HARGA_SUPPLIER2 { get; set; }

        public double HARGA_SUPPLIER3 { get; set; }

        public double HARGA_SUPPLIER4 { get; set; }

        public double JUMLAH_SUPPLIER1 { get; set; }

        public double JUMLAH_SUPPLIER2 { get; set; }

        public double JUMLAH_SUPPLIER3 { get; set; }

        public double JUMLAH_SUPPLIER4 { get; set; }

        public double DISC_SUPPLIER1 { get; set; }

        public double DISC_SUPPLIER2 { get; set; }

        public double DISC_SUPPLIER3 { get; set; }

        public double DISC_SUPPLIER4 { get; set; }
    }
}
