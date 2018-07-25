namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF21A
    {
        [Key]
        [StringLength(10)]
        public string Group { get; set; }

        [StringLength(30)]
        public string KET { get; set; }

        [Required]
        [StringLength(1)]
        public string PS { get; set; }
    }
}
