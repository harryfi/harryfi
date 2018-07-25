namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF09
    {
        [Key]
        [StringLength(5)]
        public string ALASAN { get; set; }

        [StringLength(50)]
        public string KET { get; set; }

        [StringLength(50)]
        public string KET2 { get; set; }

        public DateTime TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }
    }
}
