namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBT03A
    {
        [StringLength(20)]
        public string NOBUK { get; set; }

        [Key]
        [Column(Order = 0)]
        public DateTime TGL { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string SUPP { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(30)]
        public string NO_PO { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(1)]
        public string POSTING { get; set; }
    }
}
