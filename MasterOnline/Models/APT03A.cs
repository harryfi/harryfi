namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class APT03A
    {
        [Key]
        [StringLength(10)]
        public string BUKTI { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime TGL { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [StringLength(10)]
        public string SUPP { get; set; }

        public double MUKA1 { get; set; }

        public double MUKA2 { get; set; }

        public double MUKA3 { get; set; }

        [Column(TypeName = "ntext")]
        public string KET { get; set; }

        [StringLength(1)]
        public string POSTING { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        public double TUKAR { get; set; }

        public double TBAYAR { get; set; }

        public double TPOT { get; set; }

        public short TGIRO { get; set; }

        [StringLength(30)]
        public string NSUPP { get; set; }

        public double TOTAL_DEBET_GL { get; set; }

        public double TOTAL_KREDIT_GL { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public DateTime? TGLINPUT { get; set; }
    }
}
