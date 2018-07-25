namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF15
    {
        [Key]
        [StringLength(5)]
        public string BIAYA { get; set; }

        [StringLength(40)]
        public string KETERANGAN { get; set; }
    }
}
