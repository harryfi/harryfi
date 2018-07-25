namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF12
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string LEVEL { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string KODE { get; set; }

        [Required]
        [StringLength(100)]
        public string KET { get; set; }
    }
}
