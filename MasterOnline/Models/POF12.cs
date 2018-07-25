namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF12
    {
        [Key]
        [StringLength(10)]
        public string SUPP { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(1)]
        public string JENIS { get; set; }

        public DateTime? DR_TGL { get; set; }

        public DateTime? SD_TGL { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
