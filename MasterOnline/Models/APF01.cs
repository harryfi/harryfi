namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class APF01
    {
        [Key]
        [StringLength(10)]
        public string SUPP { get; set; }

        [StringLength(60)]
        public string NAMA { get; set; }

        [StringLength(100)]
        public string AL { get; set; }

        [StringLength(30)]
        public string AL2 { get; set; }

        [StringLength(30)]
        public string TLP { get; set; }

        [StringLength(30)]
        public string PERSO { get; set; }

        [StringLength(20)]
        public string NPWP { get; set; }

        public short TERM { get; set; }

        [StringLength(10)]
        public string KLINK { get; set; }

        public short KODE_CABANG { get; set; }

        [StringLength(12)]
        public string RekBca { get; set; }

        [StringLength(30)]
        public string NamaRekBca { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        public bool ADAPPN { get; set; }

        [StringLength(30)]
        public string AL3 { get; set; }

        [StringLength(30)]
        public string AL4 { get; set; }

        public bool STATUS_SUPP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
