namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF20
    {
        [Key]
        [StringLength(10)]
        public string SUPP { get; set; }

        public double? HARGA_M3 { get; set; }

        public double? HARGA_KG { get; set; }
    }
}
