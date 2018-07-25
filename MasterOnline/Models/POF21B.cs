namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF21B
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string Group { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string Kobar { get; set; }

        public int? STN { get; set; }

        public double? Qty { get; set; }
    }
}
