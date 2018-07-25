namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tmp_STF09A
    {
        [StringLength(20)]
        public string brg { get; set; }

        [StringLength(1)]
        public string mk { get; set; }

        [StringLength(2)]
        public string jtran { get; set; }

        [Key]
        [Column(Order = 0)]
        public double qty { get; set; }

        [Key]
        [Column(Order = 1)]
        public double Nilai { get; set; }
    }
}
