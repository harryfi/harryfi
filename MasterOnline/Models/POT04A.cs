namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT04A
    {
        [Key]
        [StringLength(10)]
        public string PO_NO { get; set; }

        public double BIAYAPERPO { get; set; }

        [Required]
        [StringLength(255)]
        public string CATATAN { get; set; }

        [Required]
        [StringLength(10)]
        public string STATUS { get; set; }

        public double BIAYAITEM { get; set; }

        [Required]
        [StringLength(1)]
        public string JENIS_HITUNG { get; set; }

        public DateTime? TGL { get; set; }

        public bool? PROSES_LPB { get; set; }

        public bool? AutoLoadPO { get; set; }

        public double? Nilai_Tukar { get; set; }

        [StringLength(255)]
        public string Catatan2 { get; set; }

        [StringLength(255)]
        public string Catatan3 { get; set; }

        [StringLength(255)]
        public string Catatan4 { get; set; }

        [StringLength(255)]
        public string Catatan5 { get; set; }

        [StringLength(255)]
        public string Catatan6 { get; set; }

        [StringLength(255)]
        public string Catatan7 { get; set; }

        [StringLength(30)]
        public string NO_LPB { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
