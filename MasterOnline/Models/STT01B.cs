namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT01B
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? No { get; set; }

        public byte Jenis_Form { get; set; }

        [Required]
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

        public double Qty { get; set; }

        public double Harsat { get; set; }

        public double Harga { get; set; }

        [StringLength(10)]
        public string Rak { get; set; }

        [StringLength(2)]
        public string JTran { get; set; }

        [StringLength(30)]
        public string UserName { get; set; }

        public DateTime? TglInput { get; set; }

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

        public double QTY_TERIMA_PO_QC { get; set; }

        [StringLength(20)]
        public string BRG_ORIGINAL { get; set; }

        public double? TRANS_NO_URUT { get; set; }

        public double? STN_N { get; set; }

        public double? QTY3 { get; set; }

        public double? BIAYA_PER_QTY { get; set; }

        [StringLength(10)]
        public string BUKTI_DS { get; set; }

        [StringLength(10)]
        public string BUKTI_REFF { get; set; }

        public virtual STT01A STT01A { get; set; }
    }
}
