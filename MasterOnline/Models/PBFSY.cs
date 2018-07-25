namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PBFSYS")]
    public partial class PBFSY
    {
        [Key]
        [StringLength(1)]
        public string ADA_PO { get; set; }

        [Required]
        [StringLength(1)]
        public string ADA_PB { get; set; }

        [Required]
        [StringLength(1)]
        public string EDIT_BRG_PO { get; set; }

        [Required]
        [StringLength(1)]
        public string EDIT_BRG_PB { get; set; }

        [Required]
        [StringLength(1)]
        public string TERIMA { get; set; }

        [Required]
        [StringLength(4)]
        public string GD { get; set; }

        [Required]
        [StringLength(1)]
        public string STN { get; set; }

        public double PPN { get; set; }

        [StringLength(3)]
        public string SERI_PI { get; set; }

        [StringLength(3)]
        public string SERI_RB { get; set; }

        [Required]
        [StringLength(1)]
        public string AUTO_NO { get; set; }

        [Required]
        [StringLength(1)]
        public string HPOKOK { get; set; }

        [Required]
        [StringLength(1)]
        public string POSTING_STATUS { get; set; }

        [Required]
        [StringLength(1)]
        public string LINK_GL { get; set; }

        [StringLength(3)]
        public string JT_BELI { get; set; }

        [StringLength(3)]
        public string JT_RETURBELI { get; set; }

        [Required]
        [StringLength(1)]
        public string METODA_LINK { get; set; }

        [Required]
        [StringLength(2)]
        public string NS_FAKTUR_PPN { get; set; }

        [Required]
        [StringLength(1)]
        public string TINGKAT_DISC { get; set; }

        [Required]
        [StringLength(1)]
        public string PPNBM { get; set; }

        public int ADA_PPH22 { get; set; }

        public double PERSEN_PPH22 { get; set; }

        public bool? Brg_Original { get; set; }

        public bool? Pengakuan_AP_Tanpa_LPB { get; set; }

        public int? STATUS_LPB { get; set; }

        public int? GIT { get; set; }

        public int? KURS_PENGAKUAN_AP { get; set; }

        public int? Inquery_Stock { get; set; }

        [StringLength(1)]
        public string MFDesimal { get; set; }

        [StringLength(1)]
        public string QFDesimal { get; set; }

        [StringLength(100)]
        public string DB_PATH { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
