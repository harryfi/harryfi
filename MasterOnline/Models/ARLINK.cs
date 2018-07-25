namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ARLINK")]
    public partial class ARLINK
    {
        [Key]
        [StringLength(5)]
        public string KLINK { get; set; }

        [StringLength(15)]
        public string KAS { get; set; }

        [StringLength(15)]
        public string BANK { get; set; }

        [StringLength(15)]
        public string PJL { get; set; }

        [StringLength(15)]
        public string PIUT { get; set; }

        [StringLength(15)]
        public string DISC { get; set; }

        [StringLength(15)]
        public string PPN { get; set; }

        [StringLength(15)]
        public string MATERAI { get; set; }

        [StringLength(15)]
        public string POT { get; set; }

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
        public string SEDIA { get; set; }

        [StringLength(15)]
        public string HPP { get; set; }

        public int LKS { get; set; }

        [StringLength(15)]
        public string JURNAL { get; set; }

        [StringLength(10)]
        public string DEPT { get; set; }

        [StringLength(10)]
        public string COST { get; set; }

        [StringLength(15)]
        public string PPNBM { get; set; }

        [StringLength(40)]
        public string KET { get; set; }

        [StringLength(15)]
        public string RETENSI { get; set; }

        [StringLength(15)]
        public string PIUT_SEMENTARA { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
