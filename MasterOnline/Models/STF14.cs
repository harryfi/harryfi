namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF14
    {
        [Key]
        [StringLength(30)]
        public string USERNAME { get; set; }

        public byte? btLEVEL { get; set; }
    }
}
