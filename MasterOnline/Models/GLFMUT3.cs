namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFMUT3
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short THN { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LKS { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(15)]
        public string KODE { get; set; }

        public short? BLN { get; set; }

        public double N1 { get; set; }

        public double N2 { get; set; }

        public double N3 { get; set; }

        public double N4 { get; set; }

        public double N5 { get; set; }

        public double N6 { get; set; }

        public double N7 { get; set; }

        public double N8 { get; set; }

        public double N9 { get; set; }

        public double N10 { get; set; }

        public double N11 { get; set; }

        public double N12 { get; set; }

        public double N13 { get; set; }

        public double N14 { get; set; }

        public double N15 { get; set; }

        public double N16 { get; set; }

        public double N17 { get; set; }

        public double N18 { get; set; }

        public double N19 { get; set; }

        public double N20 { get; set; }

        public double N21 { get; set; }

        public double N22 { get; set; }

        public double N23 { get; set; }

        public double N24 { get; set; }

        public double N25 { get; set; }

        public double N26 { get; set; }

        public double N27 { get; set; }

        public double N28 { get; set; }

        public double N29 { get; set; }

        public double N30 { get; set; }

        public double N31 { get; set; }
    }
}
