namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF01B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string Cust { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(3)]
        public string SLM { get; set; }

        public bool Day1 { get; set; }

        public bool Day2 { get; set; }

        public bool Day3 { get; set; }

        public bool Day4 { get; set; }

        public bool Day5 { get; set; }

        public bool Day6 { get; set; }

        public bool Day7 { get; set; }

        public bool Week1 { get; set; }

        public bool Week2 { get; set; }

        public bool Week3 { get; set; }

        public bool Week4 { get; set; }

        [StringLength(10)]
        public string Kd_Rute { get; set; }
    }
}
