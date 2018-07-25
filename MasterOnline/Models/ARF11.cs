namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF11
    {
        [Key]
        [StringLength(3)]
        public string VLT { get; set; }

        [StringLength(30)]
        public string KET { get; set; }

        public double TUKAR { get; set; }
    }
}
