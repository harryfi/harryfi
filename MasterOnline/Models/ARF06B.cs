namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF06B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string KodeGroup { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string KodeCust { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
