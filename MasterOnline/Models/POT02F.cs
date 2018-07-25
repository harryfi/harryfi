namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT02F
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string POP_NO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NO_URUT_UNIT { get; set; }

        [Key]
        [Column(Order = 2)]
        public int NO_URUT { get; set; }

        [StringLength(10)]
        public string NO_SO { get; set; }

        public double QTY { get; set; }

        public double QTY_TERIMA_SO { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
