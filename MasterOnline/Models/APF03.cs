namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class APF03
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string SUPP { get; set; }

        [Key]
        [Column(Order = 1, TypeName = "smalldatetime")]
        public DateTime TGL { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string BUKTI { get; set; }

        [StringLength(1)]
        public string DK { get; set; }

        [Column(TypeName = "ntext")]
        public string KET { get; set; }

        public double NILAI { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        public double TUKAR { get; set; }

        [StringLength(2)]
        public string JTRAN { get; set; }

        [StringLength(10)]
        public string INV { get; set; }

        [Key]
        [Column(Order = 3)]
        public int RECNUM { get; set; }
    }
}
