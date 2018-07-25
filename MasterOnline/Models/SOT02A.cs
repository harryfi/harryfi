namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SOT02A
    {
        [Key]
        [StringLength(10)]
        public string NOBUKTI { get; set; }

        public DateTime TGL { get; set; }

        public DateTime TGL_KIRIM { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(40)]
        public string NAMA_CUST { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        public double? NILAI_TUKAR { get; set; }

        [StringLength(5)]
        public string KODE_SALES { get; set; }

        [StringLength(5)]
        public string KODE_WIL { get; set; }

        [StringLength(5)]
        public string KODE_ALAMAT { get; set; }

        [Column(TypeName = "text")]
        public string KET { get; set; }

        public double? DISCOUNT { get; set; }

        public double? NILAI_DISC { get; set; }

        public double? PPN { get; set; }

        public double? NILAI_PPN { get; set; }

        public double? BRUTO { get; set; }

        public double? NETTO { get; set; }

        [StringLength(20)]
        public string USER_NAME { get; set; }

        public DateTime TGL_INPUT { get; set; }

        public int? PRINT_COUNT { get; set; }

        public bool KIRIM_PENUH { get; set; }

        public bool RETUR_PENUH { get; set; }

        [StringLength(40)]
        public string AL { get; set; }

        [StringLength(40)]
        public string AL1 { get; set; }

        [StringLength(40)]
        public string AL2 { get; set; }

        [StringLength(40)]
        public string AL3 { get; set; }

        [StringLength(40)]
        public string AL_CUST { get; set; }

        public double? U_MUKA { get; set; }

        public double? TERM { get; set; }

        [StringLength(10)]
        public string CUST_QQ { get; set; }

        [StringLength(1)]
        public string HARGA_FRANCO { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public double? TOTAL_TITIPAN { get; set; }

        [StringLength(20)]
        public string SUPP { get; set; }
    }
}
