namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBF05
    {
        [Key]
        [StringLength(1)]
        public string JENIS_FORM { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_1 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_2 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_3 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_4 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_5 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_6 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_7 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_8 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_9 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_10 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_11 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_12 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_13 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_14 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_15 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_16 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_17 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_18 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_19 { get; set; }

        [Required]
        [StringLength(40)]
        public string CAT_CAPTION_20 { get; set; }

        public bool CAT_ENABLE_1 { get; set; }

        public bool CAT_ENABLE_2 { get; set; }

        public bool CAT_ENABLE_3 { get; set; }

        public bool CAT_ENABLE_4 { get; set; }

        public bool CAT_ENABLE_5 { get; set; }

        public bool CAT_ENABLE_6 { get; set; }

        public bool CAT_ENABLE_7 { get; set; }

        public bool CAT_ENABLE_8 { get; set; }

        public bool CAT_ENABLE_9 { get; set; }

        public bool CAT_ENABLE_10 { get; set; }

        public bool CAT_ENABLE_11 { get; set; }

        public bool CAT_ENABLE_12 { get; set; }

        public bool CAT_ENABLE_13 { get; set; }

        public bool CAT_ENABLE_14 { get; set; }

        public bool CAT_ENABLE_15 { get; set; }

        public bool CAT_ENABLE_16 { get; set; }

        public bool CAT_ENABLE_17 { get; set; }

        public bool CAT_ENABLE_18 { get; set; }

        public bool CAT_ENABLE_19 { get; set; }

        public bool CAT_ENABLE_20 { get; set; }

        public bool CAT_NUMERIC_1 { get; set; }

        public bool CAT_NUMERIC_2 { get; set; }

        public bool CAT_NUMERIC_3 { get; set; }

        public bool CAT_NUMERIC_4 { get; set; }

        public bool CAT_NUMERIC_5 { get; set; }

        public bool CAT_NUMERIC_6 { get; set; }

        public bool CAT_NUMERIC_7 { get; set; }

        public bool CAT_NUMERIC_8 { get; set; }

        public bool CAT_NUMERIC_9 { get; set; }

        public bool CAT_NUMERIC_10 { get; set; }

        public bool CAT_NUMERIC_11 { get; set; }

        public bool CAT_NUMERIC_12 { get; set; }

        public bool CAT_NUMERIC_13 { get; set; }

        public bool CAT_NUMERIC_14 { get; set; }

        public bool CAT_NUMERIC_15 { get; set; }

        public bool CAT_NUMERIC_16 { get; set; }

        public bool CAT_NUMERIC_17 { get; set; }

        public bool CAT_NUMERIC_18 { get; set; }

        public bool CAT_NUMERIC_19 { get; set; }

        public bool CAT_NUMERIC_20 { get; set; }
    }
}
