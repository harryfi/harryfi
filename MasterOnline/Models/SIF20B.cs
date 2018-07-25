namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF20B
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int No { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(5)]
        public string GRUP { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime TGL1 { get; set; }

        [Key]
        [Column(Order = 3)]
        public DateTime TGL2 { get; set; }

        [StringLength(10)]
        public string Sort1 { get; set; }

        [StringLength(10)]
        public string Sort2 { get; set; }

        [StringLength(10)]
        public string Sort3 { get; set; }

        [StringLength(10)]
        public string Sort4 { get; set; }

        [StringLength(10)]
        public string Sort5 { get; set; }

        [Key]
        [Column(Order = 4)]
        public float PERCENT2 { get; set; }

        [Key]
        [Column(Order = 5)]
        public double NILAI2 { get; set; }

        [StringLength(10)]
        public string JENIS2 { get; set; }

        [Key]
        [Column(Order = 6)]
        public float PERCENT3 { get; set; }

        [Key]
        [Column(Order = 7)]
        public double NILAI3 { get; set; }

        [StringLength(10)]
        public string JENIS3 { get; set; }

        [Key]
        [Column(Order = 8)]
        public float PERCENT4 { get; set; }

        [Key]
        [Column(Order = 9)]
        public double NILAI4 { get; set; }

        [StringLength(10)]
        public string JENIS4 { get; set; }

        [Key]
        [Column(Order = 10)]
        public float PERCENT5 { get; set; }

        [Key]
        [Column(Order = 11)]
        public double NILAI5 { get; set; }

        [StringLength(10)]
        public string JENIS5 { get; set; }

        [StringLength(20)]
        public string Brg { get; set; }
    }
}
