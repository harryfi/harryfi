namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ART02B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string NOBUK { get; set; }

        [Key]
        [Column(Order = 1)]
        public int NO { get; set; }

        [StringLength(10)]
        public string FAKTUR { get; set; }

        public double SISA { get; set; }

        public double NILAI { get; set; }

        public double NPPN { get; set; }

        [StringLength(1)]
        public string DK { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public virtual ART02 ART02 { get; set; }
    }
}
