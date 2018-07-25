namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF22
    {
        [Key]
        [StringLength(10)]
        public string KODE { get; set; }

        [MaxLength(50)]
        public byte[] PROPINSI { get; set; }
    }
}
