namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SDF08
    {
        [Key]
        [StringLength(3)]
        public string SLM { get; set; }

        [StringLength(4)]
        public string GD { get; set; }
    }
}
