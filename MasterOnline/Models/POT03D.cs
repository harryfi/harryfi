namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT03D
    {
        [Key]
        [Column(Order = 0)]
        public int RecNum { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string Matrix { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(20)]
        public string Brg { get; set; }

        public int? Supp_Ke { get; set; }

        [StringLength(30)]
        public string Merek { get; set; }

        public double? Harga { get; set; }

        public bool? ST_PILIH { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
