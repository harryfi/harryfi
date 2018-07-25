namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF16B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(5)]
        public string BIAYA { get; set; }

        public double? NILAI { get; set; }

        public double? PERSEN { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
