namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class APF02
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string SUPP { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(3)]
        public string VLT { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short THN { get; set; }

        public double AWAL { get; set; }

        public double MUKA { get; set; }

        public double GIRO { get; set; }

        public double D1 { get; set; }

        public double D2 { get; set; }

        public double D3 { get; set; }

        public double D4 { get; set; }

        public double D5 { get; set; }

        public double D6 { get; set; }

        public double D7 { get; set; }

        public double D8 { get; set; }

        public double D9 { get; set; }

        public double D10 { get; set; }

        public double D11 { get; set; }

        public double D12 { get; set; }

        public double K1 { get; set; }

        public double K2 { get; set; }

        public double K3 { get; set; }

        public double K4 { get; set; }

        public double K5 { get; set; }

        public double K6 { get; set; }

        public double K7 { get; set; }

        public double K8 { get; set; }

        public double K9 { get; set; }

        public double K10 { get; set; }

        public double K11 { get; set; }

        public double K12 { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }
    }
}
