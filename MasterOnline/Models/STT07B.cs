namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT07B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string NO_BUKTI { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string BRG { get; set; }

        public bool? SN { get; set; }

        [StringLength(255)]
        public string NAMA { get; set; }

        [StringLength(10)]
        public string GD { get; set; }

        public double? QTY { get; set; }
    }
}
