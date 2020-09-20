namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LINKFTP")]
    public partial class LINKFTP
    {
        [Key]
        [StringLength(10)]
        public string IP { get; set; }

        [StringLength(50)]
        public string LOGIN { get; set; }

        [StringLength(20)]
        public string PASSWORD { get; set; }

        public string STATUS_FTP { get; set; }
        public string PPN { get; set; }

        public TimeSpan? JAM1 { get; set; }
        public TimeSpan? JAM2 { get; set; }
        public TimeSpan? JAM3 { get; set; }
        public TimeSpan? JAM4 { get; set; }
        public TimeSpan? JAM5 { get; set; }
    }
}