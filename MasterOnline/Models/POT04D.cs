namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT04D
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string PO_NO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(5)]
        public string BIAYA { get; set; }

        public double NILAI { get; set; }

        [Required]
        [StringLength(50)]
        public string CATATAN { get; set; }

        [Required]
        [StringLength(1)]
        public string JENIS_HITUNG { get; set; }

        public double? Nilai_Estimasi { get; set; }

        [StringLength(255)]
        public string CATATAN2 { get; set; }

        [StringLength(255)]
        public string CATATAN3 { get; set; }

        [StringLength(255)]
        public string CATATAN4 { get; set; }

        [StringLength(255)]
        public string CATATAN5 { get; set; }

        [StringLength(255)]
        public string CATATAN6 { get; set; }

        [StringLength(255)]
        public string CATATAN7 { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
