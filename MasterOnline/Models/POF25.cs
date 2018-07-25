namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF25
    {
        [Key]
        [StringLength(20)]
        public string USERNAME { get; set; }

        public int? URUT { get; set; }
    }
}
