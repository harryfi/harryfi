namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class APT04A
    {
        [Key]
        [StringLength(20)]
        public string No_SPK { get; set; }

        public DateTime? Tgl_SPK { get; set; }

        public DateTime? Dr_PKontrak { get; set; }

        public DateTime? Sd_PKontrak { get; set; }

        [StringLength(10)]
        public string Supp { get; set; }

        [StringLength(255)]
        public string Ket { get; set; }

        public double? PPN { get; set; }

        public double? PPH { get; set; }

        public double? Total_Kontrak { get; set; }

        public double? Term_Pemb { get; set; }
    }
}
