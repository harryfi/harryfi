namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBF01
    {
        [Key]
        [StringLength(5)]
        public string BIAYA { get; set; }

        [Required]
        [StringLength(40)]
        public string KET { get; set; }

        [Required]
        [StringLength(15)]
        public string REK { get; set; }
    }
}
