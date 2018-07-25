namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("APLINK")]
    public partial class APLINK
    {
        [Key]
        [StringLength(5)]
        public string KLINK { get; set; }

        [StringLength(15)]
        public string KAS { get; set; }

        [StringLength(15)]
        public string BANK { get; set; }

        [StringLength(15)]
        public string HUTA { get; set; }

        [StringLength(15)]
        public string DISC { get; set; }

        [StringLength(15)]
        public string PPN { get; set; }

        [StringLength(15)]
        public string POT { get; set; }

        [StringLength(15)]
        public string SEDIA { get; set; }

        [StringLength(15)]
        public string MUKA { get; set; }

        [StringLength(15)]
        public string GIRO { get; set; }

        [StringLength(15)]
        public string KREDIT { get; set; }

        [StringLength(15)]
        public string DEBET { get; set; }

        [StringLength(15)]
        public string RETUR { get; set; }

        [StringLength(15)]
        public string KURS { get; set; }

        [StringLength(15)]
        public string BELI { get; set; }

        [StringLength(15)]
        public string HPP { get; set; }

        public int LKS { get; set; }

        [StringLength(2)]
        public string JURNAL { get; set; }

        [StringLength(10)]
        public string DEPT { get; set; }

        [StringLength(15)]
        public string COST { get; set; }

        [StringLength(15)]
        public string PPH22 { get; set; }

        [StringLength(15)]
        public string HUTANG_PO { get; set; }

        [StringLength(15)]
        public string PERSEDIAAN_PO { get; set; }

        [StringLength(40)]
        public string Ket { get; set; }

        [StringLength(15)]
        public string APTemp { get; set; }

        [StringLength(15)]
        public string BIAYA_TITIPAN { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
