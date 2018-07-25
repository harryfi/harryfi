namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF16A
    {
        [Key]
        [StringLength(20)]
        public string BRG { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
