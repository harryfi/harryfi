namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF11B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(15)]
        public string MPS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string BRG { get; set; }

        public double QPO_MSK { get; set; }

        public double QPO_KLR { get; set; }

        public double QPROD_MSK { get; set; }

        public double QPROD_KLR { get; set; }

        public double QSO_MSK { get; set; }

        public double QSO_KLR { get; set; }

        public DateTime? TGL { get; set; }
    }
}
