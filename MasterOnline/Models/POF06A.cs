namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF06A
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string BUYER_CODE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(1)]
        public string LEVEL_SORT { get; set; }

        public DateTime TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }
    }
}
