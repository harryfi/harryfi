namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF08B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(3)]
        public string SLM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string Kode { get; set; }

        [StringLength(40)]
        public string Ket { get; set; }
    }
}
