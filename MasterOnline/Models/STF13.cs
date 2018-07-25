namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF13
    {
        [Key]
        [StringLength(2)]
        public string Kode { get; set; }

        [StringLength(30)]
        public string Ket { get; set; }

        [StringLength(1)]
        public string MK { get; set; }

        [StringLength(3)]
        public string NO_SERI { get; set; }

        [StringLength(1)]
        public string JRef { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
