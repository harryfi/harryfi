namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFNER1
    {
        [Key]
        [StringLength(2)]
        public string KODE { get; set; }

        [StringLength(40)]
        public string JUDUL1 { get; set; }

        [StringLength(40)]
        public string JUDUL2 { get; set; }

        public short LKS { get; set; }

        [StringLength(10)]
        public string DEPT { get; set; }

        [StringLength(2)]
        public string KODE_BARIS { get; set; }

        [StringLength(2)]
        public string KODE_KOLOM { get; set; }

        [StringLength(1)]
        public string CNILAI { get; set; }
    }
}
