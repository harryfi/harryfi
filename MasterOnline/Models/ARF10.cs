namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF10
    {
        [Key]
        [StringLength(5)]
        public string WIL { get; set; }

        [StringLength(30)]
        public string NAWIL { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
