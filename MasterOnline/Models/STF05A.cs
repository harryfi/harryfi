namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF05A
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(4)]
        public string GD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime TGL { get; set; }

        public short MASUK { get; set; }

        public short KELUAR { get; set; }
    }
}
