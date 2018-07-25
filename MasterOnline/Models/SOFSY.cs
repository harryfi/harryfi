namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SOFSYS")]
    public partial class SOFSY
    {
        [Key]
        [StringLength(1)]
        public string METODA_NO { get; set; }

        [StringLength(1)]
        public string SATUAN { get; set; }

        [StringLength(2)]
        public string NS_SO { get; set; }

        public int NO_URUT_TERAKHIR { get; set; }

        [StringLength(1)]
        public string PROTEK_QOH { get; set; }

        [StringLength(1)]
        public string PROTEK_LIMIT { get; set; }

        [StringLength(1)]
        public string PROTEK_HSATUAN { get; set; }

        [StringLength(1)]
        public string H_JUAL_TERENDAH { get; set; }

        [StringLength(1)]
        public string KONFIGURASI_BRG { get; set; }

        public DateTime TGLINPUT { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        [StringLength(1)]
        public string VALIDASI_TOLERANSI_KIRIM { get; set; }

        public double TOLERANSI_KIRIM_SEMUABRG { get; set; }

        [StringLength(2)]
        public string NS_PENAWARAN { get; set; }

        public int NO_URUT_PENAWARAN { get; set; }

        [StringLength(1)]
        public string APPROVAL_SO { get; set; }

        public int? PROTEK_OVER_DUEDATE { get; set; }

        public double? TERM_OF_BOOKING { get; set; }

        public int? HARSAT_SESUAI_STN { get; set; }

        public int? INPUT_SATUAN { get; set; }

        public int? INQUERY_STOCK { get; set; }

        [Required]
        [StringLength(1)]
        public string SECURITY_SO { get; set; }

        [StringLength(1)]
        public string MFDesimal { get; set; }

        [StringLength(1)]
        public string QFDesimal { get; set; }

        public int? PROTEK_PO_CUST { get; set; }

        [StringLength(255)]
        public string DB_PATH { get; set; }

        [StringLength(1)]
        public string EXPIRED { get; set; }
    }
}
