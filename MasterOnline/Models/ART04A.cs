namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ART04A
    {
        [Key]
        [StringLength(10)]
        public string NOBUK { get; set; }

        public DateTime TGL { get; set; }

        [Required]
        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(1000)]
        public string KET { get; set; }

        [StringLength(1)]
        public string POSTING { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        public double? TUKAR { get; set; }

        public double? TKOREKSI { get; set; }

        public double? TPPN { get; set; }

        [Required]
        [StringLength(50)]
        public string NCUST { get; set; }

        public double? TOTAL_DEBET_GL { get; set; }

        public double? TOTAL_KREDIT_GL { get; set; }

        [Required]
        [StringLength(20)]
        public string USERNAME { get; set; }

        public DateTime TGLINPUT { get; set; }
    }
}
