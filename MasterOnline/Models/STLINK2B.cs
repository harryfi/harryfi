namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STLINK2B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string KLINK { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(2)]
        public string KODE { get; set; }

        [StringLength(30)]
        public string KET { get; set; }

        [StringLength(1)]
        public string MK { get; set; }

        [StringLength(15)]
        public string DEBET { get; set; }

        [StringLength(15)]
        public string KREDIT { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
