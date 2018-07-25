namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF06
    {
        [Key]
        [StringLength(10)]
        public string LKS { get; set; }

        [StringLength(30)]
        public string KET { get; set; }

        public double KAPASITAS { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
