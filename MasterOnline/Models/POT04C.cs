namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT04C
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string PO_NO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(5)]
        public string BIAYA { get; set; }

        public double? NILAI { get; set; }

        public double? PERSEN { get; set; }

        public double? TOT_NILAI { get; set; }

        public double? NILAI_PERSEN { get; set; }

        [StringLength(255)]
        public string KET { get; set; }

        public double? KARTON { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(10)]
        public string PO_BRG { get; set; }

        [Key]
        [Column(Order = 4)]
        public double NO_URUT { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
