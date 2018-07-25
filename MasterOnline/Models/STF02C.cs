namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF02C
    {
        [Key]
        [StringLength(20)]
        public string BRG { get; set; }

        [StringLength(5)]
        public string STN_3 { get; set; }

        [StringLength(5)]
        public string STN_4 { get; set; }

        public double ISI_STN3 { get; set; }

        public double ISI_STN4 { get; set; }

        public virtual STF02 STF02 { get; set; }
    }
}
