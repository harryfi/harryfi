namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBF04
    {
        [Required]
        [StringLength(10)]
        public string SUPP { get; set; }

        [Required]
        [StringLength(10)]
        public string SUPP_PPN { get; set; }

        [Required]
        [StringLength(50)]
        public string NAMA_SUPP { get; set; }

        [Key]
        public int RECNUM { get; set; }
    }
}
