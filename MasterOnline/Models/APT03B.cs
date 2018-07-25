namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class APT03B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string BUKTI { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int? NO { get; set; }

        [StringLength(10)]
        public string NINV { get; set; }

        public double BAYAR { get; set; }

        public double POT { get; set; }

        public double SISA { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
