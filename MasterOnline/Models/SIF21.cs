namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF21
    {
        [Key]
        [StringLength(10)]
        public string KODE { get; set; }

        public double? TOLERANSI { get; set; }
    }
}
