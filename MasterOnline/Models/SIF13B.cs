namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF13B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(5)]
        public string WIL { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string CUST { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(10)]
        public string CUST_QQ { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(1)]
        public string BK { get; set; }

        public double H_LOCO { get; set; }

        public double H_FRANCO { get; set; }

        public DateTime TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        public virtual SIF13A SIF13A { get; set; }
    }
}
