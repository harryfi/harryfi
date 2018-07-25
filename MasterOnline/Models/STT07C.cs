namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT07C
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string NO_BUKTI { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 2)]
        public int RECNUM { get; set; }

        [StringLength(50)]
        public string LOT_NO { get; set; }

        public bool? SN { get; set; }
    }
}
