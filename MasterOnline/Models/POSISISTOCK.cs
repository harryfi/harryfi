namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("POSISISTOCK")]
    public partial class POSISISTOCK
    {
        [Key]
        public int NO { get; set; }

        [StringLength(20)]
        public string BRG { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(5)]
        public string STN { get; set; }

        public double? QOHGDQOH { get; set; }

        public double? QOHGDSEMUA { get; set; }

        public double? QSO { get; set; }

        public double? QPO { get; set; }

        public double? QPROD { get; set; }

        public double? QSALESQOH { get; set; }

        public double? QSALESSEMUA { get; set; }
    }
}
