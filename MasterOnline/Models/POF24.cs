namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF24
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string USERNAME { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(1)]
        public string LEVEL { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string DIVISI { get; set; }

        [Column(TypeName = "text")]
        public string PARAF { get; set; }

        [Column(TypeName = "image")]
        public byte[] PHOTO { get; set; }

        [Required]
        [StringLength(255)]
        public string Al_Gbr { get; set; }
    }
}
