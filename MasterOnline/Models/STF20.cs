namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF20
    {
        [Key]
        [Column(Order = 0)]
        public string CATEGORY_MO { get; set; }

        [Key]
        [Column(Order = 1)]
        public int LEVEL_JUDUL_VAR { get; set; }

        public string VALUE_JUDUL_VAR { get; set; }
    }
}
