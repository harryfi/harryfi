namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STURUT")]
    public partial class STURUT
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string Type { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(3)]
        public string Kode { get; set; }

        public int NOMOR { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
