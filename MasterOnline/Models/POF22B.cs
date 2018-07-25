namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF22B
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string Supp { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string KdGroup { get; set; }
    }
}
