namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIT03C
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string NO_BUKTI { get; set; }

        public double NILAI { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string KODE_KARTU { get; set; }

        public DateTime? EXP_DATE { get; set; }

        [StringLength(20)]
        public string NO_KARTU_KREDIT { get; set; }

        [StringLength(50)]
        public string Bank { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
