namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF17B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string SUPP { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string BRG { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(20)]
        public string BRG_SUPP { get; set; }

        [StringLength(30)]
        public string NAMA_BRG_SUPP { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        public DateTime? TGLINPUT { get; set; }
    }
}
