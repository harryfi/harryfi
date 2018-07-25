namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIFSYS_DS
    {
        [Key]
        public bool MetodaHargaSatuan { get; set; }

        public bool PrintPriceList { get; set; }

        public bool KontrolKirim { get; set; }

        public DateTime TanggalHariIni { get; set; }

        [StringLength(1)]
        public string Penomoran { get; set; }

        [Required]
        [StringLength(2)]
        public string PrefixOrder { get; set; }

        [Required]
        [StringLength(2)]
        public string PrefixFaktur { get; set; }

        public int LastOrder { get; set; }

        public int LastFaktur { get; set; }

        [StringLength(15)]
        public string RekGLCC { get; set; }

        [StringLength(15)]
        public string RekGLDC { get; set; }

        [StringLength(15)]
        public string RekGLTC { get; set; }

        [StringLength(2)]
        public string PrefixPemb { get; set; }

        [StringLength(15)]
        public string RekReturAR { get; set; }

        public int? PostingKB { get; set; }

        [StringLength(2)]
        public string No_Seri_Kas { get; set; }

        [StringLength(2)]
        public string No_Seri_GM { get; set; }

        [StringLength(15)]
        public string RekGiroMundur { get; set; }

        [StringLength(15)]
        public string RekDisc { get; set; }

        [StringLength(15)]
        public string RekSelisihBayar { get; set; }

        public int Metode_UMuka { get; set; }

        [StringLength(15)]
        public string RekPembyranSementara { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
