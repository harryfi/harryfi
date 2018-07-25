namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFVAL2
    {
        [Key]
        [Column(Order = 0)]
        public DateTime TGL { get; set; }

        public double TUKAR { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(5)]
        public string KURS { get; set; }
    }
}
