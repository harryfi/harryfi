namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_PBT01C
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENISFORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string INV { get; set; }

        [Key]
        [Column(Order = 2)]
        public int NO { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(10)]
        public string NOBUK { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(20)]
        public string USERNAME { get; set; }

        [Key]
        [Column(Order = 5)]
        public DateTime TGLINPUT { get; set; }
    }
}
