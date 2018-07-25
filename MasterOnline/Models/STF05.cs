namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF05
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(4)]
        public string Gd { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string Brg { get; set; }

        public double Qty { get; set; }
    }
}
