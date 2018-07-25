namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF02G
    {
        [Key]
        public int RecNum { get; set; }

        public int IDMARKET { get; set; }

        [Required]
        [StringLength(50)]
        public string AKUNMARKET { get; set; }

        [Required]
        [StringLength(100)]
        public string USERNAME { get; set; }

        [StringLength(50)]
        public string NAMAMARKET { get; set; }
    }
}
