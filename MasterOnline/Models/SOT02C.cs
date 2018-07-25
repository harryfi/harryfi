namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SOT02C
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(15)]
        public string NO_BUKTI { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NO_URUT_UNIT { get; set; }

        [Key]
        [Column(Order = 2)]
        public int NO_URUT { get; set; }

        [StringLength(20)]
        public string KODE_BRG_UNIT { get; set; }

        [StringLength(20)]
        public string KODE_BRG { get; set; }

        public double QTY { get; set; }

        [StringLength(1)]
        public string SATUAN { get; set; }

        public bool KONFIG_STANDART { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
