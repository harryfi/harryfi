namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DeliveryTemplateElevenia")]
    public partial class DeliveryTemplateElevenia
    {
        [StringLength(10)]
        public string KODE { get; set; }

        [StringLength(250)]
        public string KETERANGAN { get; set; }

        public double RECNUM_ARF01 { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Recnum { get; set; }
    }
}
