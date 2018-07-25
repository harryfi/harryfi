namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF20A
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

        [StringLength(50)]
        public string KODE_EVENT { get; set; }
    }
}
