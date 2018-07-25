namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFKEL")]
    public partial class GLFKEL
    {
        [Key]
        [StringLength(10)]
        public string KEL { get; set; }

        [StringLength(30)]
        public string KET { get; set; }
    }
}
