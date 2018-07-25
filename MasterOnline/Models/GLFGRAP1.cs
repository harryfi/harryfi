namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFGRAP1
    {
        [Key]
        [StringLength(2)]
        public string KODE { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(1)]
        public string J_GRAP { get; set; }

        public short LKS { get; set; }

        [StringLength(15)]
        public string REK_AGR1 { get; set; }

        [StringLength(15)]
        public string REK_AGR2 { get; set; }

        [StringLength(15)]
        public string REK1 { get; set; }

        [StringLength(15)]
        public string REK2 { get; set; }
    }
}
