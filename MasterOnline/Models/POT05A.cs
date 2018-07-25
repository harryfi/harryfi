namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT05A
    {
        [Key]
        [StringLength(10)]
        public string NO_PENAWARAN { get; set; }

        [StringLength(10)]
        public string GROUP_BRG { get; set; }

        [StringLength(10)]
        public string KD_DIV { get; set; }

        [Column(TypeName = "text")]
        public string HEADER { get; set; }

        public DateTime? TGL_INPUT { get; set; }

        [Column(TypeName = "text")]
        public string FOOTER { get; set; }

        [StringLength(10)]
        public string BUYER { get; set; }

        [StringLength(1)]
        public string STN_QR { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
