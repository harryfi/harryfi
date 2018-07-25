namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("APURUT")]
    public partial class APURUT
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string TYPE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(3)]
        public string KODE { get; set; }

        public short NOMOR { get; set; }

        [StringLength(225)]
        public string Ket { get; set; }

        public int Tahun { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
