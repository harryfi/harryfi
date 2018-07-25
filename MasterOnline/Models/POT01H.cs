namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT01H
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string PO_NO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string CONSIGNEE { get; set; }

        [StringLength(1)]
        public string SHIPMENT_TYPE { get; set; }

        [StringLength(20)]
        public string BL_NO { get; set; }

        public DateTime? BL_DATE { get; set; }

        [StringLength(20)]
        public string ORDER_CONFIRMATION { get; set; }

        public DateTime? ORDER_CONFIRM_DATE { get; set; }

        [StringLength(20)]
        public string MAWB { get; set; }

        [Column(TypeName = "numeric")]
        public decimal QTY { get; set; }

        [StringLength(10)]
        public string STN { get; set; }

        [StringLength(50)]
        public string CONTENT { get; set; }

        [Column(TypeName = "numeric")]
        public decimal SHIPMENT_TERM { get; set; }

        [Column(TypeName = "numeric")]
        public decimal FREIGHT { get; set; }

        [Column(TypeName = "numeric")]
        public decimal INSURANCE { get; set; }

        [Column(TypeName = "numeric")]
        public decimal OTHERS { get; set; }

        [StringLength(10)]
        public string SHIPPER { get; set; }

        [StringLength(50)]
        public string PORT { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
