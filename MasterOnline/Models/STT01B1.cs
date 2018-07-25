namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT01B1
    {
        [Key]
        [Column(Order = 0)]
        public int No { get; set; }

        [Key]
        [Column(Order = 1)]
        public byte Jenis_Form { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string Nobuk { get; set; }

        [StringLength(1)]
        public string Satuan { get; set; }

        [Required]
        [StringLength(1)]
        public string Jenis_Size { get; set; }

        [StringLength(4)]
        public string Ke_Gd { get; set; }

        [StringLength(4)]
        public string Dr_Gd { get; set; }

        [StringLength(10)]
        public string WO { get; set; }

        public double Qty1 { get; set; }

        public double Qty2 { get; set; }

        public double Qty3 { get; set; }

        public double Qty4 { get; set; }

        public double Qty5 { get; set; }

        public double Qty6 { get; set; }

        public double Qty7 { get; set; }

        public double Qty8 { get; set; }

        public double Qty9 { get; set; }

        public double Qty10 { get; set; }

        public double Qty11 { get; set; }

        public double Qty12 { get; set; }

        public double Qty13 { get; set; }

        public double Qty14 { get; set; }

        public double Qty15 { get; set; }

        public double Harsat { get; set; }

        public double Harga { get; set; }

        [StringLength(10)]
        public string Rak { get; set; }

        [StringLength(2)]
        public string JTran { get; set; }

        [StringLength(30)]
        public string UserName { get; set; }

        public DateTime TglInput { get; set; }

        public double Qty_Retur { get; set; }

        public double Qty_Berat { get; set; }

        [StringLength(5)]
        public string KLINK { get; set; }

        [StringLength(15)]
        public string NO_WO { get; set; }

        public double TOTAL_LOT { get; set; }

        public double TOTAL_QTY { get; set; }

        [StringLength(1000)]
        public string KET { get; set; }

        public double QTY_TERIMA { get; set; }

        public double QTY_CLAIM { get; set; }

        public double NO_URUT_PO { get; set; }

        public double NO_URUT_SJ { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT1 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT2 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT3 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT4 { get; set; }

        public virtual STT01A STT01A { get; set; }
    }
}
