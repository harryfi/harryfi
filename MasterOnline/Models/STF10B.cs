namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF10B
    {
        [StringLength(4)]
        public string GD { get; set; }

        [StringLength(20)]
        public string BRG { get; set; }

        [StringLength(20)]
        public string WO { get; set; }

        public DateTime TGL { get; set; }

        public short QTY { get; set; }

        public short HPOKOK { get; set; }

        [Key]
        public int RECNUM { get; set; }
    }
}
