namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF16
    {
        [Key]
        [StringLength(3)]
        public string Jenis { get; set; }

        [StringLength(30)]
        public string Ket { get; set; }

        public double LT { get; set; }

        public double OC { get; set; }

        public double SS { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
