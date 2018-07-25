namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ART01D
    {
        [Key]
        [StringLength(10)]
        public string FAKTUR { get; set; }

        public DateTime TGL { get; set; }

        [StringLength(1)]
        public string KONTAN { get; set; }

        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(3)]
        public string SLM { get; set; }

        public short TERM { get; set; }

        public double? MUKA { get; set; }

        public double? DISC1 { get; set; }

        public double? NDISC1 { get; set; }

        public double? PPN { get; set; }

        public double? MTR { get; set; }

        public double? NETTO { get; set; }

        [Column(TypeName = "text")]
        public string KET { get; set; }

        public double? TOTAL { get; set; }

        [StringLength(10)]
        public string F_PAJAK { get; set; }

        public DateTime? TGL_PAJAK { get; set; }

        [StringLength(30)]
        public string NCUST { get; set; }

        [StringLength(3)]
        public string WIL { get; set; }

        [StringLength(10)]
        public string SO { get; set; }

        [StringLength(1)]
        public string SATUAN { get; set; }

        [StringLength(1)]
        public string POST { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        public double? TUKAR { get; set; }

        public DateTime JTGL { get; set; }

        public double? NPPN { get; set; }

        public double? POT { get; set; }

        public double? BAYAR { get; set; }

        public double? DEBET { get; set; }

        public double? KREDIT { get; set; }

        public double? BANK { get; set; }

        public double? TOTAL_DEBET_GL { get; set; }

        public double? TOTAL_KREDIT_GL { get; set; }

        public double? PPN_RATE { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public DateTime? TGLINPUT { get; set; }

        [StringLength(50)]
        public string RANGKA { get; set; }

        [StringLength(50)]
        public string MESIN { get; set; }

        public double? TAHUN { get; set; }

        public double NILAI_PROYEK { get; set; }

        public double NFAKTUR_RETENSI { get; set; }

        public double FAKTUR_RETENSI { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }
    }
}
