namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF16B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(3)]
        public string KDHARGA { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime TGL1 { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime TGL2 { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(20)]
        public string BRG { get; set; }

        public double H_Besar { get; set; }

        public double H_Kecil { get; set; }

        public double? H_STN3 { get; set; }

        public double? H_STN4 { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
