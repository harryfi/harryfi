namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ARURUT")]
    public partial class ARURUT
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string TYPE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(3)]
        public string KODE { get; set; }

        public double NOMOR { get; set; }

        [StringLength(225)]
        public string Ket { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
