namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF23
    {
        [Key]
        [StringLength(15)]
        public string KODE_BANK { get; set; }

        [Required]
        [StringLength(50)]
        public string BANK { get; set; }

        [StringLength(20)]
        public string Keterangan { get; set; }
    }
}
