namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF08B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string CUST { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string BRG { get; set; }

        [StringLength(40)]
        public string NAMA { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(20)]
        public string BRG_CUST { get; set; }

        [StringLength(80)]
        public string NAMA_BRG_CUST { get; set; }

        [StringLength(40)]
        public string UserName { get; set; }

        public DateTime TglInput { get; set; }

        public bool EXCLUSIVE { get; set; }

        [StringLength(40)]
        public string NAMA2 { get; set; }

        [StringLength(40)]
        public string NAMA3 { get; set; }

        public virtual SIF08A SIF08A { get; set; }
    }
}
