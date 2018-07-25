namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT04A
    {
        [Key]
        [StringLength(4)]
        public string GUD { get; set; }

        [StringLength(30)]
        public string NAMA_GUDANG { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
