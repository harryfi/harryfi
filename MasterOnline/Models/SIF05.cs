namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF05
    {
        [Key]
        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(70)]
        public string NAMA_CUST { get; set; }

        [StringLength(40)]
        public string AL { get; set; }

        [StringLength(40)]
        public string AL1 { get; set; }

        [StringLength(40)]
        public string AL2 { get; set; }

        [StringLength(40)]
        public string AL3 { get; set; }

        [StringLength(10)]
        public string CUST_PPN { get; set; }

        public bool PET { get; set; }

        [StringLength(5)]
        public string KODE_BUSINESS_LINE { get; set; }

        [StringLength(50)]
        public string CAT_1 { get; set; }

        [StringLength(50)]
        public string CAT_2 { get; set; }

        [StringLength(50)]
        public string CAT_3 { get; set; }

        public DateTime? DR_TGL { get; set; }

        public DateTime? SD_TGL { get; set; }

        [StringLength(1)]
        public string TBL_HJUAL2 { get; set; }

        [StringLength(1)]
        public string SET_NOFPAJAK { get; set; }

        [StringLength(1)]
        public string SET_TGLFPAJAK { get; set; }

        public DateTime TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        [StringLength(1)]
        public string SET_TGLJTTEMPO { get; set; }

        [StringLength(1)]
        public string METODA_HJUAL { get; set; }

        [Required]
        [StringLength(10)]
        public string KODE_PRINCIPAL { get; set; }
    }
}
