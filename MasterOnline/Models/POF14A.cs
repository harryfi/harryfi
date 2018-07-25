namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF14A
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string DIV { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 2)]
        public int NO_URUT { get; set; }

        [Key]
        [Column(Order = 3)]
        public double TAHUN { get; set; }

        public int? STN { get; set; }

        [Required]
        [StringLength(20)]
        public string USERNAME { get; set; }

        public DateTime TGLINPUT { get; set; }
    }
}
