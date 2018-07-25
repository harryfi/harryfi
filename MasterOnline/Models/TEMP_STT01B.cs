namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_STT01B
    {
        [Key]
        [Column(Order = 0)]
        public byte Jenis_Form { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string Nobuk { get; set; }

        [StringLength(20)]
        public string Kobar { get; set; }

        [StringLength(1)]
        public string Satuan { get; set; }

        [StringLength(4)]
        public string Ke_Gd { get; set; }

        [StringLength(4)]
        public string Dr_Gd { get; set; }

        [StringLength(10)]
        public string WO { get; set; }

        [Key]
        [Column(Order = 2)]
        public double Qty { get; set; }

        [Key]
        [Column(Order = 3)]
        public double Harsat { get; set; }

        [Key]
        [Column(Order = 4)]
        public double Harga { get; set; }

        [StringLength(10)]
        public string Rak { get; set; }

        [StringLength(2)]
        public string JTran { get; set; }

        [StringLength(30)]
        public string UserName { get; set; }

        [Key]
        [Column(Order = 5)]
        public DateTime TglInput { get; set; }

        [Key]
        [Column(Order = 6)]
        public double Qty_Retur { get; set; }

        [Key]
        [Column(Order = 7)]
        public double Qty_Berat { get; set; }

        [StringLength(5)]
        public string KLINK { get; set; }

        [StringLength(15)]
        public string NO_WO { get; set; }

        [Key]
        [Column(Order = 8)]
        public double TOTAL_LOT { get; set; }

        [Key]
        [Column(Order = 9)]
        public double TOTAL_QTY { get; set; }

        [StringLength(1000)]
        public string KET { get; set; }

        [Key]
        [Column(Order = 10)]
        public double QTY_TERIMA { get; set; }

        [Key]
        [Column(Order = 11)]
        public double QTY_CLAIM { get; set; }

        [Key]
        [Column(Order = 12)]
        public double NO_URUT_PO { get; set; }

        [Key]
        [Column(Order = 13)]
        public double NO_URUT_SJ { get; set; }

        [Key]
        [Column(Order = 14)]
        public double NO { get; set; }
    }
}
