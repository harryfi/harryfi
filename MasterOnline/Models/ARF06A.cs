namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF06A
    {
        [Key]
        [StringLength(10)]
        public string KodeGroup { get; set; }

        [StringLength(30)]
        public string NamaGroup { get; set; }

        [StringLength(30)]
        public string Ket { get; set; }

        [StringLength(3)]
        public string Val { get; set; }

        public double? Limit { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
