namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF19
    {
        [Key]
        [StringLength(5)]
        public string Kode_MTK { get; set; }

        [StringLength(20)]
        public string Nama_MTK { get; set; }

        public int? SKS { get; set; }
    }
}
