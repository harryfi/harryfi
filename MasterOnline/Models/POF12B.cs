namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF12B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string SUPP { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string BRG { get; set; }

        [StringLength(1)]
        public string JENIS { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(5)]
        public string STN { get; set; }

        [StringLength(5)]
        public string STN2 { get; set; }

        public double DISC1_BESAR { get; set; }

        public double DISC2_BESAR { get; set; }

        public double DISC3_BESAR { get; set; }

        public double DISC1_KECIL { get; set; }

        public double DISC2_KECIL { get; set; }

        public double DISC3_KECIL { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
