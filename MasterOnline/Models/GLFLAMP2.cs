namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFLAMP2
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(2)]
        public string KODE_BARIS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(2)]
        public string KODE { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(1)]
        public string CETAK_TKEL { get; set; }

        [StringLength(1)]
        public string JR { get; set; }

        [StringLength(1)]
        public string HTK { get; set; }

        [StringLength(1)]
        public string CETAK_GTOT { get; set; }

        [StringLength(30)]
        public string NA_GTOT { get; set; }

        [StringLength(1)]
        public string AWAL { get; set; }

        public bool KONSOLIDASI { get; set; }

        [StringLength(30)]
        public string NAMA_GRANDTOTAL { get; set; }

        [StringLength(30)]
        public string NAMA_GT_KEL { get; set; }

        [StringLength(1)]
        public string HIT_GT_KEL { get; set; }

        [StringLength(1)]
        public string JR_GT_KEL { get; set; }

        [StringLength(1)]
        public string CETAK_GT_KEL { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }
    }
}
