namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class APT01D
    {
        [Key]
        [StringLength(10)]
        public string INV { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime TGL { get; set; }

        [StringLength(10)]
        public string SUPP { get; set; }

        [StringLength(1)]
        public string KONTAN { get; set; }

        public double TERM { get; set; }

        public double MUKA { get; set; }

        public double DISC1 { get; set; }

        public double NDISC1 { get; set; }

        public double PPN { get; set; }

        [Column(TypeName = "ntext")]
        public string KET { get; set; }

        public double TOTAL { get; set; }

        [StringLength(10)]
        public string PO { get; set; }

        [StringLength(1)]
        public string SATUAN { get; set; }

        [StringLength(2)]
        public string KODE_JURNAL { get; set; }

        public double NETTO { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        public double TUKAR { get; set; }

        [StringLength(1)]
        public string POSTING { get; set; }

        public double NPPN { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime JTGL { get; set; }

        public double BAYAR { get; set; }

        public double DEBET { get; set; }

        public double KREDIT { get; set; }

        [StringLength(20)]
        public string F_PAJAK { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? TGL_PAJAK { get; set; }

        [StringLength(30)]
        public string NSUPP { get; set; }

        public double TOTAL_DEBET_GL { get; set; }

        public double TOTAL_KREDIT_GL { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? TGL_INV_2 { get; set; }

        [StringLength(50)]
        public string INV_2 { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        public DateTime? TGLINPUT { get; set; }

        [StringLength(50)]
        public string RANGKA { get; set; }

        [StringLength(50)]
        public string MESIN { get; set; }

        public double? TAHUN { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }
    }
}
