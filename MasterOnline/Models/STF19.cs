namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF19
    {
        [Key]
        [StringLength(20)]
        public string BRG { get; set; }

        [StringLength(255)]
        public string NAMA { get; set; }

        public double? HPP { get; set; }
    }
}
