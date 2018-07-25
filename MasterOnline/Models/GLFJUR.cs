namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFJUR")]
    public partial class GLFJUR
    {
        [Key]
        [StringLength(2)]
        public string jurnal { get; set; }

        [StringLength(30)]
        public string njurnal { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
