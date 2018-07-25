namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF19
    {
        [Key]
        [StringLength(10)]
        public string SHIPPER { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }
    }
}
