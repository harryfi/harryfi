namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class APF05A
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string Supp { get; set; }

        [Key]
        [Column(Order = 1, TypeName = "smalldatetime")]
        public DateTime Tgl { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string Inv { get; set; }

        [Key]
        [Column(Order = 3)]
        public int RecNum { get; set; }

        [StringLength(1)]
        public string DK { get; set; }

        [Column(TypeName = "ntext")]
        public string Ket { get; set; }

        public double Nilai { get; set; }

        [StringLength(3)]
        public string Vlt { get; set; }

        public double Tukar { get; set; }

        public int? DetailNoUrut { get; set; }
    }
}
