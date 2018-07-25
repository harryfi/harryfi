namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFREK2
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LKS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string KODE { get; set; }

        [StringLength(15)]
        public string KODE2 { get; set; }

        [StringLength(1)]
        public string JR { get; set; }

        [StringLength(15)]
        public string REK_KONSOL { get; set; }
    }
}
