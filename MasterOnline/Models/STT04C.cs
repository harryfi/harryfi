namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT04C
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(4)]
        public string Gd { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string Brg { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(20)]
        public string Lot_No { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(20)]
        public string Batch_No { get; set; }

        public double Qty { get; set; }

        public DateTime? EX_DATE { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
