namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DELIVERY_PROVIDER_LAZADA
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string CUST { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(250)]
        public string NAME { get; set; }

        [StringLength(1)]
        public string COD { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RECNUM { get; set; }
    }
}
