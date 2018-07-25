namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT05B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string NO_PENAWARAN { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string NO_PP { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
