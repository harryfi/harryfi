namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF22A
    {
        [Key]
        [StringLength(10)]
        public string Supp { get; set; }

        [StringLength(30)]
        public string Ket { get; set; }
    }
}
