namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF11D
    {
        [Key]
        [StringLength(20)]
        public string BRG { get; set; }

        public double QPO_MSK { get; set; }

        public double QPO_KLR { get; set; }
    }
}
