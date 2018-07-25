namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFMTL")]
    public partial class GLFMTL
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

        public double THI1 { get; set; }

        public double THI2 { get; set; }

        public double THI3 { get; set; }

        public double THI4 { get; set; }

        public double THI5 { get; set; }

        public double THI6 { get; set; }

        public double THI7 { get; set; }

        public double THI8 { get; set; }

        public double THI9 { get; set; }

        public double THI10 { get; set; }

        public double THI11 { get; set; }

        public double THI12 { get; set; }

        public double THL1 { get; set; }

        public double THL2 { get; set; }

        public double THL3 { get; set; }

        public double THL4 { get; set; }

        public double THL5 { get; set; }

        public double THL6 { get; set; }

        public double THL7 { get; set; }

        public double THL8 { get; set; }

        public double THL9 { get; set; }

        public double THL10 { get; set; }

        public double THL11 { get; set; }

        public double THL12 { get; set; }
    }
}
