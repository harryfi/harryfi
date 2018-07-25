namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF10
    {
        [Key]
        [StringLength(5)]
        public string KODE_BUSINESS_LINE { get; set; }

        [StringLength(40)]
        public string KET { get; set; }

        public DateTime TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }
    }
}
