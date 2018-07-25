namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class APT01B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string INV { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short NO { get; set; }

        [StringLength(20)]
        public string BRG { get; set; }

        public double HS { get; set; }

        public double QTY { get; set; }

        public double DISC2 { get; set; }

        public double NDISC2 { get; set; }

        [StringLength(2)]
        public string GD { get; set; }

        [StringLength(1)]
        public string STN { get; set; }

        [StringLength(30)]
        public string NBRG { get; set; }
    }
}
