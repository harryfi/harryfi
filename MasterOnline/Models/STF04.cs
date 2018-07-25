namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF04
    {
        [StringLength(1)]
        public string TYPE { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(4)]
        public string GD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string BRG { get; set; }

        public short QAWAL { get; set; }

        public short TAHUN { get; set; }

        public short QM1 { get; set; }

        public short QM2 { get; set; }

        public short QM3 { get; set; }

        public short QM4 { get; set; }

        public short QM5 { get; set; }

        public short QM6 { get; set; }

        public short QM7 { get; set; }

        public short QM8 { get; set; }

        public short QM9 { get; set; }

        public short QM10 { get; set; }

        public short QM11 { get; set; }

        public short QM12 { get; set; }

        public short QM13 { get; set; }

        public short QM14 { get; set; }

        public short QM15 { get; set; }

        public short QM16 { get; set; }

        public short QM17 { get; set; }

        public short QM18 { get; set; }

        public short QM19 { get; set; }

        public short QM20 { get; set; }

        public short QM21 { get; set; }

        public short QM22 { get; set; }

        public short QM23 { get; set; }

        public short QM24 { get; set; }

        public short QM25 { get; set; }

        public short QM26 { get; set; }

        public short QM27 { get; set; }

        public short QM28 { get; set; }

        public short QM29 { get; set; }

        public short QM30 { get; set; }

        public short QM31 { get; set; }

        public short QK1 { get; set; }

        public short QK2 { get; set; }

        public short QK3 { get; set; }

        public short QK4 { get; set; }

        public short QK5 { get; set; }

        public short QK6 { get; set; }

        public short QK7 { get; set; }

        public short QK8 { get; set; }

        public short QK9 { get; set; }

        public short QK10 { get; set; }

        public short QK11 { get; set; }

        public short QK12 { get; set; }

        public short QK13 { get; set; }

        public short QK14 { get; set; }

        public short QK15 { get; set; }

        public short QK16 { get; set; }

        public short QK17 { get; set; }

        public short QK18 { get; set; }

        public short QK19 { get; set; }

        public short QK20 { get; set; }

        public short QK21 { get; set; }

        public short QK22 { get; set; }

        public short QK23 { get; set; }

        public short QK24 { get; set; }

        public short QK25 { get; set; }

        public short QK26 { get; set; }

        public short QK27 { get; set; }

        public short QK28 { get; set; }

        public short QK29 { get; set; }

        public short QK30 { get; set; }

        public short QK31 { get; set; }
    }
}
