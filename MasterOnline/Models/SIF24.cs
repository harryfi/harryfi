namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF24
    {
        [Key]
        [StringLength(10)]
        public string KODE_KARTU { get; set; }

        [StringLength(40)]
        public string NAMA_KARTU { get; set; }
    }
}
