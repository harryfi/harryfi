namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBF02
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENIS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(3)]
        public string SERI { get; set; }

        public double NO_AKHIR { get; set; }

        [StringLength(2)]
        public string KODE_PAJAK { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
