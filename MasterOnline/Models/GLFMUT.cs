namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFMUT")]
    public partial class GLFMUT
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

        public double SA { get; set; }

        public double DEBET1 { get; set; }

        public double DEBET2 { get; set; }

        public double DEBET3 { get; set; }

        public double DEBET4 { get; set; }

        public double DEBET5 { get; set; }

        public double DEBET6 { get; set; }

        public double DEBET7 { get; set; }

        public double DEBET8 { get; set; }

        public double DEBET9 { get; set; }

        public double DEBET10 { get; set; }

        public double DEBET11 { get; set; }

        public double DEBET12 { get; set; }

        public double KREDIT1 { get; set; }

        public double KREDIT2 { get; set; }

        public double KREDIT3 { get; set; }

        public double KREDIT4 { get; set; }

        public double KREDIT5 { get; set; }

        public double KREDIT6 { get; set; }

        public double KREDIT7 { get; set; }

        public double KREDIT8 { get; set; }

        public double KREDIT9 { get; set; }

        public double KREDIT10 { get; set; }

        public double KREDIT11 { get; set; }

        public double KREDIT12 { get; set; }
    }
}
