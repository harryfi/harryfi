namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFDEPT")]
    public partial class GLFDEPT
    {
        [Key]
        [StringLength(10)]
        public string DEPT { get; set; }

        [StringLength(30)]
        public string KET { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
