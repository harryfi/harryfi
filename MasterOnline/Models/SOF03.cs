namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SOF03
    {
        [Key]
        [StringLength(20)]
        public string BRG { get; set; }

        public double? QTY { get; set; }
    }
}
