namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF03A
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string ITEM_NO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string SUPPLIER_CODE { get; set; }

        [Key]
        [Column(Order = 2)]
        public int NO_URUT { get; set; }

        public DateTime DATE { get; set; }

        [StringLength(3)]
        public string CURRENCY_CODE { get; set; }

        public double HBESAR { get; set; }

        public byte? TYPE { get; set; }

        [StringLength(50)]
        public string DESCRIPTION { get; set; }

        public double HKECIL { get; set; }

        [StringLength(10)]
        public string PO_NO { get; set; }

        public double DISC_PERCENT { get; set; }

        public DateTime? TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        public double H_STN_3 { get; set; }

        public double H_STN_4 { get; set; }

        public virtual POF03 POF03 { get; set; }
    }
}
