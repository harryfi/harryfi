namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF17A
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(5)]
        public string GRUP { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime TGL1 { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime TGL2 { get; set; }

        [StringLength(40)]
        public string KET { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
