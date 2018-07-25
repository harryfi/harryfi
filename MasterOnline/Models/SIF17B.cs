namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF17B
    {
        [Key]
        [Column(Order = 0)]
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

        public float PERCENT1 { get; set; }

        public double NILAI1 { get; set; }

        [StringLength(10)]
        public string JENIS1 { get; set; }

        public float PERCENT2 { get; set; }

        public double NILAI2 { get; set; }

        [StringLength(10)]
        public string JENIS2 { get; set; }

        [StringLength(20)]
        public string Brg { get; set; }

        public double PERCENT3 { get; set; }

        public double NILAI3 { get; set; }

        [StringLength(10)]
        public string JENIS3 { get; set; }

        public double PERCENT4 { get; set; }

        public double NILAI4 { get; set; }

        [StringLength(10)]
        public string JENIS4 { get; set; }

        public double PERCENT5 { get; set; }

        public double NILAI5 { get; set; }

        [StringLength(10)]
        public string JENIS5 { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
