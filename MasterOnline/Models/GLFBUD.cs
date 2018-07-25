namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFBUD")]
    public partial class GLFBUD
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LKS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string KODE { get; set; }

        public double B1 { get; set; }

        public double B2 { get; set; }

        public double B3 { get; set; }

        public double B4 { get; set; }

        public double B5 { get; set; }

        public double B6 { get; set; }

        public double B7 { get; set; }

        public double B8 { get; set; }

        public double B9 { get; set; }

        public double B10 { get; set; }

        public double B11 { get; set; }

        public double B12 { get; set; }

        [StringLength(4)]
        public string THN { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
