namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF02B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string Brg { get; set; }

        [Key]
        [Column(Order = 1)]
        public int No { get; set; }

        [StringLength(2)]
        public string KDHarga { get; set; }

        public double H_Besar { get; set; }

        public double H_Kecil { get; set; }

        public double H_STN3 { get; set; }

        public double H_STN4 { get; set; }

        [StringLength(50)]
        public string KET { get; set; }

        public DateTime TGLINPUT { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        [Required]
        [StringLength(5)]
        public string VLT { get; set; }

        public bool APPROVAL_HJUAL { get; set; }

        public virtual STF02 STF02 { get; set; }
    }
}
