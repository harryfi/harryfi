namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("APFSYS")]
    public partial class APFSY
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short BLN { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        public short THN { get; set; }

        public short PJ1 { get; set; }

        public short PJ2 { get; set; }

        public short PJ3 { get; set; }

        public short PS1 { get; set; }

        public short PS2 { get; set; }

        public short PS3 { get; set; }

        [StringLength(30)]
        public string KET1 { get; set; }

        [StringLength(30)]
        public string KET2 { get; set; }

        [StringLength(30)]
        public string KET3 { get; set; }

        public short LINK_GL { get; set; }

        [StringLength(1)]
        public string HARUS_LINK_KEGL { get; set; }

        [StringLength(1)]
        public string TANPA_POSTING { get; set; }

        [StringLength(1)]
        public string METODE_SELISIHKURS { get; set; }

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
        public string FORMAT_DEC_MASTER { get; set; }

        [StringLength(1)]
        public string FORMAT_DEC_DETAIL { get; set; }

        [StringLength(1)]
        public string FORMAT_DEC_CURRENCY { get; set; }

        [StringLength(1)]
        public string FORMAT_DEC_BUDGET { get; set; }

        [StringLength(1)]
        public string FORMAT_DEC_BALANCE { get; set; }

        [StringLength(1)]
        public string FORMAT_DEC_GRID { get; set; }

        [StringLength(1)]
        public string AUTO_NO { get; set; }

        [StringLength(100)]
        public string DB_PATH { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
