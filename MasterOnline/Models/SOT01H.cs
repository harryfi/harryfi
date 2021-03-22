using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SOT01H
    {
        [StringLength(15)]
        public string NO_PESANAN { get; set; }

        [StringLength(70)]
        public string NO_REFERENSI { get; set; }

        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(50)]
        public string KURIR { get; set; }

        [StringLength(255)]
        public string TIPE_PENGIRIMAN { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RECNUM { get; set; }
    }
}
