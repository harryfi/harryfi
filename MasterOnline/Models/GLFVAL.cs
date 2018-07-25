namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFVAL")]
    public partial class GLFVAL
    {
        [Key]
        [StringLength(5)]
        public string kurs { get; set; }

        [StringLength(30)]
        public string ket { get; set; }

        public double ntukar { get; set; }
    }
}
