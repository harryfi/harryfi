namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFCAB_A
    {
        public short LKS { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string NAMA { get; set; }

        [Key]
        [Column(Order = 1)]
        public int XXXX { get; set; }
    }
}
