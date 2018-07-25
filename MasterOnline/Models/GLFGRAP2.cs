namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFGRAP2
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(2)]
        public string KODE { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short NO { get; set; }

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
