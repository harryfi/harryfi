namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PICKUP_POINT_BLIBLI")]
    public partial class PICKUP_POINT_BLIBLI
    {
        [StringLength(10)]
        public string KODE { get; set; }

        [StringLength(250)]
        public string KETERANGAN { get; set; }

        [StringLength(30)]
        public string MERCHANT_CODE { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Recnum { get; set; }
    }
}
