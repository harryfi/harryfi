namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF07
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string USERNAME { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string GUDANG { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(2)]
        public string KODE { get; set; }
    }
}
