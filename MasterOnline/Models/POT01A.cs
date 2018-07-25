namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT01A
    {
        [Key]
        [StringLength(10)]
        public string PO_NO { get; set; }

        public DateTime PO_DATE { get; set; }

        public DateTime SHIPMENT_DATE { get; set; }

        [Column(TypeName = "text")]
        public string DESCRIPTION { get; set; }

        [StringLength(10)]
        public string SUPPLIER_CODE { get; set; }

        [StringLength(50)]
        public string SUPPLIER_NAME { get; set; }

        [StringLength(5)]
        public string PO_VALUTA { get; set; }

        public double PO_KURS { get; set; }

        public double DISC_PERCENT { get; set; }

        public double DISC_VALUE { get; set; }

        public double PPN_PERCENT { get; set; }

        public double PPN_VALUE { get; set; }

        public double BRUTO_VALUE { get; set; }

        public double NETTO_VALUE { get; set; }

        public bool TERIMA_PENUH { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        public short PRINT_COPIES { get; set; }

        public double TERM { get; set; }

        [StringLength(1)]
        public string JENIS { get; set; }

        [StringLength(20)]
        public string USER_NAME { get; set; }

        public DateTime? TGL_INPUT { get; set; }

        public double UANG_MUKA { get; set; }

        [StringLength(1)]
        public string STATUS_APPROVE { get; set; }

        [StringLength(20)]
        public string USERNAME_APPROVE { get; set; }

        public DateTime? DATE_APPROVE { get; set; }

        [StringLength(5)]
        public string ALAMAT { get; set; }

        [Column(TypeName = "text")]
        public string KET { get; set; }

        public bool FLAG_MATRIX { get; set; }

        [StringLength(10)]
        public string MATRIX { get; set; }

        public double? KOEFISIEN_HRGSATUAN_STD { get; set; }

        public double PPNBM { get; set; }

        public double TYPE_BELI { get; set; }

        public double PPH_22_PERCENT { get; set; }

        public double PPH_22_VALUE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

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
        public string SATUAN { get; set; }

        [StringLength(50)]
        public string CONTENT { get; set; }

        [Column(TypeName = "numeric")]
        public decimal SHIPMENT_TERM { get; set; }

        public double FREIGHT { get; set; }

        public double INSURANCE { get; set; }

        public double OTHERS { get; set; }

        [StringLength(10)]
        public string SHIPPER { get; set; }

        [StringLength(50)]
        public string PORT { get; set; }

        [Required]
        [StringLength(1)]
        public string PROSES_PO { get; set; }

        public DateTime? TGL_CLOSING { get; set; }

        public int PO_CLOSING { get; set; }

        [Required]
        [StringLength(10)]
        public string GROUP_BRG { get; set; }

        [Required]
        [StringLength(10)]
        public string STATUS_APPROVE_2 { get; set; }

        [Required]
        [StringLength(10)]
        public string USERNAME_APPROVE_2 { get; set; }

        [Required]
        [StringLength(10)]
        public string STATUS_APPROVE_3 { get; set; }

        [Required]
        [StringLength(10)]
        public string USERNAME_APPROVE_3 { get; set; }

        public DateTime? DATE_APPROVE_2 { get; set; }

        public DateTime? DATE_APPROVE_3 { get; set; }

        [Required]
        [StringLength(200)]
        public string APPROVE_CATATAN_1 { get; set; }

        [Required]
        [StringLength(200)]
        public string APPROVE_CATATAN_2 { get; set; }

        [Required]
        [StringLength(200)]
        public string APPROVE_CATATAN_3 { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public virtual POT01E POT01E { get; set; }
    }
}
