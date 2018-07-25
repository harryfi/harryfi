namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF02A
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string DIVISION_NO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string ITEM_NO { get; set; }

        [StringLength(50)]
        public string ITEM_DESCRIPTION { get; set; }

        [StringLength(1)]
        public string UNIT_CODE { get; set; }

        public double QUANTITY { get; set; }

        public double AMOUNT { get; set; }

        public DateTime TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        public virtual POF02 POF02 { get; set; }
    }
}
