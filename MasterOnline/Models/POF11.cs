namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF11
    {
        [Key]
        [StringLength(10)]
        public string SUPP { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }
    }
}
