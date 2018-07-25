namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STFLINE")]
    public partial class STFLINE
    {
        [Key]
        [StringLength(5)]
        public string KLINE { get; set; }

        public double JLH_KARYAWAN { get; set; }
    }
}
