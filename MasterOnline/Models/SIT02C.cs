namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIT02C
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(5)]
        public string KD_KASIR { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime TGL { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(5)]
        public string VLT { get; set; }

        public double? NTUKAR { get; set; }

        public double? NTUNAI { get; set; }

        public double? HOME_CURR { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
