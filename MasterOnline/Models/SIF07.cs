namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF07
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(2)]
        public string KDHARGA { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(1)]
        public string LEVEL_SORT_FIELD { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string KODE_SORT_FIELD { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(20)]
        public string KODE_BRG { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(1)]
        public string LEVEL_CUST { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(10)]
        public string KODE_LEVEL_CUST { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(10)]
        public string KODE_CUST { get; set; }

        public double DISC_1 { get; set; }

        public double DISC_2 { get; set; }

        public double DISC_3 { get; set; }

        public double DISC_4 { get; set; }

        public double DISC_5 { get; set; }

        public double NDISC_1 { get; set; }

        public double NDISC_2 { get; set; }

        public double NDISC_3 { get; set; }

        public double NDISC_4 { get; set; }

        public double NDISC_5 { get; set; }

        public DateTime? TGLINPUT { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
