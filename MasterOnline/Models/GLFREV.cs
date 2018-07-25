namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFREV")]
    public partial class GLFREV
    {
        [StringLength(5)]
        public string KURS { get; set; }

        public short? LKS { get; set; }

        [StringLength(15)]
        public string REK { get; set; }

        public short BLN { get; set; }

        public double TUKAR1 { get; set; }

        public double TUKAR2 { get; set; }

        public double SALDO { get; set; }

        [StringLength(1)]
        public string FlagSaldoAwal { get; set; }

        [Key]
        public int RECNUM { get; set; }
    }
}
