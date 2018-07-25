namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF13
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string LEVEL { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string KODE { get; set; }

        [StringLength(40)]
        public string KET { get; set; }

        [StringLength(40)]
        public string Person { get; set; }
    }
}
