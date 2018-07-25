namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBF03
    {
        [Required]
        [StringLength(10)]
        public string SUPP { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG { get; set; }

        public short THN { get; set; }

        public double Q1 { get; set; }

        public double Q2 { get; set; }

        public double Q3 { get; set; }

        public double Q4 { get; set; }

        public double Q5 { get; set; }

        public double Q6 { get; set; }

        public double Q7 { get; set; }

        public double Q8 { get; set; }

        public double Q9 { get; set; }

        public double Q10 { get; set; }

        public double Q11 { get; set; }

        public double Q12 { get; set; }

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

        [Key]
        public int RECNUM { get; set; }
    }
}
