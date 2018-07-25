namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF05
    {
        [Key]
        [StringLength(10)]
        public string BUYER_CODE { get; set; }

        [StringLength(30)]
        public string BUYER_NAME { get; set; }

        public DateTime? TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }
    }
}
