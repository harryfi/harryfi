namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF15B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(5)]
        public string GRUP { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(4)]
        public string Kode_Gudang { get; set; }

        [StringLength(3)]
        public string KDHARGA { get; set; }

        [StringLength(10)]
        public string KDDISC { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
