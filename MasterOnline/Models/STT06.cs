namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT06
    {
        [Key]
        [StringLength(10)]
        public string Kode_Biaya { get; set; }

        [StringLength(40)]
        public string Keterangan { get; set; }

        [StringLength(10)]
        public string DEPT { get; set; }

        [StringLength(10)]
        public string CC { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
