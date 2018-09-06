namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF12
    {
        [Key]
        [StringLength(10)]
        public string CUST_QQ { get; set; }

        [StringLength(40)]
        public string NAMA { get; set; }

        [StringLength(40)]
        public string AL { get; set; }

        [StringLength(40)]
        public string AL2 { get; set; }

        [StringLength(40)]
        public string AL3 { get; set; }

        [StringLength(30)]
        public string TLP { get; set; }

        [StringLength(30)]
        public string PERSON { get; set; }

        [StringLength(15)]
        public string NPWP { get; set; }

        [StringLength(3)]
        public string WIL { get; set; }

        [StringLength(3)]
        public string SLM { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        public DateTime? TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        [StringLength(1)]
        public string JKEL { get; set; }

        [StringLength(1)]
        public string AGAMA { get; set; }

        public DateTime? TGLLAHIR { get; set; }

        [StringLength(30)]
        public string FAX { get; set; }

        [StringLength(50)]
        public string EMAIL { get; set; }

        [StringLength(50)]
        public string CATAT1 { get; set; }

        [StringLength(50)]
        public string CATAT2 { get; set; }

        [StringLength(30)]
        public string NAWIL { get; set; }
    }
}
