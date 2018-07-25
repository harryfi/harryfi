namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT04B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string PO_NO { get; set; }

        public double? TOT_BIAYA_BRG { get; set; }

        public double? TOT_BIAYA_PO { get; set; }

        public double? TOT_BIAYA_QTY { get; set; }

        public double? QUANTITY { get; set; }

        [Key]
        [Column(Order = 2)]
        public double NO_URUT { get; set; }

        public double? VOLUME { get; set; }

        public double? BERAT { get; set; }

        public double? NILAI { get; set; }

        [StringLength(10)]
        public string PO_Brg { get; set; }

        [StringLength(1)]
        public string Unit_Code { get; set; }

        public double Tot_Biaya_PO_Home { get; set; }

        [StringLength(10)]
        public string NO_PB { get; set; }

        public double? Biaya_PerItem_Estimasi { get; set; }

        [StringLength(30)]
        public string KET { get; set; }

        public double? Tot_Qty_Vol { get; set; }

        public double? BY_QTY { get; set; }

        public double? BY_QTY_Estimasi { get; set; }

        public double? Tot_QTY_Estimasi { get; set; }

        public double? Kurs { get; set; }

        [StringLength(5)]
        public string Valuta { get; set; }

        public bool? AutoLoadPO { get; set; }

        public bool? AutoLoad_LPB { get; set; }

        [StringLength(10)]
        public string REF_LPB { get; set; }

        public double? KARTON { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
