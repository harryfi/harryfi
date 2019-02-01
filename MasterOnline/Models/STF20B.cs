namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF20B
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }

        public string CATEGORY_MO { get; set; }
        public int LEVEL_VAR { get; set; }
        public string KODE_VAR { get; set; }
        public string KET_VAR { get; set; }
    }
}
