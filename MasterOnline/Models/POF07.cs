namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF07
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string USERNAME { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(3)]
        public string VLT { get; set; }

        public double DR_RANGE { get; set; }

        public double SD_RANGE { get; set; }

        [Required]
        [StringLength(1)]
        public string LEVEL { get; set; }

        [Required]
        [StringLength(200)]
        public string PARAF { get; set; }

        [Column(TypeName = "image")]
        public byte[] PHOTO { get; set; }
    }
}
