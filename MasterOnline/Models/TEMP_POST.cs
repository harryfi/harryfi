namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_POST
    {
        [StringLength(20)]
        public string BAHAN { get; set; }

        [StringLength(20)]
        public string BARANG { get; set; }

        [Key]
        public double QTY { get; set; }
    }
}
