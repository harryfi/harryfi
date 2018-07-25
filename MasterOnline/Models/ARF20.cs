namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF20
    {
        [Key]
        [StringLength(5)]
        public string Kode_jurusan { get; set; }

        [StringLength(20)]
        public string Nama_jurusan { get; set; }
    }
}
