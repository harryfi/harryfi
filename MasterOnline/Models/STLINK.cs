namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STLINK")]
    public partial class STLINK
    {
        [Key]
        [StringLength(5)]
        public string KLink { get; set; }

        public short LKS { get; set; }

        [StringLength(2)]
        public string Jurnal { get; set; }

        [StringLength(15)]
        public string Sedia { get; set; }

        [StringLength(15)]
        public string HPP { get; set; }

        [StringLength(15)]
        public string WIP { get; set; }

        [StringLength(15)]
        public string Koreksi_D { get; set; }

        [StringLength(15)]
        public string Koreksi_K { get; set; }

        [StringLength(15)]
        public string PJL { get; set; }

        [StringLength(15)]
        public string Retur { get; set; }

        [StringLength(15)]
        public string Beli { get; set; }

        [StringLength(10)]
        public string DEPT { get; set; }

        [StringLength(10)]
        public string CC { get; set; }

        [StringLength(15)]
        public string DISC_JUAL { get; set; }

        [StringLength(15)]
        public string DISC_BELI { get; set; }

        [StringLength(40)]
        public string Ket { get; set; }

        [StringLength(15)]
        public string BIAYA_QTY { get; set; }

        [StringLength(15)]
        public string COGM { get; set; }

        [StringLength(15)]
        public string FOH { get; set; }

        [StringLength(15)]
        public string DISC_JUAL_1 { get; set; }

        [StringLength(15)]
        public string DISC_JUAL_2 { get; set; }

        [StringLength(15)]
        public string DISC_JUAL_3 { get; set; }

        [StringLength(15)]
        public string DISC_JUAL_4 { get; set; }

        [StringLength(15)]
        public string DISC_JUAL_5 { get; set; }

        [StringLength(15)]
        public string PPNBM { get; set; }

        [StringLength(15)]
        public string GIT_SALES { get; set; }

        [StringLength(15)]
        public string SUSUT { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
