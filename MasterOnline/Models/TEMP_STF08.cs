namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_STF08
    {
        [StringLength(4)]
        public string GD { get; set; }

        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short Tahun { get; set; }

        [Key]
        [Column(Order = 1)]
        public double QAwal { get; set; }

        [Key]
        [Column(Order = 2)]
        public double NAwal { get; set; }

        [Key]
        [Column(Order = 3)]
        public double QM1 { get; set; }

        [Key]
        [Column(Order = 4)]
        public double QM2 { get; set; }

        [Key]
        [Column(Order = 5)]
        public double QM3 { get; set; }

        [Key]
        [Column(Order = 6)]
        public double QM4 { get; set; }

        [Key]
        [Column(Order = 7)]
        public double QM5 { get; set; }

        [Key]
        [Column(Order = 8)]
        public double QM6 { get; set; }

        [Key]
        [Column(Order = 9)]
        public double QM7 { get; set; }

        [Key]
        [Column(Order = 10)]
        public double QM8 { get; set; }

        [Key]
        [Column(Order = 11)]
        public double QM9 { get; set; }

        [Key]
        [Column(Order = 12)]
        public double QM10 { get; set; }

        [Key]
        [Column(Order = 13)]
        public double QM11 { get; set; }

        [Key]
        [Column(Order = 14)]
        public double QM12 { get; set; }

        [Key]
        [Column(Order = 15)]
        public double NM1 { get; set; }

        [Key]
        [Column(Order = 16)]
        public double NM2 { get; set; }

        [Key]
        [Column(Order = 17)]
        public double NM3 { get; set; }

        [Key]
        [Column(Order = 18)]
        public double NM4 { get; set; }

        [Key]
        [Column(Order = 19)]
        public double NM5 { get; set; }

        [Key]
        [Column(Order = 20)]
        public double NM6 { get; set; }

        [Key]
        [Column(Order = 21)]
        public double NM7 { get; set; }

        [Key]
        [Column(Order = 22)]
        public double NM8 { get; set; }

        [Key]
        [Column(Order = 23)]
        public double NM9 { get; set; }

        [Key]
        [Column(Order = 24)]
        public double NM10 { get; set; }

        [Key]
        [Column(Order = 25)]
        public double NM11 { get; set; }

        [Key]
        [Column(Order = 26)]
        public double NM12 { get; set; }

        [Key]
        [Column(Order = 27)]
        public double QK1 { get; set; }

        [Key]
        [Column(Order = 28)]
        public double QK2 { get; set; }

        [Key]
        [Column(Order = 29)]
        public double QK3 { get; set; }

        [Key]
        [Column(Order = 30)]
        public double QK4 { get; set; }

        [Key]
        [Column(Order = 31)]
        public double QK5 { get; set; }

        [Key]
        [Column(Order = 32)]
        public double QK6 { get; set; }

        [Key]
        [Column(Order = 33)]
        public double QK7 { get; set; }

        [Key]
        [Column(Order = 34)]
        public double QK8 { get; set; }

        [Key]
        [Column(Order = 35)]
        public double QK9 { get; set; }

        [Key]
        [Column(Order = 36)]
        public double QK10 { get; set; }

        [Key]
        [Column(Order = 37)]
        public double QK11 { get; set; }

        [Key]
        [Column(Order = 38)]
        public double QK12 { get; set; }

        [Key]
        [Column(Order = 39)]
        public double NK1 { get; set; }

        [Key]
        [Column(Order = 40)]
        public double NK2 { get; set; }

        [Key]
        [Column(Order = 41)]
        public double NK3 { get; set; }

        [Key]
        [Column(Order = 42)]
        public double NK4 { get; set; }

        [Key]
        [Column(Order = 43)]
        public double NK5 { get; set; }

        [Key]
        [Column(Order = 44)]
        public double NK6 { get; set; }

        [Key]
        [Column(Order = 45)]
        public double NK7 { get; set; }

        [Key]
        [Column(Order = 46)]
        public double NK8 { get; set; }

        [Key]
        [Column(Order = 47)]
        public double NK9 { get; set; }

        [Key]
        [Column(Order = 48)]
        public double NK10 { get; set; }

        [Key]
        [Column(Order = 49)]
        public double NK11 { get; set; }

        [Key]
        [Column(Order = 50)]
        public double NK12 { get; set; }

        [Key]
        [Column(Order = 51)]
        public int RECNUM { get; set; }
    }
}
