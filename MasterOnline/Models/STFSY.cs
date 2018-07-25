namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STFSYS")]
    public partial class STFSY
    {
        [Key]
        [StringLength(4)]
        public string Gudang { get; set; }

        [StringLength(1)]
        public string Satuan { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        public short THN { get; set; }

        public byte? BLN { get; set; }

        [StringLength(1)]
        public string Metoda { get; set; }

        [StringLength(10)]
        public string LSort1 { get; set; }

        [StringLength(10)]
        public string LSort2 { get; set; }

        [StringLength(10)]
        public string LSort3 { get; set; }

        [StringLength(10)]
        public string LSort4 { get; set; }

        [StringLength(10)]
        public string LSort5 { get; set; }

        [StringLength(10)]
        public string ST_K1 { get; set; }

        [StringLength(10)]
        public string ST_K2 { get; set; }

        [StringLength(10)]
        public string ST_K3 { get; set; }

        [StringLength(10)]
        public string ST_K4 { get; set; }

        [StringLength(10)]
        public string ST_K5 { get; set; }

        [StringLength(10)]
        public string ST_P1 { get; set; }

        [StringLength(10)]
        public string ST_P2 { get; set; }

        [StringLength(10)]
        public string ST_P3 { get; set; }

        [StringLength(10)]
        public string ST_P4 { get; set; }

        [StringLength(10)]
        public string ST_P5 { get; set; }

        [StringLength(10)]
        public string ST_PJ1 { get; set; }

        [StringLength(10)]
        public string ST_PJ2 { get; set; }

        [StringLength(10)]
        public string ST_PJ3 { get; set; }

        [StringLength(10)]
        public string ST_PJ4 { get; set; }

        [StringLength(10)]
        public string ST_PJ5 { get; set; }

        [StringLength(1)]
        public string Stn_Berat { get; set; }

        public short Hal { get; set; }

        [StringLength(1)]
        public string LINK_GL { get; set; }

        [StringLength(1)]
        public string POST_STOCK { get; set; }

        [StringLength(1)]
        public string STOCK_MINUS { get; set; }

        [StringLength(1)]
        public string PROMPT_BRG { get; set; }

        [StringLength(1)]
        public string POST_KONFIG { get; set; }

        [StringLength(1)]
        public string MFDesimal { get; set; }

        [StringLength(1)]
        public string QFDesimal { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_1 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_2 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_3 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_4 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_5 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_6 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_7 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_8 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_9 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_10 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_11 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_12 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_13 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_14 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_15 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_16 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_17 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_18 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_19 { get; set; }

        [StringLength(20)]
        public string SPEC_CAPTION_20 { get; set; }

        public bool SPEC_ENABLE_1 { get; set; }

        public bool SPEC_ENABLE_2 { get; set; }

        public bool SPEC_ENABLE_3 { get; set; }

        public bool SPEC_ENABLE_4 { get; set; }

        public bool SPEC_ENABLE_5 { get; set; }

        public bool SPEC_ENABLE_6 { get; set; }

        public bool SPEC_ENABLE_7 { get; set; }

        public bool SPEC_ENABLE_8 { get; set; }

        public bool SPEC_ENABLE_9 { get; set; }

        public bool SPEC_ENABLE_10 { get; set; }

        public bool SPEC_ENABLE_11 { get; set; }

        public bool SPEC_ENABLE_12 { get; set; }

        public bool SPEC_ENABLE_13 { get; set; }

        public bool SPEC_ENABLE_14 { get; set; }

        public bool SPEC_ENABLE_15 { get; set; }

        public bool SPEC_ENABLE_16 { get; set; }

        public bool SPEC_ENABLE_17 { get; set; }

        public bool SPEC_ENABLE_18 { get; set; }

        public bool SPEC_ENABLE_19 { get; set; }

        public bool SPEC_ENABLE_20 { get; set; }

        public bool SPEC_NUMERIC_1 { get; set; }

        public bool SPEC_NUMERIC_2 { get; set; }

        public bool SPEC_NUMERIC_3 { get; set; }

        public bool SPEC_NUMERIC_4 { get; set; }

        public bool SPEC_NUMERIC_5 { get; set; }

        public bool SPEC_NUMERIC_6 { get; set; }

        public bool SPEC_NUMERIC_7 { get; set; }

        public bool SPEC_NUMERIC_8 { get; set; }

        public bool SPEC_NUMERIC_9 { get; set; }

        public bool SPEC_NUMERIC_10 { get; set; }

        public bool SPEC_NUMERIC_11 { get; set; }

        public bool SPEC_NUMERIC_12 { get; set; }

        public bool SPEC_NUMERIC_13 { get; set; }

        public bool SPEC_NUMERIC_14 { get; set; }

        public bool SPEC_NUMERIC_15 { get; set; }

        public bool SPEC_NUMERIC_16 { get; set; }

        public bool SPEC_NUMERIC_17 { get; set; }

        public bool SPEC_NUMERIC_18 { get; set; }

        public bool SPEC_NUMERIC_19 { get; set; }

        public bool SPEC_NUMERIC_20 { get; set; }

        [StringLength(1)]
        public string UPDATE_QOH_SJ { get; set; }

        [StringLength(1)]
        public string METODA_NO { get; set; }

        [Column("NO_SERI_AD/GD")]
        [StringLength(3)]
        public string NO_SERI_AD_GD { get; set; }

        [StringLength(3)]
        public string NO_SERI_PD { get; set; }

        [StringLength(3)]
        public string NO_SERI_PB { get; set; }

        [StringLength(1)]
        public string ADA_PB { get; set; }

        public bool KONFIRMASI_KIRIM { get; set; }

        [StringLength(1)]
        public string POSTING_STATUS { get; set; }

        [StringLength(1)]
        public string SECURITY_GUDANG { get; set; }

        [StringLength(1)]
        public string SLIPTEMPTABLE { get; set; }

        [StringLength(1)]
        public string AUTOLOAD_QTY { get; set; }

        [StringLength(1)]
        public string POST_RETUR { get; set; }

        [StringLength(1)]
        public string METODA_TRANS_PROD { get; set; }

        public double TOLERANSI_QTY_TERIMA { get; set; }

        public double TOLERANSI_QTY_RETUR { get; set; }

        public double TOLERANSI_QTY_CLAIM { get; set; }

        [StringLength(1)]
        public string Entry_Style { get; set; }

        [StringLength(10)]
        public string LSort6 { get; set; }

        [StringLength(10)]
        public string LSort7 { get; set; }

        [StringLength(12)]
        public string LSort8 { get; set; }

        [StringLength(12)]
        public string LSort9 { get; set; }

        [StringLength(12)]
        public string LSort10 { get; set; }

        public int? ADA_GD_QC { get; set; }

        [StringLength(4)]
        public string GD_QC { get; set; }

        public int? COGS { get; set; }

        public bool? Qoh_RealTime { get; set; }

        [StringLength(1)]
        public string Validasi_Tukar { get; set; }

        public bool Create_Sort1 { get; set; }

        public bool Create_Sort2 { get; set; }

        public bool Create_Sort3 { get; set; }

        public bool Create_Sort4 { get; set; }

        public bool Create_Sort5 { get; set; }

        public bool Create_Sort6 { get; set; }

        public bool Create_Sort7 { get; set; }

        public bool Create_Sort8 { get; set; }

        public bool Create_Sort9 { get; set; }

        public bool Create_Sort10 { get; set; }

        public int? DIGIT_KODE_BRG { get; set; }

        public int? NO_KODE_BRG { get; set; }

        public bool UPDATE_STATUS_WO { get; set; }

        public int? PRINT_BRG_SAMA { get; set; }

        public int? PROMPT_BARANG { get; set; }

        public bool ProtekTglInput { get; set; }

        public int? INQUERY_STOCK { get; set; }

        [StringLength(2)]
        public string SERI_PECAH_BRG_MSK { get; set; }

        [StringLength(2)]
        public string SERI_PECAH_BRG_KLR { get; set; }

        [StringLength(2)]
        public string SERI_KONSI_BRG_KLR { get; set; }

        [StringLength(2)]
        public string SERI_KONSI_BRG_RETUR { get; set; }

        [StringLength(100)]
        public string DB_PATH { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
