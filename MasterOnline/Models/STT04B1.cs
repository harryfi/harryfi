namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT04B1
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string Jenis_Size { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string Gud { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string Sort1 { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(10)]
        public string Sort2 { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(10)]
        public string Sort3 { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(10)]
        public string Sort4 { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(10)]
        public string Sort5 { get; set; }

        public double Qty_1 { get; set; }

        public DateTime Tgl_1 { get; set; }

        public double HPokok_1 { get; set; }

        [StringLength(1)]
        public string BK_1 { get; set; }

        [StringLength(5)]
        public string Stn_1 { get; set; }

        [StringLength(10)]
        public string WO_1 { get; set; }

        public double Qty_Berat_1 { get; set; }

        public double QTY_KECIL_1 { get; set; }

        public double QTY_BESAR_1 { get; set; }

        public double QTY_3_1 { get; set; }

        public double QTY_4_1 { get; set; }

        public double Qty_2 { get; set; }

        public DateTime Tgl_2 { get; set; }

        public double HPokok_2 { get; set; }

        [StringLength(1)]
        public string BK_2 { get; set; }

        [StringLength(5)]
        public string Stn_2 { get; set; }

        [StringLength(10)]
        public string WO_2 { get; set; }

        public double Qty_Berat_2 { get; set; }

        public double QTY_KECIL_2 { get; set; }

        public double QTY_BESAR_2 { get; set; }

        public double QTY_3_2 { get; set; }

        public double QTY_4_2 { get; set; }

        public double Qty_3 { get; set; }

        public DateTime Tgl_3 { get; set; }

        public double HPokok_3 { get; set; }

        [StringLength(1)]
        public string BK_3 { get; set; }

        [StringLength(5)]
        public string Stn_3 { get; set; }

        [StringLength(10)]
        public string WO_3 { get; set; }

        public double Qty_Berat_3 { get; set; }

        public double QTY_KECIL_3 { get; set; }

        public double QTY_BESAR_3 { get; set; }

        public double QTY_3_3 { get; set; }

        public double QTY_4_3 { get; set; }

        public double Qty_4 { get; set; }

        public DateTime Tgl_4 { get; set; }

        public double HPokok_4 { get; set; }

        [StringLength(1)]
        public string BK_4 { get; set; }

        [StringLength(5)]
        public string Stn_4 { get; set; }

        [StringLength(10)]
        public string WO_4 { get; set; }

        public double Qty_Berat_4 { get; set; }

        public double QTY_KECIL_4 { get; set; }

        public double QTY_BESAR_4 { get; set; }

        public double QTY_3_4 { get; set; }

        public double QTY_4_4 { get; set; }

        public double Qty_5 { get; set; }

        public DateTime Tgl_5 { get; set; }

        public double HPokok_5 { get; set; }

        [StringLength(1)]
        public string BK_5 { get; set; }

        [StringLength(5)]
        public string Stn_5 { get; set; }

        [StringLength(10)]
        public string WO_5 { get; set; }

        public double Qty_Berat_5 { get; set; }

        public double QTY_KECIL_5 { get; set; }

        public double QTY_BESAR_5 { get; set; }

        public double QTY_3_5 { get; set; }

        public double QTY_4_5 { get; set; }

        public double Qty_6 { get; set; }

        public DateTime Tgl_6 { get; set; }

        public double HPokok_6 { get; set; }

        [StringLength(1)]
        public string BK_6 { get; set; }

        [StringLength(5)]
        public string Stn_6 { get; set; }

        [StringLength(10)]
        public string WO_6 { get; set; }

        public double Qty_Berat_6 { get; set; }

        public double QTY_KECIL_6 { get; set; }

        public double QTY_BESAR_6 { get; set; }

        public double QTY_3_6 { get; set; }

        public double QTY_4_6 { get; set; }

        public double Qty_7 { get; set; }

        public DateTime Tgl_7 { get; set; }

        public double HPokok_7 { get; set; }

        [StringLength(1)]
        public string BK_7 { get; set; }

        [StringLength(5)]
        public string Stn_7 { get; set; }

        [StringLength(10)]
        public string WO_7 { get; set; }

        public double Qty_Berat_7 { get; set; }

        public double QTY_KECIL_7 { get; set; }

        public double QTY_BESAR_7 { get; set; }

        public double QTY_3_7 { get; set; }

        public double QTY_4_7 { get; set; }

        public double Qty_8 { get; set; }

        public DateTime Tgl_8 { get; set; }

        public double HPokok_8 { get; set; }

        [StringLength(1)]
        public string BK_8 { get; set; }

        [StringLength(5)]
        public string Stn_8 { get; set; }

        [StringLength(10)]
        public string WO_8 { get; set; }

        public double Qty_Berat_8 { get; set; }

        public double QTY_KECIL_8 { get; set; }

        public double QTY_BESAR_8 { get; set; }

        public double QTY_3_8 { get; set; }

        public double QTY_4_8 { get; set; }

        public double Qty_9 { get; set; }

        public DateTime Tgl_9 { get; set; }

        public double HPokok_9 { get; set; }

        [StringLength(1)]
        public string BK_9 { get; set; }

        [StringLength(5)]
        public string Stn_9 { get; set; }

        [StringLength(10)]
        public string WO_9 { get; set; }

        public double Qty_Berat_9 { get; set; }

        public double QTY_KECIL_9 { get; set; }

        public double QTY_BESAR_9 { get; set; }

        public double QTY_3_9 { get; set; }

        public double QTY_4_9 { get; set; }

        public double Qty_10 { get; set; }

        public DateTime Tgl_10 { get; set; }

        public double HPokok_10 { get; set; }

        [StringLength(1)]
        public string BK_10 { get; set; }

        [StringLength(5)]
        public string Stn_10 { get; set; }

        [StringLength(10)]
        public string WO_10 { get; set; }

        public double Qty_Berat_10 { get; set; }

        public double QTY_KECIL_10 { get; set; }

        public double QTY_BESAR_10 { get; set; }

        public double QTY_3_10 { get; set; }

        public double QTY_4_10 { get; set; }

        public double Qty_11 { get; set; }

        public DateTime Tgl_11 { get; set; }

        public double HPokok_11 { get; set; }

        [StringLength(1)]
        public string BK_11 { get; set; }

        [StringLength(5)]
        public string Stn_11 { get; set; }

        [StringLength(10)]
        public string WO_11 { get; set; }

        public double Qty_Berat_11 { get; set; }

        public double QTY_KECIL_11 { get; set; }

        public double QTY_BESAR_11 { get; set; }

        public double QTY_3_11 { get; set; }

        public double QTY_4_11 { get; set; }

        public double Qty_12 { get; set; }

        public DateTime Tgl_12 { get; set; }

        public double HPokok_12 { get; set; }

        [StringLength(1)]
        public string BK_12 { get; set; }

        [StringLength(5)]
        public string Stn_12 { get; set; }

        [StringLength(10)]
        public string WO_12 { get; set; }

        public double Qty_Berat_12 { get; set; }

        public double QTY_KECIL_12 { get; set; }

        public double QTY_BESAR_12 { get; set; }

        public double QTY_3_12 { get; set; }

        public double QTY_4_12 { get; set; }

        public double Qty_13 { get; set; }

        public DateTime Tgl_13 { get; set; }

        public double HPokok_13 { get; set; }

        [StringLength(1)]
        public string BK_13 { get; set; }

        [StringLength(5)]
        public string Stn_13 { get; set; }

        [StringLength(10)]
        public string WO_13 { get; set; }

        public double Qty_Berat_13 { get; set; }

        public double QTY_KECIL_13 { get; set; }

        public double QTY_BESAR_13 { get; set; }

        public double QTY_3_13 { get; set; }

        public double QTY_4_13 { get; set; }

        public double Qty_14 { get; set; }

        public DateTime Tgl_14 { get; set; }

        public double HPokok_14 { get; set; }

        [StringLength(1)]
        public string BK_14 { get; set; }

        [StringLength(5)]
        public string Stn_14 { get; set; }

        [StringLength(10)]
        public string WO_14 { get; set; }

        public double Qty_Berat_14 { get; set; }

        public double QTY_KECIL_14 { get; set; }

        public double QTY_BESAR_14 { get; set; }

        public double QTY_3_14 { get; set; }

        public double QTY_4_14 { get; set; }

        public double Qty_15 { get; set; }

        public DateTime Tgl_15 { get; set; }

        public double HPokok_15 { get; set; }

        [StringLength(1)]
        public string BK_15 { get; set; }

        [StringLength(5)]
        public string Stn_15 { get; set; }

        [StringLength(10)]
        public string WO_15 { get; set; }

        public double Qty_Berat_15 { get; set; }

        public double QTY_KECIL_15 { get; set; }

        public double QTY_BESAR_15 { get; set; }

        public double QTY_3_15 { get; set; }

        public double QTY_4_15 { get; set; }

        public virtual STT04A STT04A { get; set; }
    }
}
