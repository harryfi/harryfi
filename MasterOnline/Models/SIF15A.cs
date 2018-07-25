namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF15A
    {
        [Key]
        [StringLength(5)]
        public string GRUP { get; set; }

        [StringLength(40)]
        public string KET { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
