namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBT01G
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENISFORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string INV { get; set; }

        [Key]
        [Column(Order = 2)]
        public int NO_URUT { get; set; }

        [Required]
        [StringLength(10)]
        public string NO_INVOICE { get; set; }

        public double NPOTONG { get; set; }

        public double NINVOICE { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
