namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIT02A
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(5)]
        public string KD_KASIR { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime TANGGAL { get; set; }

        public double? CREDIT { get; set; }

        public double? DEBIT { get; set; }

        public double? GIRO { get; set; }

        public double? POTONGAN { get; set; }

        public double? PIUTANG { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
