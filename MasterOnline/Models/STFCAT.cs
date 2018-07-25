namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STFCAT")]
    public partial class STFCAT
    {
        [Key]
        [StringLength(1)]
        public string JENIS_FORM { get; set; }

        [StringLength(40)]
        public string CAP_CATATAN_1 { get; set; }

        [StringLength(40)]
        public string CAP_CATATAN_2 { get; set; }

        [StringLength(40)]
        public string CAP_CATATAN_3 { get; set; }

        [StringLength(40)]
        public string CAP_CATATAN_4 { get; set; }

        [StringLength(40)]
        public string CAP_CATATAN_5 { get; set; }

        [StringLength(40)]
        public string CAP_CATATAN_6 { get; set; }

        [StringLength(40)]
        public string CAP_CATATAN_7 { get; set; }

        [StringLength(40)]
        public string CAP_CATATAN_8 { get; set; }

        [StringLength(40)]
        public string CAP_CATATAN_9 { get; set; }

        [StringLength(40)]
        public string CAP_CATATAN_10 { get; set; }
    }
}
