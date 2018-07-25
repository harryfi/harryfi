namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT05D
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string NO_PENAWARAN { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string KD_SUPP { get; set; }

        public bool? PILIH { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
