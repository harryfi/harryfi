namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF08B
    {
        [StringLength(4)]
        public string GD { get; set; }

        [StringLength(20)]
        public string BRG { get; set; }

        public short Tahun { get; set; }

        public double QAwal { get; set; }

        public double NAwal { get; set; }

        [StringLength(5)]
        public string STN { get; set; }

        public double QM1 { get; set; }

        public double QM2 { get; set; }

        public double QM3 { get; set; }

        public double QM4 { get; set; }

        public double QM5 { get; set; }

        public double QM6 { get; set; }

        public double QM7 { get; set; }

        public double QM8 { get; set; }

        public double QM9 { get; set; }

        public double QM10 { get; set; }

        public double QM11 { get; set; }

        public double QM12 { get; set; }

        public double NM1 { get; set; }

        public double NM2 { get; set; }

        public double NM3 { get; set; }

        public double NM4 { get; set; }

        public double NM5 { get; set; }

        public double NM6 { get; set; }

        public double NM7 { get; set; }

        public double NM8 { get; set; }

        public double NM9 { get; set; }

        public double NM10 { get; set; }

        public double NM11 { get; set; }

        public double NM12 { get; set; }

        public double QK1 { get; set; }

        public double QK2 { get; set; }

        public double QK3 { get; set; }

        public double QK4 { get; set; }

        public double QK5 { get; set; }

        public double QK6 { get; set; }

        public double QK7 { get; set; }

        public double QK8 { get; set; }

        public double QK9 { get; set; }

        public double QK10 { get; set; }

        public double QK11 { get; set; }

        public double QK12 { get; set; }

        public double NK1 { get; set; }

        public double NK2 { get; set; }

        public double NK3 { get; set; }

        public double NK4 { get; set; }

        public double NK5 { get; set; }

        public double NK6 { get; set; }

        public double NK7 { get; set; }

        public double NK8 { get; set; }

        public double NK9 { get; set; }

        public double NK10 { get; set; }

        public double NK11 { get; set; }

        public double NK12 { get; set; }

        [Key]
        public int RECNUM { get; set; }
    }
}
