namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIT03B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string NO_BUKTI { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string NO_FAKTUR { get; set; }

        public DateTime? TANGGAL { get; set; }

        public double? NILAI { get; set; }

        public double? BAYAR { get; set; }

        [StringLength(5)]
        public string KD_KASIR { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
