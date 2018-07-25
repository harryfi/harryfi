namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF02
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string TYPE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(3)]
        public string KODE { get; set; }

        public int NOMOR { get; set; }

        public DateTime TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }
    }
}
