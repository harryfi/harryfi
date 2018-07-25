namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF13
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string SUPP { get; set; }

        [Key]
        [Column(Order = 1)]
        public double TYPE { get; set; }

        public double DISC { get; set; }

        [Required]
        [StringLength(20)]
        public string USERNAME { get; set; }

        public DateTime TGLINPUT { get; set; }
    }
}
