namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("POFSYS")]
    public partial class POFSY
    {
        [Key]
        [StringLength(1)]
        public string AUTONUM { get; set; }

        [StringLength(1)]
        public string HRGSATUAN { get; set; }

        [StringLength(50)]
        public string DEFAULTSATUAN { get; set; }

        [StringLength(3)]
        public string NO_SERI_PP { get; set; }

        [StringLength(3)]
        public string NO_SERI_PO { get; set; }

        public int LAST_NO_URUT_PP { get; set; }

        public int LAST_NO_URUT_PO { get; set; }

        [StringLength(1)]
        public string TERBILANG_IN { get; set; }

        [StringLength(1)]
        public string APPROVE_PP { get; set; }

        [StringLength(2)]
        public string VALIDASI_TOLERANSI { get; set; }

        public double TOLERANSI_SEMUA_BRG { get; set; }

        [StringLength(1)]
        public string VALIDASI_BUYER { get; set; }

        [StringLength(1)]
        public string APPROVE_PO { get; set; }

        [StringLength(3)]
        public string NO_SERI_MATRIK { get; set; }

        public double LAST_NO_URUT_MATRIK { get; set; }

        public double KOEFISIEN_HRGSATUAN_STD { get; set; }

        [StringLength(10)]
        public string VALIDASI_SO { get; set; }

        [Required]
        [StringLength(20)]
        public string CAP_TYPE1 { get; set; }

        [Required]
        [StringLength(20)]
        public string CAP_TYPE2 { get; set; }

        [Required]
        [StringLength(20)]
        public string CAP_TYPE3 { get; set; }

        [Required]
        [StringLength(20)]
        public string CAP_TYPE4 { get; set; }

        [Required]
        [StringLength(20)]
        public string CAP_TYPE5 { get; set; }

        [Required]
        [StringLength(1)]
        public string PP_ENTRY_STYLE { get; set; }

        [Required]
        [StringLength(1)]
        public string BIAYA_IMPORT { get; set; }

        public int Toleransi_Closing { get; set; }

        public int Group_Order_Ke { get; set; }

        [Required]
        [StringLength(30)]
        public string Hutang_PO { get; set; }

        public int JENIS_PEMB { get; set; }

        public int CLOSING_PO { get; set; }

        [Required]
        [StringLength(10)]
        public string KD_ALT_KRM { get; set; }

        [StringLength(3)]
        public string PREFIX_CLOSING_PO { get; set; }

        public double NO_URUT_CLOSING_PO { get; set; }

        [Required]
        [StringLength(1)]
        public string LEVEL_APPROVAL_PO { get; set; }

        [Required]
        [StringLength(1)]
        public string LEVEL_APPROVAL_PP { get; set; }

        public int JENIS_ANGGARAN { get; set; }

        public int ANGGARAN { get; set; }

        public double EDIT_HBELI { get; set; }

        public double PRINT_APP_PO { get; set; }

        public int AUTOLOAD_PP_BRG { get; set; }

        public int VALIDASI_DIVISI { get; set; }

        public int ADA_JURNAL { get; set; }

        [StringLength(1)]
        public string MFDesimal { get; set; }

        [StringLength(1)]
        public string QFDesimal { get; set; }

        [StringLength(100)]
        public string DB_PATH { get; set; }

        [StringLength(1)]
        public string PAKAI_QR { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
