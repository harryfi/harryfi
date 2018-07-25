namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF14
    {
        [Key]
        [StringLength(2)]
        public string KODE { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        [StringLength(50)]
        public string KET { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        public DateTime? TGLINPUT { get; set; }
    }
}
